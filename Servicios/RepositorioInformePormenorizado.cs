using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using NSIE.Models;

namespace NSIE.Servicios
{
    public interface IRepositorioInformePormenorizado
    {
        Task<List<ProyectoModernizacionRegistro>> ObtenerAsync(string busqueda = null, string etapa = null, string tipoFinanciamiento = null, string universo = null);
        Task<ProyectoModernizacionRegistro> ObtenerPorIdAsync(int proyectoModernizacionId);
        Task<int> CrearAsync(ProyectoModernizacionRegistro registro);
        Task ActualizarAsync(ProyectoModernizacionRegistro registro);
        Task EliminarAsync(int proyectoModernizacionId);
        Task<int> ReemplazarDatosAsync(IReadOnlyCollection<ProyectoModernizacionRegistro> registros, string fuenteArchivo);
        Task<List<string>> ObtenerEtapasAsync();
        Task<List<string>> ObtenerTiposFinanciamientoAsync();
        Task<List<string>> ObtenerUniversosAsync();
    }

    public class RepositorioInformePormenorizado : IRepositorioInformePormenorizado
    {
        private const string TableName = "dgmesnie.InformePormenorizadoModernizacion";
        private readonly string _connectionString;

        public RepositorioInformePormenorizado(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<ProyectoModernizacionRegistro>> ObtenerAsync(string busqueda = null, string etapa = null, string tipoFinanciamiento = null, string universo = null)
        {
            const string sql = @"
SELECT *
FROM dgmesnie.InformePormenorizadoModernizacion
WHERE Activo = 1
  AND (@Busqueda IS NULL OR NombreProyecto LIKE @BusquedaLike OR GRT LIKE @BusquedaLike OR ClavePem LIKE @BusquedaLike OR EstadoRealProyecto LIKE @BusquedaLike)
  AND (@Etapa IS NULL OR EtapaProyecto = @Etapa)
  AND (@TipoFinanciamiento IS NULL OR TipoFinanciamiento = @TipoFinanciamiento)
  AND (@Universo IS NULL OR UniversoPresentacionPresidencia = @Universo)
ORDER BY Numero;";

            using var connection = new SqlConnection(_connectionString);
            var registros = await connection.QueryAsync<ProyectoModernizacionRegistro>(sql, new
            {
                Busqueda = Normalize(busqueda),
                BusquedaLike = string.IsNullOrWhiteSpace(busqueda) ? null : $"%{busqueda.Trim()}%",
                Etapa = Normalize(etapa),
                TipoFinanciamiento = Normalize(tipoFinanciamiento),
                Universo = Normalize(universo)
            });

            return registros.ToList();
        }

        public async Task<ProyectoModernizacionRegistro> ObtenerPorIdAsync(int proyectoModernizacionId)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<ProyectoModernizacionRegistro>(
                $"SELECT * FROM {TableName} WHERE ProyectoModernizacionId = @ProyectoModernizacionId;",
                new { ProyectoModernizacionId = proyectoModernizacionId });
        }

        public async Task<int> CrearAsync(ProyectoModernizacionRegistro registro)
        {
            const string sql = @"
INSERT INTO dgmesnie.InformePormenorizadoModernizacion
(
    Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto,
    MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible,
    PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto,
    ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion,
    UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, FechaCarga, Activo
)
VALUES
(
    @Numero, @NumeroOriginal, @GRT, @NombreProyecto, @TipoFinanciamiento, @AnioInstruccion, @EtapaProyecto,
    @MontoProyectoMdp, @ElementosEquiposAsociados, @FechaEstimadaInicio, @FeoIndicadaOficioSener, @FeoFactible,
    @PorcentajeAvanceEjecucion, @CircunstanciasAtrasos, @AccionesMitigacionCorreccion, @EstadoRealProyecto,
    @ComentariosNivelPriorizacion, @ClavePem, @ClasificacionSener, @FechaProgramacionTrimestre, @QuincenaPublicacion,
    @UniversoPresentacionPresidencia, @Mva, @Mvar, @KmC, @FuenteArchivo, SYSUTCDATETIME(), @Activo
);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteScalarAsync<int>(sql, BuildParameters(registro));
        }

        public async Task ActualizarAsync(ProyectoModernizacionRegistro registro)
        {
            const string sql = @"
UPDATE dgmesnie.InformePormenorizadoModernizacion
SET Numero = @Numero,
    NumeroOriginal = @NumeroOriginal,
    GRT = @GRT,
    NombreProyecto = @NombreProyecto,
    TipoFinanciamiento = @TipoFinanciamiento,
    AnioInstruccion = @AnioInstruccion,
    EtapaProyecto = @EtapaProyecto,
    MontoProyectoMdp = @MontoProyectoMdp,
    ElementosEquiposAsociados = @ElementosEquiposAsociados,
    FechaEstimadaInicio = @FechaEstimadaInicio,
    FeoIndicadaOficioSener = @FeoIndicadaOficioSener,
    FeoFactible = @FeoFactible,
    PorcentajeAvanceEjecucion = @PorcentajeAvanceEjecucion,
    CircunstanciasAtrasos = @CircunstanciasAtrasos,
    AccionesMitigacionCorreccion = @AccionesMitigacionCorreccion,
    EstadoRealProyecto = @EstadoRealProyecto,
    ComentariosNivelPriorizacion = @ComentariosNivelPriorizacion,
    ClavePem = @ClavePem,
    ClasificacionSener = @ClasificacionSener,
    FechaProgramacionTrimestre = @FechaProgramacionTrimestre,
    QuincenaPublicacion = @QuincenaPublicacion,
    UniversoPresentacionPresidencia = @UniversoPresentacionPresidencia,
    Mva = @Mva,
    Mvar = @Mvar,
    KmC = @KmC,
    Activo = @Activo,
    FechaActualizacion = SYSUTCDATETIME()
WHERE ProyectoModernizacionId = @ProyectoModernizacionId;";

            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, BuildParameters(registro));
        }

        public async Task EliminarAsync(int proyectoModernizacionId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(
                $"DELETE FROM {TableName} WHERE ProyectoModernizacionId = @ProyectoModernizacionId;",
                new { ProyectoModernizacionId = proyectoModernizacionId });
        }

        public async Task<int> ReemplazarDatosAsync(IReadOnlyCollection<ProyectoModernizacionRegistro> registros, string fuenteArchivo)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                await connection.ExecuteAsync($"DELETE FROM {TableName};", transaction: transaction);

                const string insertSql = @"
INSERT INTO dgmesnie.InformePormenorizadoModernizacion
(
    Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto,
    MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio, FeoIndicadaOficioSener, FeoFactible,
    PorcentajeAvanceEjecucion, CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto,
    ComentariosNivelPriorizacion, ClavePem, ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion,
    UniversoPresentacionPresidencia, Mva, Mvar, KmC, FuenteArchivo, FechaCarga, Activo
)
VALUES
(
    @Numero, @NumeroOriginal, @GRT, @NombreProyecto, @TipoFinanciamiento, @AnioInstruccion, @EtapaProyecto,
    @MontoProyectoMdp, @ElementosEquiposAsociados, @FechaEstimadaInicio, @FeoIndicadaOficioSener, @FeoFactible,
    @PorcentajeAvanceEjecucion, @CircunstanciasAtrasos, @AccionesMitigacionCorreccion, @EstadoRealProyecto,
    @ComentariosNivelPriorizacion, @ClavePem, @ClasificacionSener, @FechaProgramacionTrimestre, @QuincenaPublicacion,
    @UniversoPresentacionPresidencia, @Mva, @Mvar, @KmC, @FuenteArchivo, SYSUTCDATETIME(), 1
);";

                foreach (var registro in registros)
                {
                    registro.FuenteArchivo = fuenteArchivo;
                    await connection.ExecuteAsync(insertSql, BuildParameters(registro), transaction);
                }

                await transaction.CommitAsync();
                return registros.Count;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public Task<List<string>> ObtenerEtapasAsync() => ObtenerCatalogoAsync("EtapaProyecto");

        public Task<List<string>> ObtenerTiposFinanciamientoAsync() => ObtenerCatalogoAsync("TipoFinanciamiento");

        public Task<List<string>> ObtenerUniversosAsync() => ObtenerCatalogoAsync("UniversoPresentacionPresidencia");

        private async Task<List<string>> ObtenerCatalogoAsync(string columnName)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = $@"
SELECT DISTINCT {columnName}
FROM {TableName}
WHERE Activo = 1 AND NULLIF(LTRIM(RTRIM({columnName})), '') IS NOT NULL
ORDER BY {columnName};";

            var registros = await connection.QueryAsync<string>(sql);
            return registros.ToList();
        }

        private static object BuildParameters(ProyectoModernizacionRegistro registro)
        {
            return new
            {
                registro.ProyectoModernizacionId,
                registro.Numero,
                registro.NumeroOriginal,
                GRT = Normalize(registro.GRT),
                NombreProyecto = NormalizeRequired(registro.NombreProyecto),
                TipoFinanciamiento = Normalize(registro.TipoFinanciamiento),
                registro.AnioInstruccion,
                EtapaProyecto = Normalize(registro.EtapaProyecto),
                registro.MontoProyectoMdp,
                ElementosEquiposAsociados = Normalize(registro.ElementosEquiposAsociados),
                FechaEstimadaInicio = Normalize(registro.FechaEstimadaInicio),
                FeoIndicadaOficioSener = Normalize(registro.FeoIndicadaOficioSener),
                FeoFactible = Normalize(registro.FeoFactible),
                registro.PorcentajeAvanceEjecucion,
                CircunstanciasAtrasos = Normalize(registro.CircunstanciasAtrasos),
                AccionesMitigacionCorreccion = Normalize(registro.AccionesMitigacionCorreccion),
                EstadoRealProyecto = Normalize(registro.EstadoRealProyecto),
                ComentariosNivelPriorizacion = Normalize(registro.ComentariosNivelPriorizacion),
                ClavePem = Normalize(registro.ClavePem),
                ClasificacionSener = Normalize(registro.ClasificacionSener),
                FechaProgramacionTrimestre = Normalize(registro.FechaProgramacionTrimestre),
                QuincenaPublicacion = Normalize(registro.QuincenaPublicacion),
                UniversoPresentacionPresidencia = Normalize(registro.UniversoPresentacionPresidencia),
                registro.Mva,
                registro.Mvar,
                registro.KmC,
                FuenteArchivo = Normalize(registro.FuenteArchivo),
                registro.Activo
            };
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static string NormalizeRequired(string value)
        {
            return (value ?? string.Empty).Trim();
        }
    }
}