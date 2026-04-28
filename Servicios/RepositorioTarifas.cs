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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NSIE.Servicios
{
    public interface IRepositorioTarifas
    {
        Task<List<TarifaMediaInflacion>> ObtenerTarifasAsync();
        Task<IEnumerable<TarifaDetalle>> ObtenerTarifasPorMesAnioYDivisionAsync(string mesAnio, string division);
    }


    public class RepositorioTarifas : IRepositorioTarifas
    {
        private readonly string connectionString;

        public RepositorioTarifas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<TarifaMediaInflacion>> ObtenerTarifasAsync()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = "SELECT * FROM TarifasMediasInflacion";
                var tarifas = await connection.QueryAsync<TarifaMediaInflacion>(query);
                return tarifas.ToList();
            }
        }

        public async Task<IEnumerable<TarifaDetalle>> ObtenerTarifasPorMesAnioYDivisionAsync(string mesAnio, string division)
        {
            var resultados = new List<TarifaDetalle>();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("ObtenerTarifasPorMesAnioYDivision", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@MesAnio", mesAnio);
                    command.Parameters.AddWithValue("@Division", division);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            resultados.Add(new TarifaDetalle
                            {
                                Tarifa = reader["Tarifa"].ToString(),
                                Segmento = reader["Segmento"].ToString(),
                                Unidades = reader["Unidades"].ToString(),
                                Concepto = reader["Concepto"].ToString(),
                                Division = reader["División"].ToString(),
                                Int_Horario = reader["Int_Horario"].ToString(),
                                Valor = reader.IsDBNull(reader.GetOrdinal("Valor")) ? 0 : reader.GetDouble(reader.GetOrdinal("Valor"))
                            });
                        }
                    }
                }
            }

            return resultados;
        }

    }



}







