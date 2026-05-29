using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NSIE.Models.ProyectosPrivados;
using CatTecnologia = NSIE.Models.ProyectosPrivados.Core.CatTecnologia;
using GrupoInteresEconomico = NSIE.Models.ProyectosPrivados.Core.GrupoInteresEconomico;
using Proyecto = NSIE.Models.ProyectosPrivados.Core.Proyecto;
using Actor = NSIE.Models.ProyectosPrivados.Core.Actor;

namespace NSIE.Servicios
{
    public class IngestionService
    {
        private readonly string _conn;

        public IngestionService(IConfiguration config)
        {
            _conn = config.GetConnectionString("DefaultConnection")!;
            try
            {
                using var db = new SqlConnection(_conn);
                db.Execute(@"
                    IF NOT EXISTS (
                        SELECT * FROM sys.columns 
                        WHERE object_id = OBJECT_ID('staging.ExcelRaw_VUPEWorksheet') 
                        AND name = 'MatchScore'
                    )
                    BEGIN
                        ALTER TABLE staging.ExcelRaw_VUPEWorksheet ADD MatchScore NVARCHAR(MAX) NULL;
                    END
                ");
            }
            catch
            {
                // Ignorar error de base de datos en inicialización
            }
        }

        private IDbConnection Connection => new SqlConnection(_conn);

        /* ==========================================
           1. VOLCADO RAW DESDE EXCEL A STAGING
           ========================================== */

        public async Task<IngestionResultDto> IngestarVUPEWorksheetAsync(Stream stream, string filename, string usuario)
        {
            var result = new IngestionResultDto { ArchivoNombre = filename };
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var usedRange = worksheet.RangeUsed();
            if (usedRange == null)
            {
                result.MensajesLog.Add("La hoja está vacía.");
                return result;
            }

            var lastRow = usedRange.LastRowUsed().RowNumber();
            result.TotalFilasProcesadas = lastRow - 1; // Menos el header

            using var db = Connection;
            if (db.State != ConnectionState.Open) db.Open();

            // Limpiar staging anterior
            await db.ExecuteAsync("TRUNCATE TABLE staging.ExcelRaw_VUPEWorksheet");

            // Volcado a staging
            var stagingList = new List<object>();
            for (int r = 2; r <= lastRow; r++)
            {
                var row = worksheet.Row(r);
                var id = row.Cell(1).GetFormattedString().Trim();
                var preFolio = row.Cell(2).GetFormattedString().Trim();
                var folio = row.Cell(3).GetFormattedString().Trim();
                var rfc = row.Cell(4).GetFormattedString().Trim();
                var nombre = row.Cell(5).GetFormattedString().Trim();
                var proyecto = row.Cell(6).GetFormattedString().Trim();
                var descripcion = row.Cell(7).GetFormattedString().Trim();
                var Gie = row.Cell(8).GetFormattedString().Trim();
                var esHibrida = row.Cell(9).GetFormattedString().Trim();
                var tipoTecnologia = row.Cell(10).GetFormattedString().Trim();

                if (string.IsNullOrWhiteSpace(proyecto)) continue;

                stagingList.Add(new { id, preFolio, folio, rfc, nombre, proyecto, descripcion, Gie, esHibrida, tipoTecnologia });
            }

            if (stagingList.Any())
            {
                await db.ExecuteAsync(@"
                    INSERT INTO staging.ExcelRaw_VUPEWorksheet (
                        IDRaw, PreFolioRaw, FolioProyectoRaw, RFCRaw, NombreRaw, ProyectoRaw, DescripcionProyectoRaw, GrupoInteresRaw, EsHibridaRaw, TipoTecnologiaRaw
                    ) VALUES (
                        @id, @preFolio, @folio, @rfc, @nombre, @proyecto, @descripcion, @Gie, @esHibrida, @tipoTecnologia
                    )", stagingList);
            }

            result.MensajesLog.Add($"Cargados {result.TotalFilasProcesadas} registros en staging.ExcelRaw_VUPEWorksheet.");

            // Procesar deduplicación y homologación hacia core.*
            await ProcesarDeduplicacionVUPEAsync(db, filename, usuario, result);

            return result;
        }

        /* ==========================================
           2. MOTOR DE DEDUPLICACIÓN Y HOMOLOGACIÓN
           ========================================== */

        private async Task ProcesarDeduplicacionVUPEAsync(IDbConnection db, string filename, string usuario, IngestionResultDto result)
        {
            var stagingRecords = (await db.QueryAsync(@"
                SELECT * FROM staging.ExcelRaw_VUPEWorksheet
            ")).ToList();

            // Cargar catálogos actuales en memoria
            var tecnologias = (await db.QueryAsync<CatTecnologia>("SELECT * FROM core.CatTecnologia")).ToList();
            var gies = (await db.QueryAsync<GrupoInteresEconomico>("SELECT * FROM core.GrupoInteresEconomico")).ToList();
            var proyectosExistentes = (await db.QueryAsync<Proyecto>(@"
                SELECT p.*, dt.CapacidadInstaladaMW,
                       (SELECT TOP 1 u.Latitud FROM core.ProyectoCoordenada u WHERE u.ProyectoId = p.ProyectoId) AS Latitud,
                       (SELECT TOP 1 u.Longitud FROM core.ProyectoCoordenada u WHERE u.ProyectoId = p.ProyectoId) AS Longitud
                FROM core.Proyecto p
                LEFT JOIN core.ProyectoDatosTecnicos dt ON dt.ProyectoId = p.ProyectoId
                WHERE p.Activo = 1
            ")).ToList();

            // Cargar mapas en memoria para evitar consultas N^2
            var identMap = (await db.QueryAsync<(string ClaveExterna, int ProyectoId)>(@"
                SELECT ClaveExterna, ProyectoId FROM core.ProyectoIdentificador
            ")).ToDictionary(x => x.ClaveExterna, x => x.ProyectoId, StringComparer.OrdinalIgnoreCase);

            var rfcMap = (await db.QueryAsync<(int ProyectoId, string RFC)>(@"
                SELECT pa.ProyectoId, a.RFC
                FROM core.ProyectoActor pa
                JOIN core.Actor a ON a.ActorId = pa.ActorId
                WHERE pa.TipoActorId = 1
            ")).GroupBy(x => x.ProyectoId).ToDictionary(g => g.Key, g => g.First().RFC);

            var actorMap = (await db.QueryAsync<(string RazonSocial, int ActorId)>(@"
                SELECT RazonSocial, ActorId FROM core.Actor WHERE Activo = 1
            ")).GroupBy(x => x.RazonSocial).ToDictionary(g => g.Key, g => g.First().ActorId, StringComparer.OrdinalIgnoreCase);

            foreach (var stg in stagingRecords)
            {
                string rawProjName = stg.ProyectoRaw;
                string rawRfc = stg.RFCRaw;
                string rawFolio = stg.FolioProyectoRaw;
                string rawTech = stg.TipoTecnologiaRaw;
                string rawGie = stg.GrupoInteresRaw;
                string rawNombre = stg.NombreRaw;
                int stagingId = stg.StagingId;
                string esHibridaRaw = stg.EsHibridaRaw;

                // 1. Normalizar nombre del proyecto
                string normProjName = NormalizeProjectName(rawProjName);

                // 2. Intentar buscar coincidencia (Deduplicación)
                Proyecto matchProyecto = null;
                double bestScore = 0.0;

                // A. Buscar coincidencia exacta por Folio de Proyecto o Clave externa
                if (!string.IsNullOrWhiteSpace(rawFolio))
                {
                    if (identMap.TryGetValue(rawFolio, out var existId))
                    {
                        matchProyecto = proyectosExistentes.FirstOrDefault(p => p.ProyectoId == existId);
                        if (matchProyecto != null)
                        {
                            bestScore = 1.0; // 100% de coincidencia
                        }
                    }
                }

                // B. Si no hay coincidencia exacta de Folio, aplicar Matriz de Scoring
                if (matchProyecto == null && proyectosExistentes.Any())
                {
                    foreach (var exist in proyectosExistentes)
                    {
                        double score = 0.0;

                        // Similitud de nombre por Jaro-Winkler (60%)
                        double nameSim = JaroWinkler(normProjName, NormalizeProjectName(exist.NombreOficial));
                        if (nameSim >= 0.88)
                        {
                            score += nameSim * 0.60;
                        }

                        // Mismo RFC / Empresa (si coincide RFC del promovente)
                        if (!string.IsNullOrWhiteSpace(rawRfc))
                        {
                            if (rfcMap.TryGetValue(exist.ProyectoId, out var existRfc) &&
                                string.Equals(rawRfc, existRfc, StringComparison.OrdinalIgnoreCase))
                            {
                                score += 0.20; // Añadir peso por RFC coincidente
                            }
                        }

                        // Tecnología coincidente (15%)
                        var techExist = tecnologias.FirstOrDefault(t => t.TecnologiaId == exist.TecnologiaId)?.Nombre;
                        if (MatchTechnology(rawTech, techExist))
                        {
                            score += 0.15;
                        }

                        if (score > bestScore)
                        {
                            bestScore = score;
                            matchProyecto = exist;
                        }
                    }
                }

                // C. Determinar acciones según el Score
                double finalPercentage = bestScore * 100;
                if (finalPercentage >= 90.0 && matchProyecto != null)
                {
                    // FUSIÓN AUTOMÁTICA (Match Firme)
                    // Añadir identificador
                    try
                    {
                        string extKey = rawFolio ?? $"VUPE-{stagingId}";
                        await db.ExecuteAsync(@"
                            INSERT INTO core.ProyectoIdentificador (ProyectoId, OrigenDatosId, ClaveExterna, NombreEnOrigen)
                            VALUES (@ProyectoId, 1, @ClaveExterna, @NombreEnOrigen)
                        ", new { 
                            ProyectoId = matchProyecto.ProyectoId, 
                            ClaveExterna = extKey, 
                            NombreEnOrigen = rawProjName 
                        });
                        identMap[extKey] = matchProyecto.ProyectoId;
                        result.IdentificadoresAsociados++;
                    }
                    catch
                    {
                        // Ya existía la clave externa
                    }

                    // Registrar bitácora de carga
                    await RegistrarBitacoraCargaAsync(db, matchProyecto.ProyectoId, 1, filename, "Worksheet", stagingId, usuario, stg);
                }
                else if (finalPercentage >= 70.0 && finalPercentage < 90.0 && matchProyecto != null)
                {
                    // COLA DE RESOLUCIÓN MANUAL (Match Dudoso)
                    // Marcamos la fila en staging como conflictiva para la UI, sin sobreescribir NombreRaw
                    await db.ExecuteAsync(@"
                        UPDATE staging.ExcelRaw_VUPEWorksheet 
                        SET IDRaw = @ProyectoIdCanonico, PreFolioRaw = 'CONFLICTO', MatchScore = @MatchScore
                        WHERE StagingId = @StagingId
                    ", new { 
                        ProyectoIdCanonico = matchProyecto.ProyectoId.ToString(), 
                        MatchScore = finalPercentage.ToString("F1"), 
                        StagingId = stagingId 
                    });
                    result.ConflictosDetectados++;
                }
                else
                {
                    // PROYECTO NUEVO (Score < 70%)
                    // 1. Homologar tecnología
                    var techId = ResolveTechnology(rawTech, tecnologias, db);

                    // 2. Insertar Proyecto Maestro
                    var pId = await db.ExecuteScalarAsync<int>(@"
                        INSERT INTO core.Proyecto (
                            Status, NombreOficial, NombreCorto, TecnologiaId, EstatusProyectoId, NivelMadurezId, Activo, CreadoEn, CreadoPor
                        ) VALUES (
                            'Ventanilla', @NombreOficial, @NombreCorto, @TecnologiaId, 3, 1, 1, GETDATE(), @Usuario
                        );
                        SELECT SCOPE_IDENTITY();
                    ", new { 
                        NombreOficial = rawProjName, 
                        NombreCorto = normProjName, 
                        TecnologiaId = techId, 
                        Usuario = usuario 
                    });

                    // 3. Insertar Identificador
                    string extKey = rawFolio ?? $"VUPE-{stagingId}";
                    await db.ExecuteAsync(@"
                        INSERT INTO core.ProyectoIdentificador (ProyectoId, OrigenDatosId, ClaveExterna, NombreEnOrigen)
                        VALUES (@ProyectoId, 1, @ClaveExterna, @NombreEnOrigen)
                    ", new { 
                        ProyectoId = pId, 
                        ClaveExterna = extKey, 
                        NombreEnOrigen = rawProjName 
                    });
                    identMap[extKey] = pId;

                    // 4. Crear Actor / Empresa (si no existe) y asociarlo
                    int? actorId = null;
                    if (!string.IsNullOrWhiteSpace(rawNombre))
                    {
                        if (actorMap.TryGetValue(rawNombre, out var existActorId))
                        {
                            actorId = existActorId;
                        }
                        else
                        {
                            int? gieId = null;
                            if (!string.IsNullOrWhiteSpace(rawGie))
                            {
                                gieId = gies.FirstOrDefault(g => string.Equals(g.Nombre, rawGie, StringComparison.OrdinalIgnoreCase))?.GrupoEconomicoId;
                                if (!gieId.HasValue)
                                {
                                    gieId = await db.ExecuteScalarAsync<int>(@"
                                        INSERT INTO core.GrupoInteresEconomico (Nombre) VALUES (@rawGie);
                                        SELECT SCOPE_IDENTITY();
                                    ", new { rawGie });
                                    gies.Add(new GrupoInteresEconomico { GrupoEconomicoId = gieId.Value, Nombre = rawGie });
                                }
                            }

                            actorId = await db.ExecuteScalarAsync<int>(@"
                                INSERT INTO core.Actor (RazonSocial, RFC, GrupoEconomicoId, Activo)
                                VALUES (@RazonSocial, @RFC, @gieId, 1);
                                SELECT SCOPE_IDENTITY();
                            ", new { RazonSocial = rawNombre, RFC = rawRfc, gieId });
                            
                            actorMap[rawNombre] = actorId.Value;
                        }

                        // Relación Proyecto - Actor (Promovente)
                        await db.ExecuteAsync(@"
                            INSERT INTO core.ProyectoActor (ProyectoId, ActorId, TipoActorId)
                            VALUES (@ProyectoId, @ActorId, 1)
                        ", new { ProyectoId = pId, ActorId = actorId.Value });

                        if (!string.IsNullOrWhiteSpace(rawRfc))
                        {
                            rfcMap[pId] = rawRfc;
                        }
                    }

                    // 5. Ubicación (Yucatán / Quintana Roo etc)
                    // Mapear por defecto a No Especificado (Id = 99)
                    await db.ExecuteAsync(@"
                        INSERT INTO core.ProyectoUbicacion (ProyectoId, EntidadFederativaId, MunicipioId, EsPrincipal)
                        VALUES (@ProyectoId, 99, 99001, 1)
                    ", new { ProyectoId = pId });

                    // 6. Datos técnicos y almacenamiento BESS inicial
                    bool storage = string.Equals(esHibridaRaw, "SI", StringComparison.OrdinalIgnoreCase);
                    await db.ExecuteAsync(@"
                        INSERT INTO core.ProyectoDatosTecnicos (ProyectoId, CapacidadInstaladaMW, AlmacenamientoBess, EsHibrido)
                        VALUES (@ProyectoId, 0.0, @storage, @storage)
                    ", new { ProyectoId = pId, storage });

                    // 7. Datos financieros iniciales
                    await db.ExecuteAsync(@"
                        INSERT INTO core.ProyectoDatosFinancieros (ProyectoId, MonedaId)
                        VALUES (@ProyectoId, 1)
                    ", new { ProyectoId = pId });

                    // Registrar bitácora de carga
                    await RegistrarBitacoraCargaAsync(db, pId, 1, filename, "Worksheet", stagingId, usuario, stg);

                    result.NuevosProyectosInsertados++;
                    
                    // Añadir a existentes para siguientes iteraciones de este bucle
                    proyectosExistentes.Add(new Proyecto { 
                        ProyectoId = pId, 
                        NombreOficial = rawProjName, 
                        TecnologiaId = techId 
                    });
                }
            }
        }

        /* ==========================================
           3. ALGORITMOS DE AYUDA Y NORMALIZACIÓN
           ========================================== */

        public static string NormalizeProjectName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;

            // 1. Mayúsculas y quitar acentos
            string normalized = name.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (char c in normalized)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(char.ToUpperInvariant(c));
                }
            }
            string text = sb.ToString();

            // 2. Remover tipos de sociedad mercantiles (ej. S.A. de C.V.)
            text = Regex.Replace(text, @"\b(S\s*A\b|S\s*A\s*D\s*E\s*C\s*V|S\s*A\s*P\s*I|S\s*D\s*E\s*R\s*L|C\s*V|S\s*C|L\s*T\s*D|INC|CORP)\b", "", RegexOptions.IgnoreCase);

            // 3. Remover palabras comunes operativas
            // PRESERVAMOS numerales y caracteres aislados al final (I, II, III, IV, A, B, 1, 2)
            text = Regex.Replace(text, @"\b(PROYECTO|PARQUE|CENTRAL|FOTOVOLTAICO|FV|EOLICO|EOLICA|SOLAR|GENERACION)\b", "", RegexOptions.IgnoreCase);

            // 4. Limpiar espacios extra
            text = Regex.Replace(text, @"\s+", " ").Trim();

            return text;
        }

        private static bool MatchTechnology(string rawTech, string? existTech)
        {
            if (string.IsNullOrWhiteSpace(rawTech) || string.IsNullOrWhiteSpace(existTech)) return false;
            string t1 = NormalizeProjectName(rawTech);
            string t2 = NormalizeProjectName(existTech);
            return t1.Contains(t2) || t2.Contains(t1);
        }

        private static int? ResolveTechnology(string rawTech, List<CatTecnologia> tecnologias, IDbConnection db)
        {
            if (string.IsNullOrWhiteSpace(rawTech)) return null;
            string key = rawTech.ToUpper().Trim();
            
            var match = tecnologias.FirstOrDefault(t => string.Equals(t.Nombre, key, StringComparison.OrdinalIgnoreCase) ||
                                                        t.Nombre.ToUpper().Contains(key) ||
                                                        key.Contains(t.Nombre.ToUpper()));
            if (match != null) return match.TecnologiaId;

            // Insertar nueva tecnología
            try
            {
                var id = db.ExecuteScalar<int>(@"
                    INSERT INTO core.CatTecnologia (Nombre, EsRenovable, Activo) 
                    VALUES (@rawTech, 1, 1);
                    SELECT SCOPE_IDENTITY();
                ", new { rawTech });

                tecnologias.Add(new CatTecnologia { TecnologiaId = id, Nombre = rawTech });
                return id;
            }
            catch
            {
                return db.QueryFirstOrDefault<int?>("SELECT TecnologiaId FROM core.CatTecnologia WHERE Nombre = @rawTech", new { rawTech });
            }
        }

        private static async Task RegistrarBitacoraCargaAsync(IDbConnection db, int proyectoId, int origenDatosId, string archivo, string hoja, int fila, string usuario, dynamic stg)
        {
            string rawJson = JsonConvert.SerializeObject(stg);
            string hash = ComputeSha256(rawJson);

            await db.ExecuteAsync(@"
                INSERT INTO core.ProyectoBitacoraCarga (
                    ProyectoId, OrigenDatosId, ArchivoOrigen, HojaOrigen, FilaOrigen, FechaCarga, UsuarioCarga, HashDeduplicacion, DatosRawJson, EstatusValidacion
                ) VALUES (
                    @proyectoId, @origenDatosId, @archivo, @hoja, @fila, GETDATE(), @usuario, @hash, @rawJson, 'Aprobado'
                )", new { proyectoId, origenDatosId, archivo, hoja, fila, usuario, hash, rawJson });
        }

        private static string ComputeSha256(string raw)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
            var sb = new StringBuilder();
            foreach (var b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

        /* ==========================================
           4. ALGORITMO JARO-WINKLER SIMILITUD
           ========================================== */

        public static double JaroWinkler(string s1, string s2)
        {
            double jaro = Jaro(s1, s2);
            if (jaro < 0.7) return jaro;

            int prefixLength = 0;
            for (int i = 0; i < Math.Min(4, Math.Min(s1.Length, s2.Length)); i++)
            {
                if (s1[i] == s2[i]) prefixLength++;
                else break;
            }
            return jaro + (prefixLength * 0.1 * (1.0 - jaro));
        }

        private static double Jaro(string s1, string s2)
        {
            int len1 = s1.Length;
            int len2 = s2.Length;
            if (len1 == 0 && len2 == 0) return 1.0;
            if (len1 == 0 || len2 == 0) return 0.0;

            int matchWindow = Math.Max(0, Math.Max(len1, len2) / 2 - 1);
            bool[] matches1 = new bool[len1];
            bool[] matches2 = new bool[len2];
            int matches = 0;

            for (int i = 0; i < len1; i++)
            {
                int start = Math.Max(0, i - matchWindow);
                int end = Math.Min(len2 - 1, i + matchWindow);

                for (int j = start; j <= end; j++)
                {
                    if (matches2[j]) continue;
                    if (s1[i] == s2[j])
                    {
                        matches1[i] = true;
                        matches2[j] = true;
                        matches++;
                        break;
                    }
                }
            }

            if (matches == 0) return 0.0;

            int transpositions = 0;
            int k = 0;
            for (int i = 0; i < len1; i++)
            {
                if (!matches1[i]) continue;
                while (!matches2[k]) k++;
                if (s1[i] != s2[k]) transpositions++;
                k++;
            }

            double m = matches;
            return (m / len1 + m / len2 + (m - transpositions / 2.0) / m) / 3.0;
        }

        /* ==========================================
           5. CONSULTA Y RESOLUCIÓN DE CONFLICTOS (DUDAS)
           ========================================== */

        public async Task<List<ConflictResolutionDto>> ObtenerConflictosVUPEAsync()
        {
            using var db = Connection;
            var list = new List<ConflictResolutionDto>();
            
            var conflicts = (await db.QueryAsync(@"
                SELECT StagingId, ProyectoRaw, 
                       ISNULL(MatchScore, NombreRaw) AS MatchScoreStr, 
                       IDRaw AS ProyectoCanonicoIdStr,
                       RFCRaw, TipoTecnologiaRaw, FolioProyectoRaw, 
                       CASE WHEN MatchScore IS NULL THEN 'No especificado' ELSE NombreRaw END AS NombreRaw,
                       EsHibridaRaw
                FROM staging.ExcelRaw_VUPEWorksheet
                WHERE PreFolioRaw = 'CONFLICTO'
            ")).ToList();

            if (!conflicts.Any()) return list;

            var projectsWithDetails = (await db.QueryAsync(@"
                SELECT p.ProyectoId, p.NombreOficial, t.Nombre AS TecnologiaNombre, dt.CapacidadInstaladaMW AS CapacidadMW,
                       (SELECT TOP 1 a.RazonSocial 
                        FROM core.ProyectoActor pa 
                        JOIN core.Actor a ON a.ActorId = pa.ActorId 
                        WHERE pa.ProyectoId = p.ProyectoId AND pa.TipoActorId = 1) AS PromoventeNombre,
                       (SELECT STRING_AGG(pi.ClaveExterna, ', ') 
                        FROM core.ProyectoIdentificador pi 
                        WHERE pi.ProyectoId = p.ProyectoId) AS FoliosAsociados,
                       dt.EsHibrido, dt.AlmacenamientoBess
                FROM core.Proyecto p
                LEFT JOIN core.CatTecnologia t ON t.TecnologiaId = p.TecnologiaId
                LEFT JOIN core.ProyectoDatosTecnicos dt ON dt.ProyectoId = p.ProyectoId
                WHERE p.Activo = 1
            ")).ToDictionary(p => (int)p.ProyectoId);

            foreach (var item in conflicts)
            {
                if (int.TryParse((string)item.ProyectoCanonicoIdStr, out int canonId) &&
                    projectsWithDetails.TryGetValue(canonId, out var canon))
                {
                    double.TryParse((string)item.MatchScoreStr, out double score);
                    
                    string esHibridoExcel = (string)item.EsHibridaRaw;
                    bool esHibridoDb = canon.EsHibrido == true || canon.AlmacenamientoBess == true;
                    
                    list.Add(new ConflictResolutionDto
                    {
                        TempRecordId = item.StagingId,
                        NombreProyectoOrigen = item.ProyectoRaw,
                        RazonSocialOrigen = item.NombreRaw,
                        TecnologiaOrigen = item.TipoTecnologiaRaw,
                        CapacidadOrigen = null,
                        ClaveExternaOrigen = item.FolioProyectoRaw,
                        
                        ProyectoCanonicoId = canonId,
                        NombreCanonico = canon.NombreOficial,
                        RazonSocialCanonico = canon.PromoventeNombre,
                        TecnologiaCanonica = canon.TecnologiaNombre,
                        CapacidadCanonica = (decimal?)(canon.CapacidadMW) ?? 0.0m,
                        
                        MatchScore = score,
                        
                        EsHibridoOrigen = esHibridoExcel,
                        EsHibridoCanonico = esHibridoDb,
                        FoliosCanonicos = canon.FoliosAsociados
                    });
                }
            }

            return list;
        }

        public async Task ResolverConflictoFusionarAsync(int stagingId, int proyectoId, string usuario)
        {
            using var db = Connection;
            if (db.State != ConnectionState.Open) db.Open();

            var stg = await db.QueryFirstOrDefaultAsync(@"
                SELECT * FROM staging.ExcelRaw_VUPEWorksheet WHERE StagingId = @stagingId
            ", new { stagingId });

            if (stg == null) return;

            string rawProjName = stg.ProyectoRaw;
            string rawFolio = stg.FolioProyectoRaw;

            // 1. Insertar identificador
            string extKey = rawFolio ?? $"VUPE-{stagingId}";
            try
            {
                await db.ExecuteAsync(@"
                    INSERT INTO core.ProyectoIdentificador (ProyectoId, OrigenDatosId, ClaveExterna, NombreEnOrigen)
                    VALUES (@ProyectoId, 1, @ClaveExterna, @NombreEnOrigen)
                ", new { 
                    ProyectoId = proyectoId, 
                    ClaveExterna = extKey, 
                    NombreEnOrigen = rawProjName 
                });
            }
            catch
            {
                // Ya existía
            }

            // 2. Registrar bitácora de carga
            await RegistrarBitacoraCargaAsync(db, proyectoId, 1, "Importacion-Resolucion-Manual", "Worksheet", stagingId, usuario, stg);

            // 3. Eliminar de staging
            await db.ExecuteAsync(@"
                DELETE FROM staging.ExcelRaw_VUPEWorksheet WHERE StagingId = @stagingId
            ", new { stagingId });
        }

        public async Task ResolverConflictoNuevoAsync(int stagingId, string usuario)
        {
            using var db = Connection;
            if (db.State != ConnectionState.Open) db.Open();

            var stg = await db.QueryFirstOrDefaultAsync(@"
                SELECT * FROM staging.ExcelRaw_VUPEWorksheet WHERE StagingId = @stagingId
            ", new { stagingId });

            if (stg == null) return;

            string rawProjName = stg.ProyectoRaw;
            string rawRfc = stg.RFCRaw;
            string rawFolio = stg.FolioProyectoRaw;
            string rawTech = stg.TipoTecnologiaRaw;
            string rawGie = stg.GrupoInteresRaw;
            string rawNombre = stg.NombreRaw;
            string esHibridaRaw = stg.EsHibridaRaw;

            var tecnologias = (await db.QueryAsync<CatTecnologia>("SELECT * FROM core.CatTecnologia")).ToList();
            var gies = (await db.QueryAsync<GrupoInteresEconomico>("SELECT * FROM core.GrupoInteresEconomico")).ToList();

            // 1. Homologar tecnología
            var techId = ResolveTechnology(rawTech, tecnologias, db);

            // 2. Insertar Proyecto Maestro
            var pId = await db.ExecuteScalarAsync<int>(@"
                INSERT INTO core.Proyecto (
                    Status, NombreOficial, NombreCorto, TecnologiaId, EstatusProyectoId, NivelMadurezId, Activo, CreadoEn, CreadoPor
                ) VALUES (
                    'Ventanilla', @NombreOficial, @NombreCorto, @TecnologiaId, 3, 1, 1, GETDATE(), @Usuario
                );
                SELECT SCOPE_IDENTITY();
            ", new { 
                NombreOficial = rawProjName, 
                NombreCorto = NormalizeProjectName(rawProjName), 
                TecnologiaId = techId, 
                Usuario = usuario 
            });

            // 3. Insertar Identificador
            string extKey = rawFolio ?? $"VUPE-{stagingId}";
            await db.ExecuteAsync(@"
                INSERT INTO core.ProyectoIdentificador (ProyectoId, OrigenDatosId, ClaveExterna, NombreEnOrigen)
                VALUES (@ProyectoId, 1, @ClaveExterna, @NombreEnOrigen)
            ", new { 
                ProyectoId = pId, 
                ClaveExterna = extKey, 
                NombreEnOrigen = rawProjName 
            });

            // 4. Crear Actor / Empresa (si no existe) y asociarlo
            int? actorId = null;
            if (!string.IsNullOrWhiteSpace(rawNombre))
            {
                var actorMap = (await db.QueryAsync<Actor>("SELECT * FROM core.Actor WHERE Activo = 1"))
                    .GroupBy(a => a.RazonSocial).ToDictionary(g => g.Key, g => g.First().ActorId, StringComparer.OrdinalIgnoreCase);

                if (actorMap.TryGetValue(rawNombre, out var existActorId))
                {
                    actorId = existActorId;
                }
                else
                {
                    int? gieId = null;
                    if (!string.IsNullOrWhiteSpace(rawGie))
                    {
                        gieId = gies.FirstOrDefault(g => string.Equals(g.Nombre, rawGie, StringComparison.OrdinalIgnoreCase))?.GrupoEconomicoId;
                        if (!gieId.HasValue)
                        {
                            gieId = await db.ExecuteScalarAsync<int>(@"
                                INSERT INTO core.GrupoInteresEconomico (Nombre) VALUES (@rawGie);
                                SELECT SCOPE_IDENTITY();
                            ", new { rawGie });
                        }
                    }

                    actorId = await db.ExecuteScalarAsync<int>(@"
                        INSERT INTO core.Actor (RazonSocial, RFC, GrupoEconomicoId, Activo)
                        VALUES (@RazonSocial, @RFC, @gieId, 1);
                        SELECT SCOPE_IDENTITY();
                    ", new { RazonSocial = rawNombre, RFC = rawRfc, gieId });
                }

                // Relación Proyecto - Actor (Promovente)
                await db.ExecuteAsync(@"
                    INSERT INTO core.ProyectoActor (ProyectoId, ActorId, TipoActorId)
                    VALUES (@ProyectoId, @ActorId, 1)
                ", new { ProyectoId = pId, ActorId = actorId.Value });
            }

            // 5. Ubicación (Yucatán / Quintana Roo etc)
            await db.ExecuteAsync(@"
                INSERT INTO core.ProyectoUbicacion (ProyectoId, EntidadFederativaId, MunicipioId, EsPrincipal)
                VALUES (@ProyectoId, 99, 99001, 1)
            ", new { ProyectoId = pId });

            // 6. Datos técnicos y almacenamiento BESS inicial
            bool storage = string.Equals(esHibridaRaw, "SI", StringComparison.OrdinalIgnoreCase);
            await db.ExecuteAsync(@"
                INSERT INTO core.ProyectoDatosTecnicos (ProyectoId, CapacidadInstaladaMW, AlmacenamientoBess, EsHibrido)
                VALUES (@ProyectoId, 0.0, @storage, @storage)
            ", new { ProyectoId = pId, storage });

            // 7. Datos financieros iniciales
            await db.ExecuteAsync(@"
                INSERT INTO core.ProyectoDatosFinancieros (ProyectoId, MonedaId)
                VALUES (@ProyectoId, 1)
            ", new { ProyectoId = pId });

            // Registrar bitácora de carga
            await RegistrarBitacoraCargaAsync(db, pId, 1, "Importacion-Resolucion-Manual", "Worksheet", stagingId, usuario, stg);

            // 8. Eliminar de staging
            await db.ExecuteAsync(@"
                DELETE FROM staging.ExcelRaw_VUPEWorksheet WHERE StagingId = @stagingId
            ", new { stagingId });
        }
    }
}
