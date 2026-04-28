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
    public interface IRepositorioMap
    {
        Task<IEnumerable<EnergiaRS>> Obtener_TiposRS();
        Task<IEnumerable<EnergiaPYP>> Obtener_TiposPYP();
        Task<IEnumerable<EnergiaSankey>> Obtener_TiposEnergia();
        Task<IEnumerable<EnergiaSankeyUsosFinales>> Obtener_UsosFinales();
        Task<IEnumerable<EmisionesxSector>> Obtener_EmisionesxSector();
        Task<IEnumerable<UsoFinalDetalle>> ObtenerDetalleUsosFinales(int año, string tipo);
    }

    //El Select SCOPE IDENTITY devuelve el ID impactado en la BD
    public class RepositorioMap : IRepositorioMap
    {

        //Cadena de Conexión a BD
        private readonly string connectionString; //Variable en la que se coloca el valor del CS por default desde el proveedor de configuraciones
        public RepositorioMap(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Método nuevo para obtener los Resmuen de Sectores RS por año
        public async Task<IEnumerable<EnergiaRS>> Obtener_TiposRS()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var tiposrs = await connection.QueryAsync<EnergiaRS>(
                    @"
                        SELECT [Año]
                            ,[RefyDes]
                            ,[ComplejosGas]
                            ,[Autoconsumo]
                        FROM [dbo].[vResumenSectores]
					");

                return tiposrs;
            }
        }



        // Método nuevo para obtener los PYP por año
        public async Task<IEnumerable<EnergiaPYP>> Obtener_TiposPYP()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var tipospyp = await connection.QueryAsync<EnergiaPYP>(
                    @"
	                    SELECT 
                        COALESCE(A.[Año], B.[Año], C.[Año]) AS [Año],
                        A.Combustible,
                        B.Calor,
                        C.Electricidad
                    FROM
                        (SELECT [Año], SUM(Valor) as Combustible
                         FROM [cre-db-2].[dbo].[vValores_Energia_Transformacion]
                         WHERE LTRIM(RTRIM([Transformacion_Nombre])) IN ('Refinerías','Coquizadores')
                         GROUP BY [Año]) A
                    FULL OUTER JOIN
                        (SELECT [Año], SUM(Valor) as Calor
                         FROM [cre-db-2].[dbo].[vValores_Energia_Transformacion]
                         WHERE LTRIM(RTRIM([Transformacion_Nombre])) IN ('Complejos gaseros')
                         GROUP BY [Año]) B ON A.[Año] = B.[Año]
                    FULL OUTER JOIN
                        (SELECT [Año], SUM(Valor) as Electricidad
                         FROM [cre-db-2].[dbo].[vValores_Energia_Transformacion]
                         WHERE LTRIM(RTRIM([Transformacion_Nombre])) IN ('Carboeléctrica', 'Ciclo Combinado', 'Combustión Interna', 'Térmica convencional', 'Turbo Gas', 'Nucleoeléctrica', 'Eólica', 'Fotovoltaica', 'Vapor', 'Geotermoeléctrica','Hidroeléctrica')
                         GROUP BY [Año]) C ON COALESCE(A.[Año], B.[Año]) = C.[Año]
                    ORDER BY COALESCE(A.[Año], B.[Año], C.[Año]);
					");

                return tipospyp;
            }
        }


        // Método nuevo para obtener los años
        public async Task<IEnumerable<EnergiaSankey>> Obtener_TiposEnergia()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var tiposenergia = await connection.QueryAsync<EnergiaSankey>(
                    @"
					SELECT  [Año]
                          ,[CargaPico]
                         ,[Intermitente]
                         ,[CargaBase]
                         ,[GasSeco]
                        ,[GasLP]
                         ,[Petrolíferos] as [Petroliferos]
                      FROM [dbo].[vTipos_Energia_Sankey]


					");

                return tiposenergia;
            }
        }


        // Método nuevo para obtener los años
        public async Task<IEnumerable<EnergiaSankeyUsosFinales>> Obtener_UsosFinales()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var tiposenergia = await connection.QueryAsync<EnergiaSankeyUsosFinales>(
                    @"
					SELECT
                       [Año]
                      ,[Hogares]
                      ,[Transporte]
                      ,[SerPubCom]
                      ,[Agricultura]
                      ,[Industrial]
                      ,[SectorEnergia]
                      ,[Hogares_Co2]
                      ,[Transporte_Co2]
                      ,[SerPubCom_Co2]
                      ,[Agricultura_Co2]
                      ,[Industrial_Co2]
                      ,[SectorEnergia_Co2]
                      
                  FROM [dbo].[vValores_UsosFinales_sankey]


					");

                return tiposenergia;
            }
        }


        //
        public async Task<IEnumerable<EmisionesxSector>> Obtener_EmisionesxSector()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var emisionesxSector = await connection.QueryAsync<EmisionesxSector>(
                    @"
                   SELECT 
                        Año, 
                        TRIM(Tipo) AS Tipo, 
                        SUM(Valor_Co2) AS TotalCo2
                    FROM 
                        [dbo].[vValores_Sector_Energetico]
                    GROUP BY 
                        Año, TRIM(Tipo)
                    ORDER BY 
                        Año, TRIM(Tipo);
                    ");

                return emisionesxSector;
            }
        }


        #region  Obtiene el Detalle del Uso Final por Año Seleccionado

        public async Task<IEnumerable<UsoFinalDetalle>> ObtenerDetalleUsosFinales(int año, string tipo)
        {
            using var connection = new SqlConnection(connectionString);

            // Prepara los parámetros para el procedimiento almacenado
            var parametros = new { Año = año, Tipo = tipo };

            // Ejecuta el procedimiento almacenado y recupera los resultados
            var detalles = await connection.QueryAsync<UsoFinalDetalle>(
                "spObtenerDetalleUsosFinalesxAño",
                parametros,
                commandType: CommandType.StoredProcedure);

            return detalles;
        }

        #endregion


    }






}











