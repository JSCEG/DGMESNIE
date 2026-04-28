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
    public interface IRepositorioSankeySener
    {
        Task<IEnumerable<int>> ObtenerAños(); // Método para obtener los años
        Task<IEnumerable<ConsultaSankey>> devuelveSankey(ConsultaSankey consultaSankey);
        Task<IEnumerable<NodosSankey>> obtenerNodosxAño([FromBody] NodosSankey nodosSankey);
        Task<IEnumerable<NodosCajaSankey>> devuelveNodosCajaSankey(NodosCajaSankey nodosCajaSankey);
        Task<IEnumerable<NodosSectores>> devuelveNodosSectores([FromBody] NodosSectores nodosSectores);
        Task<IEnumerable<NodosTransformaciones>> devuelveNodosTransformaciones([FromBody] NodosTransformaciones nodosTransformaciones);
        Task<IEnumerable<NodosTiposEnergia>> devuelveNodosTiposEnergia([FromBody] NodosTiposEnergia nodosTiposEnergia);
        Task<IEnumerable<NodosUsoFinal>> devuelveNodosUsoFinal([FromBody] NodosUsoFinal nodosUsoFinal);
        Task<IEnumerable<NodosGrafica>> devuelveGrafica(NodosGrafica nodosGrafica);
        Task<IEnumerable<NodosTablaFep>> devuelveTablaFep(NodosTablaFep nodosTablaFep);
        Task<IEnumerable<NodosTablaSector>> devuelveTablaSector(NodosTablaSector nodosTablaSector);
        Task<IEnumerable<NodosTablaTransformacion>> devuelveTablaTransformacion(NodosTablaTransformacion nodosTablaTransformacion);
        Task<IEnumerable<NodosTablaTipos>> devuelveTablaTipos(NodosTablaTipos nodosTablaTipos);
        Task<IEnumerable<NodosTablaUso>> devuelveTablaUso(NodosTablaUso nodosTablaUso);

        //SANKEY ENERGÍA
        Task<IEnumerable<EnergyFlatDto>> ObtenerEnergyDataAsync();
        Task<IEnumerable<SankeyColor>> ObtenerColorDataAsync();
        Task UpsertEnergyDataAsync(List<EnergyParentDto> data);
    }

    //El Select SCOPE IDENTITY devuelve el ID impactado en la BD
    public class RepositorioSankeySener : IRepositorioSankeySener
    {

        //Cadena de Conexión a BD
        private readonly string connectionString; //Variable en la que se coloca el valor del CS por default desde el proveedor de configuraciones
        public RepositorioSankeySener(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }



        // Método nuevo para obtener los años
        // Método actualizado para obtener los años desde la tabla de catálogo
        public async Task<IEnumerable<int>> ObtenerAños()
        {
            using var connection = new SqlConnection(connectionString);

            var query = "SELECT IdAño FROM balance.CatAniosSankey ORDER BY IdAño ASC";

            var años = await connection.QueryAsync<int>(query);

            return años;
        }



        public async Task<IEnumerable<ConsultaSankey>> devuelveSankey([FromBody] ConsultaSankey consultaSankey)
        {
            using var connection = new SqlConnection(connectionString);

            // Implementa aquí la lógica para obtener los parámetros de la vista al stored procedure
            var parameters = new
            {

                yearSelect = consultaSankey.yearSelect

            };

            IEnumerable<ConsultaSankey> resultado;
            resultado = await connection.QueryAsync<ConsultaSankey>(
                    "[dbo].[Sankey]  @yearSelect",
                    parameters);

            return resultado;
        }

        [HttpPost]
        public async Task<IEnumerable<NodosSankey>> obtenerNodosxAño([FromBody] NodosSankey nodosSankey)
        {
            using var connection = new SqlConnection(connectionString);

            // Implementa aquí la lógica para obtener los parámetros de la vista al stored procedure
            var parameters = new
            {

                yearSelect = nodosSankey.yearSelect

            };

            //IEnumerable<NodosSankey> resultado;
            var resultado = await connection.QueryAsync<NodosSankey>(
                    // "Select * From [dbo].[vValores_FEP] where Año=@yearSelect",
                    "SELECT *, TRIM(FEP_Nombre) AS FEP_Nombre_sin_espacios, TRIM(Imagen) AS Imagen_sin_espacios FROM [dbo].[vValores_FEP] where Año=@yearSelect",
                     parameters);

            return resultado;
        }


        public async Task<IEnumerable<NodosCajaSankey>> devuelveNodosCajaSankey(NodosCajaSankey nodosCajaSankey)
        {
            using var connection = new SqlConnection(connectionString);



            IEnumerable<NodosCajaSankey> resultado;
            resultado = await connection.QueryAsync<NodosCajaSankey>(
                     "Select * From [dbo].[NodosCajaSankey] "
                    );

            return resultado;
        }




        [HttpPost]
        public async Task<IEnumerable<NodosSectores>> devuelveNodosSectores([FromBody] NodosSectores nodosSectores)
        {
            using var connection = new SqlConnection(connectionString);

            // Implementa aquí la lógica para obtener los parámetros de la vista al stored procedure
            var parameters = new
            {

                yearSelect = nodosSectores.yearSelect

            };

            //IEnumerable<NodosSankey> resultado;
            var resultado = await connection.QueryAsync<NodosSectores>(
                    // "Select * From [dbo].[vValores_FEP] where Año=@yearSelect",
                    "SELECT *, TRIM(Sector_Nombre) AS Sector_Nombre_SE, TRIM(Tipo) AS Tipo_SE FROM [dbo].[vValores_Sector_Energetico] where Año=@yearSelect",
                     parameters);

            return resultado;
        }

        [HttpPost]
        public async Task<IEnumerable<NodosTransformaciones>> devuelveNodosTransformaciones([FromBody] NodosTransformaciones nodosTransformaciones)
        {
            using var connection = new SqlConnection(connectionString);

            // Implementa aquí la lógica para obtener los parámetros de la vista al stored procedure
            var parameters = new
            {

                yearSelect = nodosTransformaciones.yearSelect

            };

            //IEnumerable<NodosSankey> resultado;
            var resultado = await connection.QueryAsync<NodosTransformaciones>(
                    // "Select * From [dbo].[vValores_FEP] where Año=@yearSelect",
                    //"SELECT *, TRIM(Sector_Nombre) AS Sector_Nombre_SE, TRIM(Tipo) AS Tipo_SE FROM [dbo].[vValores_Sector_Energetico] where Año=@yearSelect", 
                    "SELECT *, TRIM(Transformacion_Nombre) AS Transformacion_Nombre_SE FROM [dbo].[vValores_Energia_Transformacion] where Año=@yearSelect",
                     parameters);

            return resultado;
        }

        [HttpPost]
        public async Task<IEnumerable<NodosTiposEnergia>> devuelveNodosTiposEnergia([FromBody] NodosTiposEnergia nodosTiposEnergia)
        {
            using var connection = new SqlConnection(connectionString);

            // Implementa aquí la lógica para obtener los parámetros de la vista al stored procedure
            var parameters = new
            {

                yearSelect = nodosTiposEnergia.yearSelect

            };

            //IEnumerable<NodosSankey> resultado;
            var resultado = await connection.QueryAsync<NodosTiposEnergia>(
                    // "Select * From [dbo].[vValores_FEP] where Año=@yearSelect",
                    "SELECT * FROM [dbo].[vTipos_Energia_Sankey] where Año=@yearSelect",
                     parameters);

            return resultado;
        }

        [HttpPost]
        public async Task<IEnumerable<NodosUsoFinal>> devuelveNodosUsoFinal([FromBody] NodosUsoFinal nodosUsoFinal)
        {
            using var connection = new SqlConnection(connectionString);

            // Implementa aquí la lógica para obtener los parámetros de la vista al stored procedure
            var parameters = new
            {

                yearSelect = nodosUsoFinal.yearSelect

            };

            //IEnumerable<NodosSankey> resultado;
            var resultado = await connection.QueryAsync<NodosUsoFinal>(
                    // "Select * From [dbo].[vValores_FEP] where Año=@yearSelect",
                    "SELECT * FROM [dbo].[vValores_UsosFinales_sankey] where Año=@yearSelect",
                     parameters);

            return resultado;
        }


        public async Task<IEnumerable<NodosGrafica>> devuelveGrafica(NodosGrafica nodosGrafica)
        {
            using var connection = new SqlConnection(connectionString);


            IEnumerable<NodosGrafica> resultado;
            resultado = await connection.QueryAsync<NodosGrafica>(
                    "SELECT *, TRIM(Consumo_Nombre) AS Consumo_Nombre_SE, TRIM(UsoFinal_Nombre) AS UsoFinal_Nombre_SE FROM [dbo].[vConsumo_Usos_Finales]" // where Año=@yearSelect",
                     );

            return resultado;
        }

        public async Task<IEnumerable<NodosTablaFep>> devuelveTablaFep(NodosTablaFep nodosTablaFep)
        {
            using var connection = new SqlConnection(connectionString);


            IEnumerable<NodosTablaFep> resultado;
            resultado = await connection.QueryAsync<NodosTablaFep>(
                    "SELECT *, TRIM(FEP_Nombre) AS FEP_Nombre_sin_espacios, TRIM(Imagen) AS Imagen_sin_espacios FROM [dbo].[vValores_FEP]"
                     );

            return resultado;
        }

        public async Task<IEnumerable<NodosTablaSector>> devuelveTablaSector(NodosTablaSector nodosTablaSector)
        {
            using var connection = new SqlConnection(connectionString);


            IEnumerable<NodosTablaSector> resultado;
            resultado = await connection.QueryAsync<NodosTablaSector>(
                    "SELECT *, TRIM(Sector_Nombre) AS Sector_Nombre_SE, TRIM(Tipo) AS Tipo_SE FROM [dbo].[vValores_Sector_Energetico]"
                     );

            return resultado;
        }

        public async Task<IEnumerable<NodosTablaTransformacion>> devuelveTablaTransformacion(NodosTablaTransformacion nodosTablaTransformacion)
        {
            using var connection = new SqlConnection(connectionString);


            IEnumerable<NodosTablaTransformacion> resultado;
            resultado = await connection.QueryAsync<NodosTablaTransformacion>(
                    "SELECT *, TRIM(Transformacion_Nombre) AS Transformacion_Nombre_SE FROM [dbo].[vValores_Energia_Transformacion]"
                     );

            return resultado;
        }

        public async Task<IEnumerable<NodosTablaTipos>> devuelveTablaTipos(NodosTablaTipos nodosTablaTipos)
        {
            using var connection = new SqlConnection(connectionString);


            IEnumerable<NodosTablaTipos> resultado;
            resultado = await connection.QueryAsync<NodosTablaTipos>(
                    "SELECT * FROM [dbo].[vTipos_Energia_Sankey]"
                     );

            return resultado;
        }

        public async Task<IEnumerable<NodosTablaUso>> devuelveTablaUso(NodosTablaUso nodosTablaUso)
        {
            using var connection = new SqlConnection(connectionString);


            IEnumerable<NodosTablaUso> resultado;
            resultado = await connection.QueryAsync<NodosTablaUso>(
                   "SELECT * FROM [dbo].[vValores_UsosFinales_sankey]"
                     );

            return resultado;
        }

        //SANKEY ENERGÍA
        public async Task<IEnumerable<EnergyFlatDto>> ObtenerEnergyDataAsync()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                return await connection.QueryAsync<EnergyFlatDto>(
                    "energy.sp_GetEnergyData",
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        public async Task<IEnumerable<SankeyColor>> ObtenerColorDataAsync()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = @"
                SELECT Nombre, ColorHex FROM SankeyEnergeticColors";

                return await connection.QueryAsync<SankeyColor>(query);
            }
        }

        public async Task UpsertEnergyDataAsync(List<EnergyParentDto> data)
        {
            var table = new DataTable();

            table.Columns.Add("ParentExternalId", typeof(int));
            table.Columns.Add("ParentName", typeof(string));
            table.Columns.Add("ParentDescription", typeof(string));
            table.Columns.Add("ParentColor", typeof(string));

            table.Columns.Add("ChildExternalId", typeof(int));
            table.Columns.Add("ChildName", typeof(string));
            table.Columns.Add("Tipo", typeof(string));
            table.Columns.Add("ChildDescription", typeof(string));
            table.Columns.Add("ChildColor", typeof(string));

            table.Columns.Add("Year", typeof(int));
            table.Columns.Add("Value", typeof(double));

            foreach (var parent in data)
            {
                foreach (var child in parent.NodosHijo)
                {
                    foreach (var kvp in child.Valores)
                    {
                        if (int.TryParse(kvp.Key, out int year))
                        {
                            double valor = 0;

                            if (kvp.Value is JsonElement jsonElement)
                            {
                                if (jsonElement.ValueKind == JsonValueKind.Number)
                                {
                                    valor = jsonElement.GetDouble();
                                }
                            }
                            else
                            {
                                valor = Convert.ToDouble(kvp.Value);
                            }

                            Console.WriteLine($"Año: {year}, Valor raw: {kvp.Value}, Tipo: {kvp.Value?.GetType()}");

                            table.Rows.Add(
                                parent.id_padre,
                                Clean(parent.NodoPadre),
                                Clean(parent.descripcion),
                                parent.color,

                                child.id_hijo,
                                Clean(child.NodoHijo),
                                Clean(child.tipo),
                                Clean(child.descripcion),
                                child.color,

                                year,
                                valor
                            );
                        }
                    }
                }
            }

            Console.WriteLine($"Filas a insertar: {table.Rows.Count}");

            var duplicados = table.AsEnumerable()
                .GroupBy(r => new {
                    ChildId = r["ChildExternalId"],
                    Year = r["Year"]
                })
                .Where(g => g.Count() > 1)
                .ToList();

            foreach (var dup in duplicados)
            {
                Console.WriteLine($"DUPLICADO EN C#: ChildExternalId={dup.Key.ChildId}, Year={dup.Key.Year}, Veces={dup.Count()}");
            }

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new SqlCommand("energy.UpsertEnergyData", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    var param = cmd.Parameters.AddWithValue("@Data", table);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "energy.EnergyDataType";

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        private string Clean(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            return text
                .Replace("Ã³", "ó")
                .Replace("Ã¡", "á")
                .Replace("Ã©", "é")
                .Replace("Ã­", "í")
                .Replace("Ãº", "ú")
                .Replace("Ã±", "ñ");
        }

    }

}











