
using Microsoft.Data.SqlClient;
using NSIE.Models;
using Dapper;
using System.Data;
using NuGet.Protocol.Plugins;
using Microsoft.AspNetCore.Mvc;

namespace NSIE.Servicios
{
    public interface IRepositorioFinanzas
    {
        /// <summary>
        /// Consulta el tablero financiero aplicando filtros opcionales.
        /// </summary>
        /// <param name="ciclo">Año fiscal (ej. 2025)</param>
        /// <param name="idUR">ID de la Unidad Responsable</param>
        /// <param name="idCapitulo">ID del Capítulo de gasto</param>
        /// <param name="idPP">ID del Programa Presupuestario</param>
        /// <param name="tipoGasto">Tipo de gasto (ej. "PROGRAMABLE")</param>
        /// <param name="idEntidadFederativa">ID de la Entidad Federativa</param>
        /// <returns>Lista de registros presupuestales filtrados</returns>
        Task<List<TableroFinancieroModel>> ObtenerTableroFinancieroAsync(
            int? ciclo = null,
            int? idUR = null,
            int? idCapitulo = null,
            string idPP = null,
            string tipoGasto = null,
            int? idEntidadFederativa = null);
    }



    public class RepositorioFinanzas : IRepositorioFinanzas
    {
        private readonly string connectionString;


        public RepositorioFinanzas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");

        }
        public async Task<List<TableroFinancieroModel>> ObtenerTableroFinancieroAsync(
            int? ciclo, int? idUR, int? idCapitulo, string idPP, string tipoGasto, int? idEntidadFederativa)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            var parametros = new DynamicParameters();
            parametros.Add("@CICLO", ciclo);
            parametros.Add("@ID_UR", idUR);
            parametros.Add("@ID_CAPITULO", idCapitulo);
            parametros.Add("@ID_PP", idPP);
            parametros.Add("@TIPO_DE_GASTO", tipoGasto);
            parametros.Add("@ID_ENTIDAD_FEDERATIVA", idEntidadFederativa);

            var resultado = await connection.QueryAsync<TableroFinancieroModel>(
                "presupuesto.sp_ObtenerTableroFinanciero",
                parametros,
                commandType: CommandType.StoredProcedure);

            return resultado.ToList();
        }


    }

}

