using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using NSIE.Models;

namespace NSIE.Servicios
{
    public interface IRepositorioInformePormenorizado
    {
        Task<List<ProyectoModernizacionRegistro>> ObtenerAsync(string busqueda = null, string etapa = null, string tipoFinanciamiento = null, string universo = null);
        Task<List<ProyectoModernizacionListado>> ObtenerPaginaAsync(string busqueda, string etapa, string tipoFinanciamiento, string universo, int pagina, int tamanoPagina);
        Task<InformePormenorizadoAgregados> ObtenerAgregadosAsync(string busqueda, string etapa, string tipoFinanciamiento, string universo);
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

        private const string FiltroSql = @"Activo = 1
  AND (@Busqueda IS NULL OR NombreProyecto LIKE @BusquedaLike OR GRT LIKE @BusquedaLike OR ClavePem LIKE @BusquedaLike OR EstadoRealProyecto LIKE @BusquedaLike)
  AND (@Etapa IS NULL OR EtapaProyecto = @Etapa)
  AND (@TipoFinanciamiento IS NULL OR TipoFinanciamiento = @TipoFinanciamiento)
  AND (@Universo IS NULL OR UniversoPresentacionPresidencia = @Universo)";

        private static object BuildFilterParameters(string busqueda, string etapa, string tipoFinanciamiento, string universo, int? offset = null, int? tamanoPagina = null)
        {
            return new
            {
                Busqueda = Normalize(busqueda),
                BusquedaLike = string.IsNullOrWhiteSpace(busqueda) ? null : $"%{busqueda.Trim()}%",
                Etapa = Normalize(etapa),
                TipoFinanciamiento = Normalize(tipoFinanciamiento),
                Universo = Normalize(universo),
                Offset = offset ?? 0,
                TamanoPagina = tamanoPagina ?? 20,
                UniversoSi = "Sí"
            };
        }

        public async Task<List<ProyectoModernizacionListado>> ObtenerPaginaAsync(string busqueda, string etapa, string tipoFinanciamiento, string universo, int pagina, int tamanoPagina)
        {
            var sql = $@"
SELECT ProyectoModernizacionId, Numero, NombreProyecto, GRT, ClavePem, EtapaProyecto,
       TipoFinanciamiento, MontoProyectoMdp, PorcentajeAvanceEjecucion, UniversoPresentacionPresidencia,
       COUNT(*) OVER() AS TotalFiltrado
FROM {TableName}
WHERE {FiltroSql}
ORDER BY Numero
OFFSET @Offset ROWS FETCH NEXT @TamanoPagina ROWS ONLY;";

            var offset = Math.Max(0, (pagina - 1) * tamanoPagina);
            using var connection = new SqlConnection(_connectionString);
            var registros = await connection.QueryAsync<ProyectoModernizacionListado>(
                sql, BuildFilterParameters(busqueda, etapa, tipoFinanciamiento, universo, offset, tamanoPagina));
            return registros.ToList();
        }

        public async Task<InformePormenorizadoAgregados> ObtenerAgregadosAsync(string busqueda, string etapa, string tipoFinanciamiento, string universo)
        {
            var sql = $@"
SELECT COUNT(*) AS TotalRegistros,
       ISNULL(SUM(MontoProyectoMdp), 0) AS MontoTotalMdp,
       ISNULL(ROUND(AVG(PorcentajeAvanceEjecucion), 1), 0) AS AvancePromedio,
       SUM(CASE WHEN EtapaProyecto LIKE '%Operaci%' THEN 1 ELSE 0 END) AS ProyectosOperacion,
       SUM(CASE WHEN UniversoPresentacionPresidencia = @UniversoSi THEN 1 ELSE 0 END) AS ProyectosPriorizados
FROM {TableName}
WHERE {FiltroSql};

SELECT ISNULL(NULLIF(LTRIM(RTRIM(EtapaProyecto)), ''), 'Sin etapa') AS Etiqueta,
       COUNT(*) AS Total,
       ISNULL(SUM(MontoProyectoMdp), 0) AS Monto
FROM {TableName}
WHERE {FiltroSql}
GROUP BY ISNULL(NULLIF(LTRIM(RTRIM(EtapaProyecto)), ''), 'Sin etapa')
ORDER BY COUNT(*) DESC
OFFSET 0 ROWS FETCH NEXT 6 ROWS ONLY;

SELECT ISNULL(NULLIF(LTRIM(RTRIM(TipoFinanciamiento)), ''), 'Sin tipo') AS Etiqueta,
       COUNT(*) AS Total,
       ISNULL(SUM(MontoProyectoMdp), 0) AS Monto
FROM {TableName}
WHERE {FiltroSql}
GROUP BY ISNULL(NULLIF(LTRIM(RTRIM(TipoFinanciamiento)), ''), 'Sin tipo')
ORDER BY SUM(MontoProyectoMdp) DESC
OFFSET 0 ROWS FETCH NEXT 4 ROWS ONLY;";

            using var connection = new SqlConnection(_connectionString);
            using var multi = await connection.QueryMultipleAsync(
                sql, BuildFilterParameters(busqueda, etapa, tipoFinanciamiento, universo));

            var agregados = await multi.ReadFirstAsync<InformePormenorizadoAgregados>();
            agregados.ResumenEtapas = (await multi.ReadAsync<InformePormenorizadoResumenItem>()).ToList();
            agregados.ResumenFinanciamiento = (await multi.ReadAsync<InformePormenorizadoResumenItem>()).ToList();
            return agregados;
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