using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using NSIE.Controllers;
using NSIE.Models;
using System.Text.Json;
using System.Collections.Generic;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Data;
using System.ComponentModel;

namespace NSIE.Servicios
{
    public interface IRepositorioPermisosPV
    {
        Task<IEnumerable<PermisoVehicular>> ObtenerPermisosPV_GLP();
        Task<IEnumerable<PermisoVehicular>> ObtenerPermisosPV_GLP_DIST();
        // Task<IEnumerable<PermisoVehicular>> BuscarPermisosPV(string busqueda);
        Task<(IEnumerable<PermisoVehicular>, DateTime)> BuscarPermisosPV(string busqueda);

        Task<(int totalPermisosGLP, int totalPermisosGLPDist)> ObtenerTotalesPermisos();
    }

    //El Select SCOPE IDENTITY devuelve el ID impactado en la BD
    public class RepositorioPermisosPV : IRepositorioPermisosPV
    {

        //Cadena de Conexión a BD
        private readonly string connectionString; //Variable en la que se coloca el valor del CS por default desde el proveedor de configuraciones
        public RepositorioPermisosPV(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }


        public async Task<IEnumerable<PermisoVehicular>> ObtenerPermisosPV_GLP()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    var query = "SELECT * FROM [dbo].[ParqueVehicularPermisosGLP]";
                    var permisospv = await connection.QueryAsync<PermisoVehicular>(query);
                    return permisospv;
                }
            }
            catch (SqlException ex)
            {
                // Log this exception or handle it as needed
                throw new ApplicationException("Hubo un Error al obtener permisos de parque Vehicular de Gas L.P., por favor intente más tarde", ex);
            }
        }


        public async Task<IEnumerable<PermisoVehicular>> ObtenerPermisosPV_GLP_DIST()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    var query = "SELECT * FROM [dbo].[ParqueVehicularPermisosGLPDist]";
                    var permisospv = await connection.QueryAsync<PermisoVehicular>(query);
                    return permisospv;
                }
            }
            catch (SqlException ex)
            {
                // Log this exception or handle it as needed
                throw new ApplicationException("Hubo un Error al obtener permisos de parque Vehicular de Gas L.P., por favor intente más tarde", ex);
            }
        }

        public async Task<(IEnumerable<PermisoVehicular>, DateTime)> BuscarPermisosPV(string busqueda)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var query1 = "SELECT PERMISO, TIPO_DE_VEHICULO, ID_CRE, NUMERO_ECONOMICO, MARCA_DEL_RECIPIENTE, CAPACIDAD_DEL_RECIPIENTE_LITROS, NUMERO_DE_SERIE_DEL_RECIPIENTE, NUMERO_DE_PLACA_DEL_RECIPIENTE, FECHA, NULL AS MARCA, NULL AS MODELO, NULL AS PLACAS, [FECHA] FROM [dbo].[ParqueVehicularPermisosGLP] WHERE PERMISO LIKE @Busqueda OR NUMERO_DE_PLACA_DEL_RECIPIENTE LIKE @Busqueda";

                    var query2 = "SELECT PERMISO, TIPO_DE_VEHICULO, ID_CRE, NUMERO_ECONOMICO, MARCA_DEL_RECIPIENTE, CAPACIDAD_DEL_RECIPIENTE_LITROS, NULL AS NUMERO_DE_SERIE_DEL_RECIPIENTE, PLACAS AS NUMERO_DE_PLACA_DEL_RECIPIENTE, FECHA, MARCA, MODELO, PLACAS, [FECHA] FROM [dbo].[ParqueVehicularPermisosGLPDist] WHERE PERMISO LIKE @Busqueda OR PLACAS LIKE @Busqueda";

                    var parametros = new { Busqueda = "%" + busqueda + "%" };

                    var permisosGLP = await connection.QueryAsync<PermisoVehicular>(query1, parametros);
                    var permisosGLPDist = await connection.QueryAsync<PermisoVehicular>(query2, parametros);

                    var permisosCombinados = permisosGLP.Concat(permisosGLPDist).ToList();

                    // Obtener la fecha más reciente de las dos tablas
                    var ultimaFecha = permisosCombinados.Max(p => p.FECHA);

                    return (permisosCombinados, ultimaFecha);
                }
            }
            catch (SqlException ex)
            {
                throw new ApplicationException("Hubo un Error al buscar permisos de parque Vehicular, por favor intente más tarde", ex);
            }
        }

        // Calcula los totales d elos permisos
        public async Task<(int totalPermisosGLP, int totalPermisosGLPDist)> ObtenerTotalesPermisos()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var query1 = "SELECT COUNT(DISTINCT PERMISO) FROM [dbo].[ParqueVehicularPermisosGLP]";
                    var query2 = "SELECT COUNT(DISTINCT PERMISO) FROM [dbo].[ParqueVehicularPermisosGLPDist]";

                    var totalPermisosGLP = await connection.ExecuteScalarAsync<int>(query1);
                    var totalPermisosGLPDist = await connection.ExecuteScalarAsync<int>(query2);

                    return (totalPermisosGLP, totalPermisosGLPDist);
                }
            }
            catch (SqlException ex)
            {
                throw new ApplicationException("Hubo un error al obtener los totales de permisos de parque vehicular.", ex);
            }
        }


    }

}











