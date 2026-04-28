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
    public interface IRepositorioLaboratoriosyUE
    {
        Task<List<string>> ObtenerCamposVisiblesLUV(int rolId, int mercadoId);
        Task<IEnumerable<LaboratoriosyUE>> ObtenerLUV();
    }

    //El Select SCOPE IDENTITY devuelve el ID impactado en la BD
    public class RepositorioLaboratoriosyUE : IRepositorioLaboratoriosyUE
    {

        //Cadena de Conexión a BD
        private readonly string connectionString; //Variable en la que se coloca el valor del CS por default desde el proveedor de configuraciones
        public RepositorioLaboratoriosyUE(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }


        //Obtiene el atributo del cada coordenada si es visible o no
        public async Task<List<string>> ObtenerCamposVisiblesLUV(int rolId, int mercadoId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var campos = await connection.QueryAsync<string>(
                    @"SELECT Nombre_Campo 
					  FROM [dbo].[CamposMapas] 
					 WHERE Rol_ID = @RolId AND Mercado_ID = @MercadoId AND Visible = 1 
                    AND REPLACE([Tabla], ' ', '') = 'vLaboratorios_UV'",
                    new { RolId = rolId, MercadoId = mercadoId }
                );

                return campos.ToList();
            }
        }

        public async Task<IEnumerable<LaboratoriosyUE>> ObtenerLUV()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var coordenadas = await connection.QueryAsync<LaboratoriosyUE>(
                    @"
                        SELECT [ID]
                            ,[Nombre]
                            ,[Elemento_tipo]
                            ,[Entidad_Federativa]
                            ,[Municipio]
                            ,[Dirección]
                            ,[Tipo]
                            ,[Norma]
                            ,[Resolucion]
                            ,[Nombre_corto]
                            ,[Actividad]
                            ,[Latitud]
                            ,[Longitud]
                            ,[Comentarios]
                            ,[Estatus]
                            ,[Fecha_actualizacion]                             
                          FROM [dbo].[vLaboratorios_UV]

								 ");

                return coordenadas;
            }
        }


    }

}











