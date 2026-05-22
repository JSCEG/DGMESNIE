using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using NSIE.Models;

namespace NSIE.Servicios
{
    public interface IRepositorioReuniones
    {
        Task<List<ReunionesSeguimientoItem>> ObtenerDashboardAsync();
        Task<int> CrearAsuntoAsync(ReunionesCrearAsuntoInput input, int? idUsuario, string nombreUsuario);
        Task ActualizarEstatusAsync(ReunionesActualizarEstatusInput input, int? idUsuario, string nombreUsuario);
        Task AgregarComentarioAsync(ReunionesAgregarComentarioInput input, int? idUsuario, string nombreUsuario);
        Task MarcarEnvioMonicaAsync(ReunionesMarcarEnvioMonicaInput input, int? idUsuario, string nombreUsuario);
        Task DesactivarAsuntoAsync(int asuntoId, int? idUsuario, string nombreUsuario);
    }

    public class RepositorioReuniones : IRepositorioReuniones
    {
        private const string SharePointFolderBaseUrl = "https://senermx.sharepoint.com/sites/DGMESNIE/Documentos%20compartidos/6.%20Consultas%20informes%20estudios/6.15%20Asuntos%20M%C3%B3nica";
        private readonly string _connectionString;

        public RepositorioReuniones(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        public async Task<List<ReunionesSeguimientoItem>> ObtenerDashboardAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            var asuntos = (await connection.QueryAsync<ReunionesSeguimientoItem>(
                "dgmesnie.sp_AsuntoSeguimiento_ListarDashboard",
                commandType: CommandType.StoredProcedure)).ToList();

            foreach (var asunto in asuntos)
            {
                asunto.CarpetaSharePointUrl = ConstruirCarpetaUrl(
                    asunto.CarpetaSharePointUrl,
                    asunto.UbicacionCarpeta,
                    asunto.Expediente);
            }

            return asuntos;
        }

        public async Task<int> CrearAsuntoAsync(ReunionesCrearAsuntoInput input, int? idUsuario, string nombreUsuario)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.ExecuteScalarAsync<int>(
                "dgmesnie.sp_AsuntoSeguimiento_Crear",
                new
                {
                    input.TituloAsunto,
                    input.Descripcion,
                    input.Expediente,
                    input.FechaSolicitud,
                    input.DiasTranscurridos,
                    input.Responsable,
                    input.Encargado,
                    input.Minuta,
                    input.FichaInformativaOficio,
                    input.AreaResponsable,
                    Estatus = string.IsNullOrWhiteSpace(input.Estatus) ? "Pendiente" : input.Estatus,
                    input.EstadoActual,
                    input.TipoAsunto,
                    input.SemaforoManual,
                    input.FechaReunion,
                    input.FechaCompromiso,
                    input.FechaAtencion,
                    CarpetaSharePointUrl = input.UbicacionCarpeta,
                    input.UbicacionCarpeta,
                    input.DatosContacto,
                    input.FechaTextoReunion,
                    input.RequiereEnvioMonica,
                    IdUsuarioCreacion = idUsuario,
                    NombreUsuarioCreacion = nombreUsuario
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task ActualizarEstatusAsync(ReunionesActualizarEstatusInput input, int? idUsuario, string nombreUsuario)
        {
            using var connection = new SqlConnection(_connectionString);

            await connection.ExecuteAsync(
                "dgmesnie.sp_AsuntoSeguimiento_ActualizarEstatus",
                new
                {
                    input.AsuntoId,
                    input.Estatus,
                    input.FechaCompromiso,
                    input.FechaAtencion,
                    IdUsuario = idUsuario,
                    NombreUsuario = nombreUsuario
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task AgregarComentarioAsync(ReunionesAgregarComentarioInput input, int? idUsuario, string nombreUsuario)
        {
            using var connection = new SqlConnection(_connectionString);

            await connection.ExecuteAsync(
                "dgmesnie.sp_AsuntoComentario_Agregar",
                new
                {
                    input.AsuntoId,
                    IdUsuario = idUsuario,
                    NombreUsuario = nombreUsuario,
                    input.Mensaje,
                    input.TipoMensaje
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task MarcarEnvioMonicaAsync(ReunionesMarcarEnvioMonicaInput input, int? idUsuario, string nombreUsuario)
        {
            using var connection = new SqlConnection(_connectionString);

            await connection.ExecuteAsync(
                "dgmesnie.sp_AsuntoSeguimiento_MarcarEnvioMonica",
                new
                {
                    input.AsuntoId,
                    IdUsuario = idUsuario,
                    NombreUsuario = nombreUsuario,
                    input.ObservacionesEnvioMonica
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task DesactivarAsuntoAsync(int asuntoId, int? idUsuario, string nombreUsuario)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            await connection.ExecuteAsync(
                @"UPDATE [dgmesnie].[AsuntoSeguimiento]
                  SET [Activo] = 0,
                      [IdUsuarioUltimaActualizacion] = @IdUsuario,
                      [NombreUsuarioUltimaActualizacion] = @NombreUsuario,
                      [FechaActualizacion] = SYSUTCDATETIME()
                  WHERE [AsuntoId] = @AsuntoId AND [Activo] = 1;",
                new { AsuntoId = asuntoId, IdUsuario = idUsuario, NombreUsuario = nombreUsuario },
                transaction);

            await connection.ExecuteAsync(
                @"INSERT INTO [dgmesnie].[AsuntoMovimiento]
                  ([AsuntoId], [TipoMovimiento], [Detalle], [ValorAnterior], [ValorNuevo], [IdUsuario], [NombreUsuario])
                  VALUES (@AsuntoId, N'Baja lógica', N'Se desactivó el asunto desde el dashboard de reuniones.', N'Activo', N'Inactivo', @IdUsuario, @NombreUsuario);",
                new { AsuntoId = asuntoId, IdUsuario = idUsuario, NombreUsuario = nombreUsuario },
                transaction);

            transaction.Commit();
        }

        private static string ConstruirCarpetaUrl(params string[] valoresCarpeta)
        {
            var carpetaValor = valoresCarpeta.FirstOrDefault(valor => !string.IsNullOrWhiteSpace(valor));

            if (string.IsNullOrWhiteSpace(carpetaValor))
            {
                return null;
            }

            var valorNormalizado = carpetaValor.Trim();

            if (valorNormalizado.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                valorNormalizado.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return valorNormalizado;
            }

            return $"{SharePointFolderBaseUrl}/{Uri.EscapeDataString(valorNormalizado)}";
        }
    }
}