using System.Globalization;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using ClosedXML.Excel;
using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using NSIE.Models;

namespace NSIE.Servicios
{
    public interface IPamSeguimientoTransmisionService
    {
        Task<bool> EsLoteSeguimientoAsync(long loteId, CancellationToken cancellationToken);
        Task<PamSeguimientoPreparacionViewModel> PrepararAsync(long loteId, CancellationToken cancellationToken);
        Task<PamSeguimientoAplicacionResultado> AplicarAsync(
            long loteId,
            int usuarioId,
            string usuarioNombre,
            CancellationToken cancellationToken);
    }

    public sealed class PamSeguimientoTransmisionService : IPamSeguimientoTransmisionService
    {
        public const string PerfilVersion = "SeguimientoTransmisionV1";
        private const string HojaBase = "Base_Transmisión";
        private const long MaximoExcelBytes = 80L * 1024 * 1024;
        private static readonly Regex ClaveRegex = new(
            @"(?<![A-Z0-9])(?:[A-Z]{1,5}\d{2})-[A-Z0-9]{2,12}(?![A-Z0-9])",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex NumeroRegex = new(
            @"-?\d[\d,]*(?:\.\d+)?",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private readonly string _connectionString;
        private readonly string _storageRoot;

        public PamSeguimientoTransmisionService(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No existe ConnectionStrings:DefaultConnection para el seguimiento PAM.");

            var configuredRoot = configuration["PamActualizaciones:StorageRoot"];
            if (string.IsNullOrWhiteSpace(configuredRoot))
                configuredRoot = @"D:\DGMESNIE\RepositorioPAM\Actualizaciones";

            configuredRoot = Environment.ExpandEnvironmentVariables(configuredRoot.Trim());
            if (!Path.IsPathRooted(configuredRoot))
                throw new InvalidOperationException("La carpeta del repositorio PAM debe usar una ruta absoluta.");

            _storageRoot = Path.GetFullPath(configuredRoot);
            if (EsMismaOSubcarpeta(_storageRoot, environment.ContentRootPath)
                || (!string.IsNullOrWhiteSpace(environment.WebRootPath)
                    && EsMismaOSubcarpeta(_storageRoot, environment.WebRootPath)))
            {
                throw new InvalidOperationException("El repositorio documental PAM debe estar fuera de la aplicación y de wwwroot.");
            }
        }

        public async Task<bool> EsLoteSeguimientoAsync(long loteId, CancellationToken cancellationToken)
        {
            const string sql = @"
SELECT COUNT_BIG(1)
FROM dgmesnie.PAMLoteActualizacion
WHERE LoteId = @LoteId AND TipoActualizacion = @Tipo;";

            await using var connection = new SqlConnection(_connectionString);
            var total = await connection.ExecuteScalarAsync<long>(new CommandDefinition(
                sql,
                new { LoteId = loteId, Tipo = PamTiposActualizacion.SeguimientoTransmision },
                cancellationToken: cancellationToken));
            return total > 0;
        }

        public async Task<PamSeguimientoPreparacionViewModel> PrepararAsync(
            long loteId,
            CancellationToken cancellationToken)
        {
            var preparacion = await ConstruirPreparacionAsync(loteId, cancellationToken);
            return CrearViewModel(preparacion);
        }

        public async Task<PamSeguimientoAplicacionResultado> AplicarAsync(
            long loteId,
            int usuarioId,
            string usuarioNombre,
            CancellationToken cancellationToken)
        {
            var preparacion = await ConstruirPreparacionAsync(loteId, cancellationToken);
            if (preparacion.YaIncorporado)
            {
                return new PamSeguimientoAplicacionResultado
                {
                    LoteId = loteId,
                    YaExistia = true,
                    RegistrosIncorporados = preparacion.Registros.Count,
                    UnidadesIncorporadas = preparacion.Registros.Select(x => x.CodigoUnico).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
                    VinculacionesRegistradas = preparacion.Relaciones.Count,
                    ClavesNoEncontradas = preparacion.Pendientes.Select(x => x.CodigoPem).Distinct(StringComparer.OrdinalIgnoreCase).Count()
                };
            }

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                var yaAplicado = await connection.ExecuteScalarAsync<int>(new CommandDefinition(@"
SELECT COUNT(1)
FROM dgmesnie.PAMSeguimientoCorteDetalle WITH (UPDLOCK, HOLDLOCK)
WHERE LoteId = @LoteId;", new { LoteId = loteId }, transaction, cancellationToken: cancellationToken));

                if (yaAplicado > 0)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return new PamSeguimientoAplicacionResultado
                    {
                        LoteId = loteId,
                        YaExistia = true,
                        RegistrosIncorporados = preparacion.Registros.Count,
                        UnidadesIncorporadas = preparacion.Registros.Select(x => x.CodigoUnico).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
                        VinculacionesRegistradas = preparacion.Relaciones.Count,
                        ClavesNoEncontradas = preparacion.Pendientes.Select(x => x.CodigoPem).Distinct(StringComparer.OrdinalIgnoreCase).Count()
                    };
                }

                var loteDuplicado = await connection.QuerySingleOrDefaultAsync<long?>(new CommandDefinition(@"
SELECT TOP (1) LoteId
FROM dgmesnie.PAMSeguimientoCorteDetalle
WHERE FuenteId = @FuenteId AND LoteId <> @LoteId
ORDER BY LoteId DESC;", new { preparacion.Contexto.FuenteId, LoteId = loteId }, transaction, cancellationToken: cancellationToken));

                if (loteDuplicado.HasValue)
                    throw new InvalidOperationException($"Este mismo archivo ya fue incorporado en el lote {loteDuplicado.Value}. Si recibiste una corrección, carga el archivo corregido para conservar ambas huellas.");

                const string buscarUnidadSql = @"
SELECT SeguimientoUnidadId
FROM dgmesnie.PAMSeguimientoUnidad WITH (UPDLOCK, HOLDLOCK)
WHERE CodigoUnico = @CodigoUnico;";
                const string insertarUnidadSql = @"
INSERT dgmesnie.PAMSeguimientoUnidad
    (CodigoUnico, IdSepi, NombreUnidad, TieneFases, PrimerLoteId, UltimoLoteId, UsuarioActualizacion)
VALUES
    (@CodigoUnico, @IdSepi, @NombreUnidad, @TieneFases, @LoteId, @LoteId, @UsuarioNombre);
SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";
                const string actualizarUnidadSql = @"
UPDATE dgmesnie.PAMSeguimientoUnidad
SET IdSepi = COALESCE(NULLIF(@IdSepi, N''), IdSepi),
    NombreUnidad = @NombreUnidad,
    TieneFases = CASE WHEN TieneFases = 1 OR @TieneFases = 1 THEN 1 ELSE 0 END,
    UltimoLoteId = @LoteId,
    FechaActualizacionUtc = SYSUTCDATETIME(),
    UsuarioActualizacion = @UsuarioNombre
WHERE SeguimientoUnidadId = @SeguimientoUnidadId;";

                var unidades = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
                foreach (var grupo in preparacion.Registros.GroupBy(x => x.CodigoUnico, StringComparer.OrdinalIgnoreCase))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var referencia = grupo.First();
                    var idSepi = grupo.Select(x => x.IdSepi).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
                    var nombre = grupo.Select(x => x.NombreProyecto).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? referencia.CodigoUnico;
                    var tieneFases = grupo.Any(x => x.TieneFases);
                    var unidadId = await connection.QuerySingleOrDefaultAsync<long?>(new CommandDefinition(
                        buscarUnidadSql,
                        new { CodigoUnico = grupo.Key },
                        transaction,
                        cancellationToken: cancellationToken));

                    if (!unidadId.HasValue)
                    {
                        unidadId = await connection.ExecuteScalarAsync<long>(new CommandDefinition(
                            insertarUnidadSql,
                            new
                            {
                                CodigoUnico = grupo.Key,
                                IdSepi = Limitar(idSepi, 200),
                                NombreUnidad = Limitar(nombre, 1000),
                                TieneFases = tieneFases,
                                LoteId = loteId,
                                UsuarioNombre = Limitar(usuarioNombre, 150)
                            },
                            transaction,
                            cancellationToken: cancellationToken));
                    }
                    else
                    {
                        await connection.ExecuteAsync(new CommandDefinition(
                            actualizarUnidadSql,
                            new
                            {
                                SeguimientoUnidadId = unidadId.Value,
                                IdSepi = Limitar(idSepi, 200),
                                NombreUnidad = Limitar(nombre, 1000),
                                TieneFases = tieneFases,
                                LoteId = loteId,
                                UsuarioNombre = Limitar(usuarioNombre, 150)
                            },
                            transaction,
                            cancellationToken: cancellationToken));
                    }

                    unidades[grupo.Key] = unidadId.Value;
                }

                const string insertarDetalleSql = @"
INSERT dgmesnie.PAMSeguimientoCorteDetalle
    (SeguimientoUnidadId, LoteId, FuenteId, PerfilVersion, Hoja, NumeroFila,
     CodigoPem, IdSepi, IdProcedimiento, ProyectoEnFases, FaseInicial, FasesSubsecuentes,
     ActivoAdministracionActual, AdministracionActual, NombreProyecto, Categoria,
     Mva, Mvar, KmC, OtraMetaFisica, ImporteMdp, FinanciamientoCfe,
     FinanciamientoPormenorizado, AnioInstruccion,
     FechaInicioConcursoProgramada, FechaInicioConcursoReal,
     FechaAdjudicacionProgramada, FechaAdjudicacionReal,
     FechaFirmaContratoProgramada, FechaFirmaContratoReal,
     FechaInicioConstruccionProgramada, FechaInicioConstruccionReal,
     FechaTerminoConstruccion, FeoIndicada, FeoFactible, PlazoEjecucionDias,
     AvanceProgramado, AvanceReal, ComentariosPpt, NotaPpt, NombrePormenorizado,
     ElementosEquipos, ActualizacionEstatus, UltimaActualizacionFecha,
     DetalleUltimaActualizacion, OrigenUltimaActualizacion, ComentariosInternos,
     Visible, DatosOrigenJson, HashFila, UsuarioRegistro)
VALUES
    (@SeguimientoUnidadId, @LoteId, @FuenteId, @PerfilVersion, @Hoja, @NumeroFila,
     @CodigoPem, @IdSepi, @IdProcedimiento, @ProyectoEnFases, @FaseInicial, @FasesSubsecuentes,
     @ActivoAdministracionActual, @AdministracionActual, @NombreProyecto, @Categoria,
     @Mva, @Mvar, @KmC, @OtraMetaFisica, @ImporteMdp, @FinanciamientoCfe,
     @FinanciamientoPormenorizado, @AnioInstruccion,
     @FechaInicioConcursoProgramada, @FechaInicioConcursoReal,
     @FechaAdjudicacionProgramada, @FechaAdjudicacionReal,
     @FechaFirmaContratoProgramada, @FechaFirmaContratoReal,
     @FechaInicioConstruccionProgramada, @FechaInicioConstruccionReal,
     @FechaTerminoConstruccion, @FeoIndicada, @FeoFactible, @PlazoEjecucionDias,
     @AvanceProgramado, @AvanceReal, @ComentariosPpt, @NotaPpt, @NombrePormenorizado,
     @ElementosEquipos, @ActualizacionEstatus, @UltimaActualizacionFecha,
     @DetalleUltimaActualizacion, @OrigenUltimaActualizacion, @ComentariosInternos,
     @Visible, @DatosOrigenJson, @HashFila, @UsuarioRegistro);";

                foreach (var registro in preparacion.Registros)
                {
                    registro.SeguimientoUnidadId = unidades[registro.CodigoUnico];
                    registro.LoteId = loteId;
                    registro.FuenteId = preparacion.Contexto.FuenteId;
                    registro.UsuarioRegistro = Limitar(usuarioNombre, 150);
                    await connection.ExecuteAsync(new CommandDefinition(
                        insertarDetalleSql,
                        registro,
                        transaction,
                        cancellationToken: cancellationToken));
                }

                const string insertarRelacionSql = @"
INSERT dgmesnie.PAMSeguimientoUnidadProyecto
    (SeguimientoUnidadId, ProyectoId, LoteId, FuenteId, CodigoPem,
     TipoCoincidencia, FechaCorte, UsuarioRegistro)
VALUES
    (@SeguimientoUnidadId, @ProyectoId, @LoteId, @FuenteId, @CodigoPem,
     @TipoCoincidencia, @FechaCorte, @UsuarioRegistro);";

                foreach (var relacion in preparacion.Relaciones
                    .GroupBy(x => $"{x.CodigoUnico}|{x.ProyectoId}|{x.CodigoPem}", StringComparer.OrdinalIgnoreCase)
                    .Select(x => x.First()))
                {
                    await connection.ExecuteAsync(new CommandDefinition(
                        insertarRelacionSql,
                        new
                        {
                            SeguimientoUnidadId = unidades[relacion.CodigoUnico],
                            relacion.ProyectoId,
                            LoteId = loteId,
                            preparacion.Contexto.FuenteId,
                            relacion.CodigoPem,
                            relacion.TipoCoincidencia,
                            preparacion.Contexto.FechaCorte,
                            UsuarioRegistro = Limitar(usuarioNombre, 150)
                        },
                        transaction,
                        cancellationToken: cancellationToken));
                }

                await connection.ExecuteAsync(new CommandDefinition(@"
UPDATE dgmesnie.PAMLoteFuente
SET EstadoExtraccion = N'Extraído',
    MensajeExtraccion = @Mensaje
WHERE LoteId = @LoteId AND FuenteId = @FuenteId;

UPDATE dgmesnie.PAMLoteActualizacion
SET Estado = N'Aplicado',
    TotalRegistrosDetectados = @TotalRegistros,
    TotalCambiosPropuestos = 0,
    TotalObservados = @TotalObservados,
    FechaActualizacionUtc = SYSUTCDATETIME()
WHERE LoteId = @LoteId;",
                    new
                    {
                        LoteId = loteId,
                        preparacion.Contexto.FuenteId,
                        Mensaje = $"Seguimiento incorporado con {PerfilVersion}: {preparacion.Registros.Count} registro(s). La cartera maestra no fue modificada.",
                        TotalRegistros = preparacion.Registros.Count,
                        TotalObservados = preparacion.Pendientes.Select(x => x.CodigoPem).Distinct(StringComparer.OrdinalIgnoreCase).Count()
                    },
                    transaction,
                    cancellationToken: cancellationToken));

                await transaction.CommitAsync(cancellationToken);
                return new PamSeguimientoAplicacionResultado
                {
                    LoteId = loteId,
                    RegistrosIncorporados = preparacion.Registros.Count,
                    UnidadesIncorporadas = unidades.Count,
                    VinculacionesRegistradas = preparacion.Relaciones
                        .GroupBy(x => $"{x.CodigoUnico}|{x.ProyectoId}|{x.CodigoPem}", StringComparer.OrdinalIgnoreCase).Count(),
                    ClavesNoEncontradas = preparacion.Pendientes.Select(x => x.CodigoPem).Distinct(StringComparer.OrdinalIgnoreCase).Count()
                };
            }
            catch
            {
                await transaction.RollbackAsync(CancellationToken.None);
                throw;
            }
        }

        private async Task<PreparacionInterna> ConstruirPreparacionAsync(
            long loteId,
            CancellationToken cancellationToken)
        {
            var contexto = await ObtenerContextoAsync(loteId, cancellationToken);
            if (contexto == null) throw new InvalidOperationException("No existe el lote de seguimiento solicitado.");
            if (contexto.TipoActualizacion != PamTiposActualizacion.SeguimientoTransmision)
                throw new InvalidOperationException("El lote no corresponde a un seguimiento de transmisión.");

            var rutaFisica = ResolverRuta(contexto.RutaArchivo);
            if (!File.Exists(rutaFisica))
                throw new InvalidOperationException("No se encontró el archivo original del seguimiento en el repositorio administrado.");
            if (new FileInfo(rutaFisica).Length > MaximoExcelBytes)
                throw new InvalidOperationException("El Excel supera el límite de 80 MB para el perfil de seguimiento.");

            var registros = await Task.Run(() => ExtraerRegistros(rutaFisica, cancellationToken), cancellationToken);
            if (registros.Count == 0)
                throw new InvalidOperationException("La hoja Base_Transmisión no contiene registros utilizables.");

            var claves = await ObtenerClavesVigentesAsync(cancellationToken);
            var relaciones = new List<RelacionDetectada>();
            var pendientes = new List<PamSeguimientoClavePendiente>();

            foreach (var registro in registros)
            {
                var codigos = ExtraerClaves(registro.CodigoPem);
                if (codigos.Count == 0 && !string.IsNullOrWhiteSpace(registro.CodigoPem))
                {
                    pendientes.Add(new PamSeguimientoClavePendiente
                    {
                        CodigoUnico = registro.CodigoUnico,
                        CodigoPem = registro.CodigoPem,
                        NombreProyecto = registro.NombreProyecto,
                        NumeroFila = registro.NumeroFila,
                        Motivo = "El valor no contiene una clave PEM reconocible."
                    });
                    continue;
                }

                foreach (var codigo in codigos)
                {
                    if (!claves.TryGetValue(NormalizarClave(codigo), out var candidatos)
                        || candidatos.Select(x => x.ProyectoId).Distinct().Count() != 1)
                    {
                        pendientes.Add(new PamSeguimientoClavePendiente
                        {
                            CodigoUnico = registro.CodigoUnico,
                            CodigoPem = codigo,
                            NombreProyecto = registro.NombreProyecto,
                            NumeroFila = registro.NumeroFila,
                            Motivo = candidatos == null
                                ? "La clave no existe en la cartera vigente."
                                : "La clave coincide con más de una identidad y requiere conciliación."
                        });
                        continue;
                    }

                    var proyectoId = candidatos.Select(x => x.ProyectoId).Distinct().Single();
                    var esPrincipal = candidatos.Any(x => x.ProyectoId == proyectoId
                        && string.Equals(x.TipoClave, "Principal", StringComparison.OrdinalIgnoreCase));
                    relaciones.Add(new RelacionDetectada
                    {
                        CodigoUnico = registro.CodigoUnico,
                        CodigoPem = codigo,
                        ProyectoId = proyectoId,
                        TipoCoincidencia = esPrincipal ? "Exacta" : "Alterna"
                    });
                }
            }

            await using var connection = new SqlConnection(_connectionString);
            var yaIncorporado = await connection.ExecuteScalarAsync<int>(new CommandDefinition(@"
SELECT COUNT(1)
FROM dgmesnie.PAMSeguimientoCorteDetalle
WHERE LoteId = @LoteId;", new { LoteId = loteId }, cancellationToken: cancellationToken)) > 0;

            return new PreparacionInterna
            {
                Contexto = contexto,
                Registros = registros,
                Relaciones = relaciones,
                Pendientes = pendientes,
                YaIncorporado = yaIncorporado
            };
        }

        private static PamSeguimientoPreparacionViewModel CrearViewModel(PreparacionInterna preparacion)
        {
            var claves = preparacion.Registros.SelectMany(x => ExtraerClaves(x.CodigoPem))
                .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var vinculadas = preparacion.Relaciones.Select(x => x.CodigoPem)
                .Distinct(StringComparer.OrdinalIgnoreCase).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var pendientes = preparacion.Pendientes
                .GroupBy(x => x.CodigoPem ?? $"fila:{x.NumeroFila}", StringComparer.OrdinalIgnoreCase)
                .Select(x => x.First())
                .OrderBy(x => x.CodigoPem)
                .ToList();

            var model = new PamSeguimientoPreparacionViewModel
            {
                LoteId = preparacion.Contexto.LoteId,
                LoteUid = preparacion.Contexto.LoteUid,
                NombreLote = preparacion.Contexto.Nombre,
                FechaCorte = preparacion.Contexto.FechaCorte,
                EstadoLote = preparacion.Contexto.Estado,
                FuenteNombre = preparacion.Contexto.NombreOriginal,
                PerfilVersion = PerfilVersion,
                YaIncorporado = preparacion.YaIncorporado,
                TotalRegistros = preparacion.Registros.Count,
                TotalUnidades = preparacion.Registros.Select(x => x.CodigoUnico).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
                TotalClavesPem = claves.Count,
                ClavesVinculadas = vinculadas.Count,
                ClavesNoEncontradas = pendientes.Count,
                RegistrosConFases = preparacion.Registros.Count(x => x.TieneFases),
                ImporteTotalMdp = preparacion.Registros.Sum(x => x.ImporteMdp ?? 0m),
                Pendientes = pendientes
            };

            if (preparacion.Registros.GroupBy(x => x.CodigoUnico, StringComparer.OrdinalIgnoreCase).Any(x => x.Count() > 1))
                model.Advertencias.Add("Las filas repetidas por Código Único se conservarán como fases o procedimientos del mismo seguimiento; no crearán proyectos maestros nuevos.");
            if (pendientes.Count > 0)
                model.Advertencias.Add($"{pendientes.Count} clave(s) quedarán visibles para conciliación, sin impedir que se conserve la fotografía del corte.");
            model.Advertencias.Add("El calendario, la fecha estimada de término y las diferencias de avance se calcularán en SQL; no se importan las fórmulas del libro.");
            return model;
        }

        private async Task<LoteContexto> ObtenerContextoAsync(long loteId, CancellationToken cancellationToken)
        {
            const string sql = @"
SELECT l.LoteId, l.LoteUid, l.Nombre, l.TipoActualizacion, l.FechaCorte, l.Estado,
       lf.FuenteId, lf.NombreOriginal, lf.Extension, f.RutaArchivo, f.HashSha256
FROM dgmesnie.PAMLoteActualizacion l
INNER JOIN dgmesnie.PAMLoteFuente lf ON lf.LoteId = l.LoteId
INNER JOIN dgmesnie.PAMFuente f ON f.FuenteId = lf.FuenteId
WHERE l.LoteId = @LoteId AND LOWER(lf.Extension) = N'.xlsx';";

            await using var connection = new SqlConnection(_connectionString);
            var fuentes = (await connection.QueryAsync<LoteContexto>(new CommandDefinition(
                sql,
                new { LoteId = loteId },
                cancellationToken: cancellationToken))).ToList();
            if (fuentes.Count == 0) return null;
            if (fuentes.Count > 1)
                throw new InvalidOperationException("El lote de seguimiento debe contener un solo archivo .xlsx.");
            return fuentes[0];
        }

        private async Task<Dictionary<string, List<ClaveVigente>>> ObtenerClavesVigentesAsync(
            CancellationToken cancellationToken)
        {
            const string sql = @"
SELECT ProyectoId, ClaveProyecto, TipoClave
FROM dgmesnie.vw_PAMProyectoClaveVigente
WHERE NULLIF(LTRIM(RTRIM(ClaveProyecto)), N'') IS NOT NULL;";
            await using var connection = new SqlConnection(_connectionString);
            var rows = await connection.QueryAsync<ClaveVigente>(new CommandDefinition(
                sql,
                cancellationToken: cancellationToken));
            return rows
                .Select(x => new { Clave = NormalizarClave(x.ClaveProyecto), Item = x })
                .Where(x => !string.IsNullOrWhiteSpace(x.Clave))
                .GroupBy(x => x.Clave, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(x => x.Key, x => x.Select(y => y.Item).ToList(), StringComparer.OrdinalIgnoreCase);
        }

        private static List<RegistroSeguimiento> ExtraerRegistros(
            string rutaFisica,
            CancellationToken cancellationToken)
        {
            var rutaCompatible = CrearCopiaCompatibleClosedXml(rutaFisica);
            try
            {
                using var workbook = new XLWorkbook(rutaCompatible);
                var worksheet = workbook.Worksheets.FirstOrDefault(x =>
                    string.Equals(x.Name.Trim(), HojaBase, StringComparison.OrdinalIgnoreCase));
                if (worksheet == null)
                    throw new InvalidOperationException($"El archivo no contiene la hoja {HojaBase}.");

                var used = worksheet.RangeUsed();
                if (used == null) return new List<RegistroSeguimiento>();
                var headerRowNumber = 2;
                var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                var originalHeaders = new Dictionary<int, string>();
                for (var column = used.FirstColumn().ColumnNumber(); column <= used.LastColumn().ColumnNumber(); column++)
                {
                    var original = worksheet.Cell(headerRowNumber, column).GetFormattedString()?.Trim();
                    if (string.IsNullOrWhiteSpace(original)) continue;
                    var normalized = NormalizarEncabezado(original);
                    if (!headers.ContainsKey(normalized)) headers[normalized] = column;
                    originalHeaders[column] = original;
                }

                ValidarEncabezados(headers);
                var registros = new List<RegistroSeguimiento>();
                for (var rowNumber = headerRowNumber + 1; rowNumber <= used.LastRow().RowNumber(); rowNumber++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var row = worksheet.Row(rowNumber);
                    var codigoUnico = Limitar(LeerTexto(row, headers, "CODIGOUNICOLLAVEINTERNA"), 200);
                    var nombre = Limitar(LeerTexto(row, headers, "PROYECTO"), 1000);
                    if (string.IsNullOrWhiteSpace(codigoUnico) && string.IsNullOrWhiteSpace(nombre)) continue;
                    if (string.IsNullOrWhiteSpace(codigoUnico) || string.IsNullOrWhiteSpace(nombre))
                        continue;

                    codigoUnico = NormalizarCodigoUnidad(codigoUnico);
                    var datos = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var header in originalHeaders)
                    {
                        var value = ObtenerTextoSeguro(row.Cell(header.Key));
                        if (!string.IsNullOrWhiteSpace(value)) datos[header.Value] = value;
                    }
                    var json = JsonConvert.SerializeObject(datos, Formatting.None);

                    var proyectoEnFases = LeerTexto(row, headers, "PROYECTOENFASES");
                    var faseInicial = LeerTexto(row, headers, "FASEINICIAL");
                    var fasesSubsecuentes = LeerTexto(row, headers, "FASESSUBSECUENTES");
                    var registro = new RegistroSeguimiento
                    {
                    PerfilVersion = PerfilVersion,
                    Hoja = HojaBase,
                    NumeroFila = rowNumber,
                    CodigoUnico = codigoUnico,
                    CodigoPem = Limitar(LeerTexto(row, headers, "CODIGOPEM"), 500),
                    IdSepi = Limitar(LeerTexto(row, headers, "IDSEPIINVERSIONCFE"), 200),
                    IdProcedimiento = Limitar(LeerTexto(row, headers, "IDPROCEDIMIENTO"), 300),
                    ProyectoEnFases = Limitar(proyectoEnFases, 100),
                    FaseInicial = Limitar(faseInicial, 300),
                    FasesSubsecuentes = Limitar(fasesSubsecuentes, 1000),
                    ActivoAdministracionActual = LeerBit(row, headers, "PROYECTOSACTIVOSADMINISTRACIONACTUAL133INCLUYEFASES"),
                    AdministracionActual = LeerBit(row, headers, "PROYECTOSADMINISTRACIONACTUAL164INCLUYEFASES"),
                    NombreProyecto = nombre,
                    Categoria = Limitar(LeerTexto(row, headers, "CATEGORIA"), 300),
                    Mva = LeerDecimal(row, headers, "MVA"),
                    Mvar = LeerDecimal(row, headers, "MVAR"),
                    KmC = LeerDecimal(row, headers, "KMC"),
                    OtraMetaFisica = Limitar(LeerTexto(row, headers, "OTRA"), 1000),
                    ImporteMdp = LeerDecimal(row, headers, "IMPORTEPORMENORIZADOJUNIO"),
                    FinanciamientoCfe = Limitar(LeerTexto(row, headers, "TIPODEFINANCIAMIENTOCFETRANSMISION"), 500),
                    FinanciamientoPormenorizado = Limitar(LeerTexto(row, headers, "TIPODEFINANCIAMIENTOINFPORMENORIZADO"), 500),
                    AnioInstruccion = LeerEntero(row, headers, "ANODEINSTRUCCION"),
                    FechaInicioConcursoProgramada = LeerFecha(row, headers, "FECHAINICIODECONCURSOPROGRAMADA"),
                    FechaInicioConcursoReal = LeerFecha(row, headers, "FECHAINICIODECONCURSOREAL"),
                    FechaAdjudicacionProgramada = LeerFecha(row, headers, "FECHADEADJUDICACIONPROGRAMADA"),
                    FechaAdjudicacionReal = LeerFecha(row, headers, "FECHADEADJUDICACIONREAL"),
                    FechaFirmaContratoProgramada = LeerFecha(row, headers, "FECHAFIRMADECONTRATOPROGRAMADA"),
                    FechaFirmaContratoReal = LeerFecha(row, headers, "FECHAFIRMADECONTRATOREAL"),
                    FechaInicioConstruccionProgramada = LeerFecha(row, headers, "FECHADEINICIODECONSTRUCCIONPROGRAMADA"),
                    FechaInicioConstruccionReal = LeerFecha(row, headers, "FECHADEINICIODECONSTRUCCIONREAL"),
                    FechaTerminoConstruccion = LeerFecha(row, headers, "FECHADETERMINODECONSTRUCCION"),
                    FeoIndicada = Limitar(LeerTexto(row, headers, "FEOINDICADAENOFICIOSENERINFPORMENORIZADO"), 200),
                    FeoFactible = Limitar(LeerTexto(row, headers, "FEOFACTIBLEINFPORMENORIZADO"), 200),
                    PlazoEjecucionDias = LeerEntero(row, headers, "PLAZODEEJECUCIONDN"),
                    AvanceProgramado = LeerPorcentaje(row, headers, "AVANCEPROGRAMADO"),
                    AvanceReal = LeerPorcentaje(row, headers, "AVANCEREAL"),
                    ComentariosPpt = LeerTextoPorPrefijo(row, headers, "COMENTARIOSPPTCFETRANSMISION"),
                    NotaPpt = LeerTextoPorPrefijo(row, headers, "NOTAPPTCFETRANSMISION"),
                    NombrePormenorizado = Limitar(LeerTextoPorPrefijo(row, headers, "NOMBREPROYECTOPORMENORIZADOS"), 1000),
                    ElementosEquipos = LeerTextoPorPrefijo(row, headers, "ELEMENTOSYEQUIPOSASOCIADOSPORMENORIZADOS"),
                    ActualizacionEstatus = LeerTexto(row, headers, "ACTUALIZACIONESTATUS"),
                    UltimaActualizacionFecha = LeerFecha(row, headers, "ULTIMAACTUALIZACIONFECHA"),
                    DetalleUltimaActualizacion = LeerTextoPorPrefijo(row, headers, "DETALLEULTIMAACTUALIZACION"),
                    OrigenUltimaActualizacion = Limitar(LeerTextoPorPrefijo(row, headers, "ORIGENFUENTEACTUALIZACION"), 1000),
                    ComentariosInternos = LeerTexto(row, headers, "COMENTARIOSINTERNOS"),
                    Visible = LeerBitPorPrefijo(row, headers, "VISIBLE"),
                    DatosOrigenJson = json,
                    HashFila = CrearHash($"{PerfilVersion}|{HojaBase}|{rowNumber}|{json}")
                    };
                    registro.TieneFases = EsVerdadero(proyectoEnFases)
                        || !string.IsNullOrWhiteSpace(faseInicial)
                        || !string.IsNullOrWhiteSpace(fasesSubsecuentes);
                    registros.Add(registro);
                }

                return registros;
            }
            finally
            {
                try { File.Delete(rutaCompatible); }
                catch { }
            }
        }

        private static string CrearCopiaCompatibleClosedXml(string rutaFisica)
        {
            var temporal = Path.Combine(Path.GetTempPath(), $"pam-seguimiento-{Guid.NewGuid():N}.xlsx");
            File.Copy(rutaFisica, temporal, false);
            using var archive = ZipFile.Open(temporal, ZipArchiveMode.Update);
            RecortarTextosOpenXml(archive, "xl/sharedStrings.xml", "si");
            foreach (var entryName in archive.Entries
                .Where(x => x.FullName.StartsWith("xl/worksheets/", StringComparison.OrdinalIgnoreCase)
                    && x.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                .Select(x => x.FullName)
                .ToList())
            {
                RecortarTextosOpenXml(archive, entryName, "is");
            }
            return temporal;
        }

        private static void RecortarTextosOpenXml(ZipArchive archive, string entryName, string containerName)
        {
            var entry = archive.GetEntry(entryName);
            if (entry == null) return;
            XDocument document;
            using (var stream = entry.Open())
                document = XDocument.Load(stream, System.Xml.Linq.LoadOptions.PreserveWhitespace);

            var ns = (XNamespace)"http://schemas.openxmlformats.org/spreadsheetml/2006/main";
            var changed = false;
            foreach (var container in document.Descendants(ns + containerName))
            {
                // ClosedXML agrega los segmentos de texto enriquecido uno a uno; se deja
                // margen por debajo del límite formal de Excel para libros con rich text.
                var remaining = 30000;
                foreach (var textNode in container.Descendants(ns + "t"))
                {
                    var value = textNode.Value ?? string.Empty;
                    if (remaining <= 0)
                    {
                        if (value.Length > 0) { textNode.Value = string.Empty; changed = true; }
                        continue;
                    }
                    if (value.Length > remaining)
                    {
                        textNode.Value = value[..remaining];
                        changed = true;
                    }
                    remaining -= Math.Min(value.Length, remaining);
                }
            }

            if (!changed) return;
            entry.Delete();
            var replacement = archive.CreateEntry(entryName, CompressionLevel.Optimal);
            using var output = replacement.Open();
            document.Save(output, System.Xml.Linq.SaveOptions.DisableFormatting);
        }

        private static void ValidarEncabezados(IReadOnlyDictionary<string, int> headers)
        {
            var required = new[]
            {
                "CODIGOUNICOLLAVEINTERNA", "CODIGOPEM", "PROYECTO", "CATEGORIA", "IMPORTEPORMENORIZADOJUNIO"
            };
            var missing = required.Where(x => !headers.ContainsKey(x)).ToList();
            if (missing.Count > 0)
                throw new InvalidOperationException($"La hoja {HojaBase} no coincide con {PerfilVersion}. Faltan {missing.Count} encabezado(s) requerido(s).");
        }

        private string ResolverRuta(string ruta)
        {
            if (string.IsNullOrWhiteSpace(ruta))
                throw new InvalidOperationException("La fuente no tiene una ruta de archivo registrada.");
            var fullPath = Path.IsPathRooted(ruta)
                ? Path.GetFullPath(ruta)
                : Path.GetFullPath(Path.Combine(_storageRoot, ruta.Replace('/', Path.DirectorySeparatorChar)));
            if (!Path.IsPathRooted(ruta) && !EsMismaOSubcarpeta(fullPath, _storageRoot))
                throw new InvalidOperationException("La ruta del seguimiento quedó fuera del repositorio administrado.");
            return fullPath;
        }

        private static bool EsMismaOSubcarpeta(string candidate, string root)
        {
            var normalizedCandidate = Path.GetFullPath(candidate).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            return normalizedCandidate.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase);
        }

        private static string LeerTexto(IXLRow row, IReadOnlyDictionary<string, int> headers, string header)
            => headers.TryGetValue(header, out var column) ? ObtenerTextoSeguro(row.Cell(column)) : null;

        private static string LeerTextoPorPrefijo(IXLRow row, IReadOnlyDictionary<string, int> headers, string prefix)
        {
            var match = headers.FirstOrDefault(x => x.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
            return match.Value > 0 ? ObtenerTextoSeguro(row.Cell(match.Value)) : null;
        }

        private static decimal? LeerDecimal(IXLRow row, IReadOnlyDictionary<string, int> headers, string header)
        {
            if (!headers.TryGetValue(header, out var column)) return null;
            var cell = row.Cell(column);
            if (cell.TryGetValue<decimal>(out var decimalValue)) return decimalValue;
            var text = ObtenerTextoSeguro(cell);
            var match = NumeroRegex.Match(text ?? string.Empty);
            if (!match.Success) return null;
            return decimal.TryParse(match.Value.Replace(",", string.Empty), NumberStyles.Number | NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
        }

        private static int? LeerEntero(IXLRow row, IReadOnlyDictionary<string, int> headers, string header)
        {
            var value = LeerDecimal(row, headers, header);
            if (!value.HasValue || value.Value > int.MaxValue || value.Value < int.MinValue) return null;
            return Convert.ToInt32(decimal.Truncate(value.Value));
        }

        private static decimal? LeerPorcentaje(IXLRow row, IReadOnlyDictionary<string, int> headers, string header)
        {
            if (!headers.TryGetValue(header, out var column)) return null;
            var cell = row.Cell(column);
            decimal? value = null;
            if (cell.TryGetValue<decimal>(out var numeric)) value = numeric;
            else
            {
                var text = ObtenerTextoSeguro(cell);
                var match = NumeroRegex.Match(text ?? string.Empty);
                if (match.Success && decimal.TryParse(match.Value.Replace(",", string.Empty), NumberStyles.Number,
                    CultureInfo.InvariantCulture, out var parsed))
                    value = text.Contains('%') ? parsed / 100m : parsed;
            }
            if (value > 1m && value <= 100m) value /= 100m;
            return value is >= 0m and <= 1m ? value : null;
        }

        private static DateTime? LeerFecha(IXLRow row, IReadOnlyDictionary<string, int> headers, string header)
        {
            if (!headers.TryGetValue(header, out var column)) return null;
            var cell = row.Cell(column);
            if (cell.TryGetValue<DateTime>(out var dateValue)) return dateValue.Date;
            var text = ObtenerTextoSeguro(cell);
            if (DateTime.TryParse(text, CultureInfo.GetCultureInfo("es-MX"), DateTimeStyles.AllowWhiteSpaces, out var parsed)
                || DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out parsed))
                return parsed.Date;
            return null;
        }

        private static bool? LeerBit(IXLRow row, IReadOnlyDictionary<string, int> headers, string header)
        {
            var text = LeerTexto(row, headers, header);
            return string.IsNullOrWhiteSpace(text) ? null : EsVerdadero(text);
        }

        private static bool? LeerBitPorPrefijo(IXLRow row, IReadOnlyDictionary<string, int> headers, string prefix)
        {
            var text = LeerTextoPorPrefijo(row, headers, prefix);
            return string.IsNullOrWhiteSpace(text) ? null : EsVerdadero(text);
        }

        private static bool EsVerdadero(string value)
        {
            var normalized = NormalizarEncabezado(value);
            return normalized is "1" or "SI" or "S" or "TRUE" or "VERDADERO" or "X" or "VISIBLE";
        }

        private static string ObtenerTextoSeguro(IXLCell cell)
        {
            try
            {
                var value = cell.GetFormattedString()?.Trim();
                return value is "#VALUE!" or "#REF!" or "#N/A" or "#DIV/0!" ? null : value;
            }
            catch
            {
                return null;
            }
        }

        private static List<string> ExtraerClaves(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return new List<string>();
            return ClaveRegex.Matches(value.Replace('–', '-').Replace('—', '-'))
                .Select(x => NormalizarClave(x.Value))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string NormalizarClave(string value)
            => Regex.Replace((value ?? string.Empty).Replace('–', '-').Replace('—', '-').ToUpperInvariant(), @"\s+", string.Empty).Trim();

        private static string NormalizarCodigoUnidad(string value)
            => Regex.Replace((value ?? string.Empty).Replace('–', '-').Replace('—', '-').ToUpperInvariant(), @"\s+", " ").Trim();

        private static string NormalizarEncabezado(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            var builder = new StringBuilder();
            foreach (var character in value.Normalize(NormalizationForm.FormD))
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark) continue;
                if (char.IsLetterOrDigit(character)) builder.Append(char.ToUpperInvariant(character));
            }
            return builder.ToString();
        }

        private static string CrearHash(string value)
            => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value ?? string.Empty)));

        private static string Limitar(string value, int maximo)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim()[..Math.Min(value.Trim().Length, maximo)];

        private sealed class LoteContexto
        {
            public long LoteId { get; set; }
            public Guid LoteUid { get; set; }
            public string Nombre { get; set; }
            public string TipoActualizacion { get; set; }
            public DateTime FechaCorte { get; set; }
            public string Estado { get; set; }
            public long FuenteId { get; set; }
            public string NombreOriginal { get; set; }
            public string Extension { get; set; }
            public string RutaArchivo { get; set; }
            public string HashSha256 { get; set; }
        }

        private sealed class ClaveVigente
        {
            public long ProyectoId { get; set; }
            public string ClaveProyecto { get; set; }
            public string TipoClave { get; set; }
        }

        private sealed class RelacionDetectada
        {
            public string CodigoUnico { get; set; }
            public string CodigoPem { get; set; }
            public long ProyectoId { get; set; }
            public string TipoCoincidencia { get; set; }
        }

        private sealed class RegistroSeguimiento
        {
            public long SeguimientoUnidadId { get; set; }
            public long LoteId { get; set; }
            public long FuenteId { get; set; }
            public string PerfilVersion { get; set; }
            public string Hoja { get; set; }
            public int NumeroFila { get; set; }
            public string CodigoUnico { get; set; }
            public string CodigoPem { get; set; }
            public string IdSepi { get; set; }
            public string IdProcedimiento { get; set; }
            public string ProyectoEnFases { get; set; }
            public string FaseInicial { get; set; }
            public string FasesSubsecuentes { get; set; }
            public bool TieneFases { get; set; }
            public bool? ActivoAdministracionActual { get; set; }
            public bool? AdministracionActual { get; set; }
            public string NombreProyecto { get; set; }
            public string Categoria { get; set; }
            public decimal? Mva { get; set; }
            public decimal? Mvar { get; set; }
            public decimal? KmC { get; set; }
            public string OtraMetaFisica { get; set; }
            public decimal? ImporteMdp { get; set; }
            public string FinanciamientoCfe { get; set; }
            public string FinanciamientoPormenorizado { get; set; }
            public int? AnioInstruccion { get; set; }
            public DateTime? FechaInicioConcursoProgramada { get; set; }
            public DateTime? FechaInicioConcursoReal { get; set; }
            public DateTime? FechaAdjudicacionProgramada { get; set; }
            public DateTime? FechaAdjudicacionReal { get; set; }
            public DateTime? FechaFirmaContratoProgramada { get; set; }
            public DateTime? FechaFirmaContratoReal { get; set; }
            public DateTime? FechaInicioConstruccionProgramada { get; set; }
            public DateTime? FechaInicioConstruccionReal { get; set; }
            public DateTime? FechaTerminoConstruccion { get; set; }
            public string FeoIndicada { get; set; }
            public string FeoFactible { get; set; }
            public int? PlazoEjecucionDias { get; set; }
            public decimal? AvanceProgramado { get; set; }
            public decimal? AvanceReal { get; set; }
            public string ComentariosPpt { get; set; }
            public string NotaPpt { get; set; }
            public string NombrePormenorizado { get; set; }
            public string ElementosEquipos { get; set; }
            public string ActualizacionEstatus { get; set; }
            public DateTime? UltimaActualizacionFecha { get; set; }
            public string DetalleUltimaActualizacion { get; set; }
            public string OrigenUltimaActualizacion { get; set; }
            public string ComentariosInternos { get; set; }
            public bool? Visible { get; set; }
            public string DatosOrigenJson { get; set; }
            public string HashFila { get; set; }
            public string UsuarioRegistro { get; set; }
        }

        private sealed class PreparacionInterna
        {
            public LoteContexto Contexto { get; set; }
            public List<RegistroSeguimiento> Registros { get; set; } = new();
            public List<RelacionDetectada> Relaciones { get; set; } = new();
            public List<PamSeguimientoClavePendiente> Pendientes { get; set; } = new();
            public bool YaIncorporado { get; set; }
        }
    }
}
