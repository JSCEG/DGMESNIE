using Dapper;
using Microsoft.Data.SqlClient;
using NSIE.Models.PlaneacionVinculante;

namespace NSIE.Servicios.PlaneacionVinculante
{
    public interface IPlaneacionVinculanteOverviewService
    {
        Task<PlaneacionVinculanteOverview> ObtenerAsync(CancellationToken cancellationToken = default);
    }

    public sealed class PlaneacionVinculanteOverviewService : IPlaneacionVinculanteOverviewService
    {
        private readonly string? _connectionString;
        private readonly ILogger<PlaneacionVinculanteOverviewService> _logger;

        public PlaneacionVinculanteOverviewService(
            IConfiguration configuration,
            ILogger<PlaneacionVinculanteOverviewService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        public async Task<PlaneacionVinculanteOverview> ObtenerAsync(
            CancellationToken cancellationToken = default)
        {
            var overview = new PlaneacionVinculanteOverview();
            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                overview.AdvertenciaDatos = "La conexión institucional no está configurada; se muestran únicamente cifras documentales validadas.";
                return overview;
            }

            const string sql = """
                DECLARE @VersionRedId BIGINT =
                (
                    SELECT TOP (1) VersionId
                    FROM dgmesnie.RedElectricaVersion
                    WHERE Activa = 1 AND Estado = N'publicada'
                    ORDER BY VersionId DESC
                );

                SELECT
                    (SELECT COUNT(*)
                     FROM dgmesnie.vw_PAMProyectoVigente
                     WHERE EstadoVigenciaCartera = N'Vigente') AS PamProyectosVigentes,
                    (SELECT CAST(SUM(ISNULL(MontoProyectoMdp, 0)) AS DECIMAL(18,2))
                     FROM dgmesnie.vw_PAMProyectoVigente
                     WHERE EstadoVigenciaCartera = N'Vigente') AS PamInversionVigenteMdp,
                    (SELECT COUNT(*)
                     FROM dgmesnie.RedElectricaNodo
                     WHERE VersionId = @VersionRedId) AS RedNodos,
                    (SELECT COUNT(*)
                     FROM dgmesnie.RedElectricaArista
                     WHERE VersionId = @VersionRedId) AS RedAristas,
                    (SELECT COUNT(*)
                     FROM dgmesnie.RedElectricaSubestacionInventario
                     WHERE Activa = 1) AS SubestacionesInventario,
                    (SELECT COUNT(*)
                     FROM dgmesnie.CarteraConvocatoriaProyecto
                     WHERE Activo = 1) AS ConvocatoriasActivas,
                    (SELECT CAST(SUM(ISNULL(CapacidadMw, 0)) AS DECIMAL(18,3))
                     FROM dgmesnie.CarteraConvocatoriaProyecto
                     WHERE Activo = 1) AS ConvocatoriasMw,
                    (SELECT COUNT(*)
                     FROM dgmesnie.CarteraConvocatoriaProyecto
                     WHERE Activo = 1 AND ProyectoCoreId IS NOT NULL) AS ConvocatoriasVinculadas;
                """;

            try
            {
                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);
                var operational = await connection.QuerySingleAsync<PlaneacionVinculanteOverview>(
                    new CommandDefinition(sql, cancellationToken: cancellationToken));

                overview.PamProyectosVigentes = operational.PamProyectosVigentes;
                overview.PamInversionVigenteMdp = operational.PamInversionVigenteMdp;
                overview.RedNodos = operational.RedNodos;
                overview.RedAristas = operational.RedAristas;
                overview.SubestacionesInventario = operational.SubestacionesInventario;
                overview.ConvocatoriasActivas = operational.ConvocatoriasActivas;
                overview.ConvocatoriasMw = operational.ConvocatoriasMw;
                overview.ConvocatoriasVinculadas = operational.ConvocatoriasVinculadas;
                overview.DatosOperativosDisponibles = true;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.LogWarning(exception, "No fue posible completar el resumen operativo de Planeación Vinculante.");
                overview.AdvertenciaDatos = "No fue posible consultar los componentes operativos; las cifras PLADESE/PVIRCE siguen disponibles como controles documentales.";
            }

            return overview;
        }
    }
}
