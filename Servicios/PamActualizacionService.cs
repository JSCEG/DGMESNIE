using System.Data;
using System.IO.Compression;
using System.Security.Cryptography;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using NSIE.Models;

namespace NSIE.Servicios
{
    public interface IPamActualizacionService
    {
        Task<List<PamLoteResumen>> ObtenerLotesRecientesAsync(int limite = 10);
        Task<PamLoteCreadoResultado> CrearLoteAsync(PamNuevaActualizacionInput input, int? usuarioId, string usuarioNombre);
    }

    public class PamActualizacionService : IPamActualizacionService
    {
        private const long MaximoPorArchivo = 200L * 1024 * 1024;
        private const long MaximoPorLote = 240L * 1024 * 1024;
        private const int MaximoArchivos = 12;
        private static readonly HashSet<string> ExtensionesPermitidas = new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".ppt", ".pptx", ".xls", ".xlsx"
        };

        private readonly string _connectionString;
        private readonly string _storageRoot;

        public PamActualizacionService(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");

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

        public async Task<List<PamLoteResumen>> ObtenerLotesRecientesAsync(int limite = 10)
        {
            const string sql = @"
SELECT TOP (@Limite)
    l.LoteId, l.LoteUid, l.Nombre, l.FechaCorte, l.Estado, l.TotalArchivos,
    l.TotalRegistrosDetectados, l.TotalCambiosPropuestos, l.TotalObservados,
    l.FechaRegistroUtc, l.UsuarioNombre,
    archivos.Archivos,
    archivos.PendientesExtraccion,
    archivos.RequierenConversion
FROM dgmesnie.PAMLoteActualizacion l
OUTER APPLY
(
    SELECT
        STRING_AGG(CAST(lf.NombreOriginal AS NVARCHAR(MAX)), N', ') AS Archivos,
        SUM(CASE WHEN lf.EstadoExtraccion = N'Pendiente' THEN 1 ELSE 0 END) AS PendientesExtraccion,
        SUM(CASE WHEN lf.EstadoExtraccion = N'Requiere conversión' THEN 1 ELSE 0 END) AS RequierenConversion
    FROM dgmesnie.PAMLoteFuente lf
    WHERE lf.LoteId = l.LoteId
) archivos
ORDER BY l.FechaRegistroUtc DESC, l.LoteId DESC;";

            await using var connection = new SqlConnection(_connectionString);
            return (await connection.QueryAsync<PamLoteResumen>(sql, new { Limite = Math.Clamp(limite, 1, 50) })).ToList();
        }

        public async Task<PamLoteCreadoResultado> CrearLoteAsync(PamNuevaActualizacionInput input, int? usuarioId, string usuarioNombre)
        {
            Validar(input);
            Directory.CreateDirectory(_storageRoot);

            var loteUid = Guid.NewGuid();
            var fechaRegistroUtc = DateTime.UtcNow;
            var relativeFolder = Path.Combine(
                fechaRegistroUtc.ToString("yyyy"),
                fechaRegistroUtc.ToString("MM"),
                loteUid.ToString("N"));
            var lotFolder = Path.GetFullPath(Path.Combine(_storageRoot, relativeFolder));
            if (!EsMismaOSubcarpeta(lotFolder, _storageRoot) || string.Equals(lotFolder, _storageRoot, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("No fue posible resolver la carpeta segura del lote.");

            Directory.CreateDirectory(lotFolder);
            var preparados = new List<ArchivoPreparado>();
            var limpiarArchivosAlFallar = true;
            var resultado = new PamLoteCreadoResultado
            {
                LoteUid = loteUid,
                TotalArchivos = input.Archivos.Count
            };

            try
            {
                foreach (var archivo in input.Archivos)
                    preparados.Add(await GuardarOriginalAsync(archivo, lotFolder, relativeFolder));

                if (preparados.Select(x => x.HashSha256).Distinct(StringComparer.OrdinalIgnoreCase).Count() != preparados.Count)
                    throw new InvalidOperationException("El lote contiene archivos repetidos.");

                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                await using var transaction = await connection.BeginTransactionAsync();

                try
                {
                    const string insertLote = @"
INSERT dgmesnie.PAMLoteActualizacion
    (LoteUid, Nombre, FechaCorte, Notas, Estado, TotalArchivos, UsuarioId, UsuarioNombre)
VALUES
    (@LoteUid, @Nombre, @FechaCorte, @Notas, N'Cargado', @TotalArchivos, @UsuarioId, @UsuarioNombre);
SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

                    var loteId = await connection.ExecuteScalarAsync<long>(insertLote, new
                    {
                        LoteUid = loteUid,
                        Nombre = input.Nombre.Trim(),
                        FechaCorte = input.FechaCorte!.Value.Date,
                        Notas = Normalizar(input.Notas),
                        TotalArchivos = preparados.Count,
                        UsuarioId = usuarioId,
                        UsuarioNombre = Normalizar(usuarioNombre)
                    }, transaction);

                    resultado.LoteId = loteId;

                    foreach (var archivo in preparados)
                    {
                        var fuenteExistente = await connection.QuerySingleOrDefaultAsync<FuenteExistente>(
                            @"SELECT FuenteId, RutaArchivo, Activo
                              FROM dgmesnie.PAMFuente WITH (UPDLOCK, HOLDLOCK)
                              WHERE HashSha256 = @HashSha256;",
                            new { archivo.HashSha256 }, transaction);
                        long fuenteId;

                        if (fuenteExistente == null)
                        {
                            const string insertFuente = @"
INSERT dgmesnie.PAMFuente
    (NombreDocumento, TipoDocumento, FechaDocumento, FechaCorte, RutaArchivo,
     VersionDocumento, HashSha256, Observaciones, UsuarioRegistro, Activo)
VALUES
    (@NombreDocumento, @TipoDocumento, NULL, @FechaCorte, @RutaArchivo,
     @VersionDocumento, @HashSha256, @Observaciones, @UsuarioRegistro, 1);
SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

                            fuenteId = await connection.ExecuteScalarAsync<long>(insertFuente, new
                            {
                                NombreDocumento = archivo.NombreOriginal,
                                archivo.TipoDocumento,
                                FechaCorte = input.FechaCorte.Value.Date,
                                RutaArchivo = archivo.RutaRelativa,
                                VersionDocumento = $"Lote {loteId}",
                                archivo.HashSha256,
                                Observaciones = "Archivo original registrado desde el centro de actualización diaria.",
                                UsuarioRegistro = Normalizar(usuarioNombre)
                            }, transaction);
                            resultado.FuentesNuevas++;
                        }
                        else
                        {
                            fuenteId = fuenteExistente.FuenteId;
                            resultado.FuentesReutilizadas++;
                            if (fuenteExistente.Activo
                                && await OriginalAdministradoCoincideAsync(fuenteExistente.RutaArchivo, archivo.HashSha256))
                            {
                                File.Delete(archivo.RutaFisica);
                            }
                            else
                            {
                                const string repararFuente = @"
UPDATE dgmesnie.PAMFuente
SET RutaArchivo = @RutaArchivo,
    Activo = 1,
    Observaciones = LEFT(CONCAT(COALESCE(Observaciones + N' | ', N''),
        N'Original reubicado en el repositorio documental administrado.'), 1000)
WHERE FuenteId = @FuenteId;";
                                await connection.ExecuteAsync(repararFuente, new
                                {
                                    RutaArchivo = archivo.RutaRelativa,
                                    FuenteId = fuenteId
                                }, transaction);
                            }
                        }

                        const string insertLoteFuente = @"
INSERT dgmesnie.PAMLoteFuente
    (LoteId, FuenteId, CargaId, Papel, NombreOriginal, Extension, TipoContenido,
     TamanoBytes, FirmaValidada, EstadoExtraccion, MensajeExtraccion)
VALUES
    (@LoteId, @FuenteId, NULL, @Papel, @NombreOriginal, @Extension, @TipoContenido,
     @TamanoBytes, 1, @EstadoExtraccion, @MensajeExtraccion);";
                        await connection.ExecuteAsync(insertLoteFuente, new
                        {
                            LoteId = loteId,
                            FuenteId = fuenteId,
                            archivo.Papel,
                            archivo.NombreOriginal,
                            archivo.Extension,
                            archivo.TipoContenido,
                            archivo.TamanoBytes,
                            archivo.EstadoExtraccion,
                            archivo.MensajeExtraccion
                        }, transaction);
                    }

                    await transaction.CommitAsync();
                    return resultado;
                }
                catch
                {
                    var rollbackConfirmado = false;
                    try
                    {
                        await transaction.RollbackAsync();
                        rollbackConfirmado = true;
                    }
                    catch
                    {
                        // Si la conexión cayó durante COMMIT, primero se verifica el LoteUid.
                    }

                    if (!rollbackConfirmado)
                    {
                        var verificacion = await BuscarLoteRegistradoAsync(loteUid);
                        if (verificacion.Verificada && verificacion.LoteId.HasValue)
                        {
                            resultado.LoteId = verificacion.LoteId.Value;
                            return resultado;
                        }

                        limpiarArchivosAlFallar = verificacion.Verificada;
                    }
                    throw;
                }
            }
            catch
            {
                if (limpiarArchivosAlFallar)
                    EliminarCarpetaSilenciosamente(lotFolder);
                throw;
            }
        }

        private async Task<ArchivoPreparado> GuardarOriginalAsync(IFormFile archivo, string lotFolder, string relativeFolder)
        {
            var originalName = Path.GetFileName(archivo.FileName);
            var extension = Path.GetExtension(originalName).ToLowerInvariant();
            await ValidarFirmaAsync(archivo, extension);

            var storedName = $"{Guid.NewGuid():N}{extension}";
            var physicalPath = Path.GetFullPath(Path.Combine(lotFolder, storedName));
            if (!EsMismaOSubcarpeta(physicalPath, lotFolder))
                throw new InvalidOperationException("No fue posible resolver la ruta segura del archivo.");

            await using (var output = new FileStream(physicalPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true))
                await archivo.CopyToAsync(output);

            if (extension is ".pptx" or ".xlsx")
                ValidarEstructuraOpenXml(physicalPath, extension, originalName);

            await using var input = new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
            var hash = Convert.ToHexString(await SHA256.HashDataAsync(input));
            var requiereConversion = extension is ".xls" or ".ppt";

            return new ArchivoPreparado
            {
                NombreOriginal = originalName,
                Extension = extension,
                TipoContenido = NormalizarTipoContenido(archivo.ContentType),
                TamanoBytes = archivo.Length,
                HashSha256 = hash,
                RutaFisica = physicalPath,
                RutaRelativa = Path.Combine(relativeFolder, storedName).Replace('\\', '/'),
                TipoDocumento = extension is ".xls" or ".xlsx" ? "Excel" : extension is ".ppt" or ".pptx" ? "Presentación" : "PDF",
                Papel = extension is ".xls" or ".xlsx" ? "Datos" : "Evidencia",
                EstadoExtraccion = requiereConversion ? "Requiere conversión" : "Pendiente",
                MensajeExtraccion = requiereConversion
                    ? "Original preservado; la extracción automática requerirá conversión interna."
                    : "Original preservado; pendiente de análisis y comparación."
            };
        }

        private static void Validar(PamNuevaActualizacionInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (string.IsNullOrWhiteSpace(input.Nombre)) throw new InvalidOperationException("Escribe un nombre para la actualización.");
            if (input.Nombre.Trim().Length > 200) throw new InvalidOperationException("El nombre de la actualización no puede superar 200 caracteres.");
            if (!string.IsNullOrWhiteSpace(input.Notas) && input.Notas.Trim().Length > 1000) throw new InvalidOperationException("La nota no puede superar 1000 caracteres.");
            if (!input.FechaCorte.HasValue) throw new InvalidOperationException("Indica la fecha de corte.");
            if (input.FechaCorte.Value.Date > DateTime.Today) throw new InvalidOperationException("La fecha de corte no puede estar en el futuro.");
            if (input.Archivos == null || input.Archivos.Count == 0) throw new InvalidOperationException("Selecciona al menos un archivo.");
            if (input.Archivos.Count > MaximoArchivos) throw new InvalidOperationException($"Se permiten hasta {MaximoArchivos} archivos por lote.");

            long totalBytes = 0;
            foreach (var archivo in input.Archivos)
            {
                var originalName = Path.GetFileName(archivo.FileName);
                var extension = Path.GetExtension(originalName);
                if (string.IsNullOrWhiteSpace(originalName) || originalName.Length > 500) throw new InvalidOperationException("Uno de los archivos tiene un nombre inválido o demasiado largo.");
                if (!ExtensionesPermitidas.Contains(extension)) throw new InvalidOperationException($"El archivo {originalName} no es PDF, PowerPoint o Excel.");
                if (archivo.Length <= 0) throw new InvalidOperationException($"El archivo {originalName} está vacío.");
                if (archivo.Length > MaximoPorArchivo) throw new InvalidOperationException($"El archivo {originalName} supera el límite de 200 MB.");
                totalBytes = checked(totalBytes + archivo.Length);
            }

            if (totalBytes > MaximoPorLote) throw new InvalidOperationException("El conjunto de archivos supera el límite de 240 MB por actualización.");
        }

        private static async Task ValidarFirmaAsync(IFormFile archivo, string extension)
        {
            var header = new byte[8];
            await using var stream = archivo.OpenReadStream();
            var read = await stream.ReadAsync(header);
            if (read < 4) throw new InvalidOperationException($"No fue posible validar {archivo.FileName}.");

            var isPdf = header[0] == 0x25 && header[1] == 0x50 && header[2] == 0x44 && header[3] == 0x46;
            var isZip = header[0] == 0x50 && header[1] == 0x4B && header[2] == 0x03 && header[3] == 0x04;
            var isOle = read >= 8 && header.SequenceEqual(new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 });
            var valid = extension == ".pdf" ? isPdf : extension is ".pptx" or ".xlsx" ? isZip : isOle;
            if (!valid) throw new InvalidOperationException($"El contenido de {archivo.FileName} no coincide con su extensión.");
        }

        private static void ValidarEstructuraOpenXml(string physicalPath, string extension, string originalName)
        {
            try
            {
                using var stream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false);
                var requiredEntry = extension == ".xlsx" ? "xl/workbook.xml" : "ppt/presentation.xml";
                var hasContentTypes = archive.Entries.Any(entry =>
                    string.Equals(entry.FullName, "[Content_Types].xml", StringComparison.OrdinalIgnoreCase));
                var hasRequiredEntry = archive.Entries.Any(entry =>
                    string.Equals(entry.FullName, requiredEntry, StringComparison.OrdinalIgnoreCase));

                if (!hasContentTypes || !hasRequiredEntry)
                    throw new InvalidOperationException($"El archivo {originalName} no contiene una estructura Office válida para su extensión.");
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (InvalidDataException)
            {
                throw new InvalidOperationException($"El archivo {originalName} no es un paquete Office válido.");
            }
        }

        private async Task<bool> OriginalAdministradoCoincideAsync(string rutaArchivo, string hashEsperado)
        {
            if (string.IsNullOrWhiteSpace(rutaArchivo) || Path.IsPathRooted(rutaArchivo)) return false;

            try
            {
                var normalized = rutaArchivo.Replace('/', Path.DirectorySeparatorChar);
                var physicalPath = Path.GetFullPath(Path.Combine(_storageRoot, normalized));
                if (!EsMismaOSubcarpeta(physicalPath, _storageRoot) || !File.Exists(physicalPath)) return false;

                await using var stream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
                var actualHash = Convert.ToHexString(await SHA256.HashDataAsync(stream));
                return string.Equals(actualHash, hashEsperado, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private async Task<(bool Verificada, long? LoteId)> BuscarLoteRegistradoAsync(Guid loteUid)
        {
            try
            {
                const string sql = "SELECT TOP (1) LoteId FROM dgmesnie.PAMLoteActualizacion WHERE LoteUid = @LoteUid;";
                await using var connection = new SqlConnection(_connectionString);
                var loteId = await connection.QuerySingleOrDefaultAsync<long?>(sql, new { LoteUid = loteUid });
                return (true, loteId);
            }
            catch
            {
                return (false, null);
            }
        }

        private static bool EsMismaOSubcarpeta(string candidatePath, string rootPath)
        {
            var candidate = Path.GetFullPath(candidatePath);
            var root = Path.GetFullPath(rootPath);
            var relative = Path.GetRelativePath(root, candidate);
            return !Path.IsPathRooted(relative)
                && relative != ".."
                && !relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !relative.StartsWith($"..{Path.AltDirectorySeparatorChar}", StringComparison.Ordinal);
        }

        private static void EliminarCarpetaSilenciosamente(string path)
        {
            try
            {
                if (Directory.Exists(path)) Directory.Delete(path, recursive: true);
            }
            catch
            {
                // El error original debe conservarse; la limpieza podrá retomarse operativamente.
            }
        }

        private static string Normalizar(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static string NormalizarTipoContenido(string value)
        {
            var normalized = Normalizar(value);
            return normalized?.Length > 150 ? normalized[..150] : normalized;
        }

        private class ArchivoPreparado
        {
            public string NombreOriginal { get; set; }
            public string Extension { get; set; }
            public string TipoContenido { get; set; }
            public long TamanoBytes { get; set; }
            public string HashSha256 { get; set; }
            public string RutaFisica { get; set; }
            public string RutaRelativa { get; set; }
            public string TipoDocumento { get; set; }
            public string Papel { get; set; }
            public string EstadoExtraccion { get; set; }
            public string MensajeExtraccion { get; set; }
        }

        private class FuenteExistente
        {
            public long FuenteId { get; set; }
            public string RutaArchivo { get; set; }
            public bool Activo { get; set; }
        }
    }
}
