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
    public interface IRepositorioAtlas
    {
        Task<IEnumerable<ReporteDIario_PMLS>> EjecutarSP_GetRD_PMLS();
        Task<IEnumerable<ReporteHora_PMLS>> ObtenerPreciosPorFechas(string claveProceso, string claveSistema, string nombreZonaCarga);
        Task<bool> ProbarConexion();

        //Demanda Diare a por GCR
        Task<List<DemandaDiaria>> ObtenerDemandaHistoricaAsync(DateTime inicio, DateTime fin, string claveProcesoMercado);
        Task<List<string>> ObtenerCamposPublicosAzelAsync();
        Task<IEnumerable<AzelPermisoPublico>> ObtenerPermisosPublicosAzelAsync();




    }

    public class RepositorioAtlas : IRepositorioAtlas
    {
        private readonly string connectionString;
        private readonly string defaultConnectionString;

        public RepositorioAtlas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("MIMConnection");
            defaultConnectionString = configuration.GetConnectionString("DefaultConnection");
        }


        public async Task<IEnumerable<ReporteDIario_PMLS>> EjecutarSP_GetRD_PMLS()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = @"
                                DECLARE @FechaSolicitada DATE = GETDATE();

                                WHILE NOT EXISTS (SELECT * 
                                                FROM [Reporte].[fn_PMLs_Map]('MDA', @FechaSolicitada, @FechaSolicitada))
                                BEGIN
                                    SET @FechaSolicitada = DATEADD(DAY, -1, @FechaSolicitada);
                                END

                                SELECT 
                                    ClaveProcesoMercado,
                                    ClaveSistema,
                                    NombreZonaCarga,
                                    Fecha,
                                    PrecioZonal_AVG,
                                    CompEnergia_AVG,
                                    CompPerdida_AVG,
                                    CompCongestion_AVG,
                                    LimInf,
                                    Step,
                                    LimSup,
                                    Latitud,
                                    Longitud
                                FROM 
                                    [Reporte].[fn_PMLs_Map]('MDA', @FechaSolicitada, @FechaSolicitada);
                            ";

                var resultado = (await connection.QueryAsync<ReporteDIario_PMLS>(query)).ToList();

                return resultado;
            }
        }

        public async Task<IEnumerable<ReporteHora_PMLS>> ObtenerPreciosPorFechas(string claveProceso, string claveSistema, string nombreZonaCarga)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = @"
                    DECLARE @FechaSolicitada DATE = GETDATE();
                    DECLARE @FechaInicio DATE = DATEADD(DAY, -8, GETDATE());
                    SET @FechaSolicitada = DATEADD(DAY, -2, @FechaSolicitada);
                    SELECT *
                    FROM [Reporte].[fn_TblPML_Zonal](@claveProceso, @claveSistema, @FechaInicio, @FechaSolicitada)
                    WHERE NombreZonaCarga = @nombreZonaCarga
                    ORDER BY Fecha ASC;
                ";

                var parameters = new { claveProceso, claveSistema, nombreZonaCarga };
                return await connection.QueryAsync<ReporteHora_PMLS>(query, parameters);
            }
        }




        public async Task<bool> ProbarConexion()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }


        //Demanda Diara  por GCR MIM
        public async Task<List<DemandaDiaria>> ObtenerDemandaHistoricaAsync(DateTime inicio, DateTime fin, string claveProcesoMercado)
        {
            var query = @"
        WITH A AS (
            SELECT [ClaveProcesoMercado], [Fecha], [ClaveArea], SUM([DemandaTotal]) AS Demanda
            FROM [DW].[Hechos].[Demanda]
            WHERE Fecha BETWEEN @Inicio AND @Fin
                  AND ClaveProcesoMercado = @ClaveProcesoMercado
            GROUP BY ClaveProcesoMercado, ClaveArea, Fecha
        )
        SELECT * FROM A
        PIVOT (SUM(Demanda) FOR ClaveArea IN (
            [CEN], [ORI], [OCC], [NOR], [NTE], [NES], [BCA], [BCS], [PEN])
        ) PIV
        ORDER BY Fecha";

            using (var connection = new SqlConnection(connectionString))
            {
                return (await connection.QueryAsync<DemandaDiaria>(query, new
                {
                    Inicio = inicio,
                    Fin = fin,
                    ClaveProcesoMercado = claveProcesoMercado
                })).ToList();
            }
        }

        public async Task<List<string>> ObtenerCamposPublicosAzelAsync()
        {
            using var connection = new SqlConnection(defaultConnectionString);
            var campos = await connection.QueryAsync<string>(
                "dgmesnie.sp_AzelPublico_ObtenerCampos",
                commandType: CommandType.StoredProcedure);

            return campos.ToList();
        }

        public async Task<IEnumerable<AzelPermisoPublico>> ObtenerPermisosPublicosAzelAsync()
        {
            using var connection = new SqlConnection(defaultConnectionString);
            return await connection.QueryAsync<AzelPermisoPublico>(
                "dgmesnie.sp_AzelPublico_ObtenerPermisos",
                commandType: CommandType.StoredProcedure);
        }



    }
}










