using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using NSIE.Models;

namespace NSIE.Servicios
{
    public interface IRepositorioReuniones
    {
        Task<List<ReunionesSeguimientoItem>> ObtenerDashboardAsync();
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
                asunto.CarpetaSharePointUrl = ConstruirCarpetaUrl(asunto.CarpetaSharePointUrl);
            }

            return asuntos;
        }

        private static string ConstruirCarpetaUrl(string carpetaValor)
        {
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