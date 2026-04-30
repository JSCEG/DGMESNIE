using Dapper;
using Microsoft.Data.SqlClient;
using NSIE.Models;
using System.Data;

namespace NSIE.Servicios
{
    public interface IRepositorioPODECOBIS
    {
        Task<List<PODECOBISAgendaItem>> ObtenerAgendaAsync(string busqueda = null);
        Task<PODECOBISAgendaItem> ObtenerPorIdAsync(int agendaId);
        Task<int> CrearAsync(PODECOBISAgendaItem registro);
        Task ActualizarAsync(PODECOBISAgendaItem registro);
        Task EliminarAsync(int agendaId);
    }

    public class RepositorioPODECOBIS : IRepositorioPODECOBIS
    {
        private readonly string _connectionString;

        public RepositorioPODECOBIS(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<PODECOBISAgendaItem>> ObtenerAgendaAsync(string busqueda = null)
        {
            using var connection = new SqlConnection(_connectionString);

            var registros = await connection.QueryAsync<PODECOBISAgendaItem>(
                "dgmesnie.sp_PODECOBISAgenda_Listar",
                new { Busqueda = Normalizar(busqueda) },
                commandType: CommandType.StoredProcedure);

            return registros.ToList();
        }

        public async Task<PODECOBISAgendaItem> ObtenerPorIdAsync(int agendaId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryFirstOrDefaultAsync<PODECOBISAgendaItem>(
                "dgmesnie.sp_PODECOBISAgenda_ObtenerPorId",
                new { AgendaId = agendaId },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> CrearAsync(PODECOBISAgendaItem registro)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.ExecuteScalarAsync<int>(
                "dgmesnie.sp_PODECOBISAgenda_Crear",
                CrearParametros(registro),
                commandType: CommandType.StoredProcedure);
        }

        public async Task ActualizarAsync(PODECOBISAgendaItem registro)
        {
            using var connection = new SqlConnection(_connectionString);

            await connection.ExecuteAsync(
                "dgmesnie.sp_PODECOBISAgenda_Actualizar",
                new
                {
                    registro.AgendaId,
                    registro.Numero,
                    Polo = NormalizarRequerido(registro.Polo),
                    NombreOficialDeclaratoria = NormalizarRequerido(registro.NombreOficialDeclaratoria),
                    OrganismoSeguimiento = Normalizar(registro.OrganismoSeguimiento),
                    NombreContacto = Normalizar(registro.NombreContacto),
                    Cargo = Normalizar(registro.Cargo),
                    Correo = Normalizar(registro.Correo),
                    NumeroTelefonico = Normalizar(registro.NumeroTelefonico),
                    registro.Activo
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task EliminarAsync(int agendaId)
        {
            using var connection = new SqlConnection(_connectionString);

            await connection.ExecuteAsync(
                "dgmesnie.sp_PODECOBISAgenda_Eliminar",
                new { AgendaId = agendaId },
                commandType: CommandType.StoredProcedure);
        }

        private static object CrearParametros(PODECOBISAgendaItem registro)
        {
            return new
            {
                registro.Numero,
                Polo = NormalizarRequerido(registro.Polo),
                NombreOficialDeclaratoria = NormalizarRequerido(registro.NombreOficialDeclaratoria),
                OrganismoSeguimiento = Normalizar(registro.OrganismoSeguimiento),
                NombreContacto = Normalizar(registro.NombreContacto),
                Cargo = Normalizar(registro.Cargo),
                Correo = Normalizar(registro.Correo),
                NumeroTelefonico = Normalizar(registro.NumeroTelefonico)
            };
        }

        private static string Normalizar(string valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }

        private static string NormalizarRequerido(string valor)
        {
            return (valor ?? string.Empty).Trim();
        }
    }
}