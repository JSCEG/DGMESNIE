using Dapper;
using Microsoft.Data.SqlClient;
using NSIE.Models;
using System.Data;

namespace NSIE.Servicios
{
    /// <summary>
    /// Acceso a dgmesnie.PODECOBI_* — polos, vocaciones, contactos, geometría, fuentes.
    /// Devuelve listas con cabecera + grids relacionados (vocaciones, contactos, fuentes,
    /// geometrías) usando QueryMultiple para evitar round-trips innecesarios.
    /// </summary>
    public interface IRepositorioPODECOBIPolos
    {
        Task<List<PODECOBIPolo>> ListarAsync(string busqueda = null, bool soloActivos = true);
        Task<PODECOBIPoloDetalle> ObtenerPorNumeroAsync(string numero);
        Task<PODECOBIPoloDetalle> ObtenerPorCoordenadaAsync(double lon, double lat);
        Task<PODECOBIPoloResumen> ObtenerResumenAsync();
        Task<List<PODECOBIEstadoConteo>> ListarEstadosAsync();
        Task<List<PODECOBIPoloGeoRow>> ListarComoGeoJsonAsync();
    }

    public class PODECOBIEstadoConteo
    {
        public string Estado { get; set; }
        public int PoloCount { get; set; }
    }

    /// <summary>Fila cruda para armar FeatureCollection en el controller.</summary>
    public class PODECOBIPoloGeoRow
    {
        public string Numero { get; set; }
        public string NombreOficial { get; set; }
        public string Estado { get; set; }
        public string Municipio { get; set; }
        public decimal? AreaOficialHa { get; set; }
        public decimal? AreaGeojsonHa { get; set; }
        public string Etapa { get; set; }
        public decimal? Lon { get; set; }
        public decimal? Lat { get; set; }
        public string GeometriasJson { get; set; }    // JSON array (uno o más features)
    }

    public class RepositorioPODECOBIPolos : IRepositorioPODECOBIPolos
    {
        private readonly string _connectionString;

        public RepositorioPODECOBIPolos(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<PODECOBIPolo>> ListarAsync(string busqueda = null, bool soloActivos = true)
        {
            using var cn = new SqlConnection(_connectionString);
            var rows = await cn.QueryAsync<PODECOBIPolo>(
                "dgmesnie.sp_PODECOBI_Polo_Listar",
                new { Busqueda = Normalizar(busqueda), SoloActivos = soloActivos ? 1 : 0 },
                commandType: CommandType.StoredProcedure);
            return rows.ToList();
        }

        public async Task<PODECOBIPoloDetalle> ObtenerPorNumeroAsync(string numero)
        {
            using var cn = new SqlConnection(_connectionString);
            using var multi = await cn.QueryMultipleAsync(
                "dgmesnie.sp_PODECOBI_Polo_ObtenerPorNumero",
                new { Numero = (numero ?? string.Empty).Trim() },
                commandType: CommandType.StoredProcedure);

            var polo = await multi.ReadFirstOrDefaultAsync<PODECOBIPolo>();
            if (polo == null) return null;

            return new PODECOBIPoloDetalle
            {
                Polo = polo,
                Vocaciones = (await multi.ReadAsync<PODECOBIVocacion>()).ToList(),
                Contactos  = (await multi.ReadAsync<PODECOBIContacto>()).ToList(),
                Fuentes    = (await multi.ReadAsync<PODECOBIFuente>()).ToList(),
                Geometrias = (await multi.ReadAsync<PODECOBIGeometria>()).ToList()
            };
        }

        public async Task<PODECOBIPoloDetalle> ObtenerPorCoordenadaAsync(double lon, double lat)
        {
            using var cn = new SqlConnection(_connectionString);
            using var multi = await cn.QueryMultipleAsync(
                "dgmesnie.sp_PODECOBI_Polo_ObtenerPorCoordenada",
                new { Lon = lon, Lat = lat },
                commandType: CommandType.StoredProcedure);

            var polo = await multi.ReadFirstOrDefaultAsync<PODECOBIPolo>();
            if (polo == null) return null;

            return new PODECOBIPoloDetalle
            {
                Polo = polo,
                Vocaciones = (await multi.ReadAsync<PODECOBIVocacion>()).ToList(),
                Contactos  = (await multi.ReadAsync<PODECOBIContacto>()).ToList(),
                Fuentes    = (await multi.ReadAsync<PODECOBIFuente>()).ToList(),
                Geometrias = (await multi.ReadAsync<PODECOBIGeometria>()).ToList()
            };
        }

        public async Task<PODECOBIPoloResumen> ObtenerResumenAsync()
        {
            using var cn = new SqlConnection(_connectionString);
            return await cn.QueryFirstOrDefaultAsync<PODECOBIPoloResumen>(
                "dgmesnie.sp_PODECOBI_Polo_ObtenerResumen",
                commandType: CommandType.StoredProcedure);
        }

        public async Task<List<PODECOBIEstadoConteo>> ListarEstadosAsync()
        {
            using var cn = new SqlConnection(_connectionString);
            var rows = await cn.QueryAsync<PODECOBIEstadoConteo>(
                "dgmesnie.sp_PODECOBI_Polo_ListarEstados",
                commandType: CommandType.StoredProcedure);
            return rows.ToList();
        }

        public async Task<List<PODECOBIPoloGeoRow>> ListarComoGeoJsonAsync()
        {
            using var cn = new SqlConnection(_connectionString);
            var rows = await cn.QueryAsync<PODECOBIPoloGeoRow>(
                "dgmesnie.sp_PODECOBI_Polo_ListarGeoJSON",
                commandType: CommandType.StoredProcedure);
            return rows.ToList();
        }

        private static string Normalizar(string valor)
            => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }
}
