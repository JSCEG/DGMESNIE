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
    public interface IRepositorioEnergiasLimpias
    {
        Task<IEnumerable<MarcoRegulatorio>> ObtenerMarcoRegulatorio();
    }

    public class RepositorioEnergiasLimpias : IRepositorioEnergiasLimpias
    {
        private readonly string connectionString;

        public RepositorioEnergiasLimpias(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<MarcoRegulatorio>> ObtenerMarcoRegulatorio()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                string sql = @"
                               SELECT * 
                                FROM [dbo].[TimelineEnergiasLimpiasMarcoRegulatorio]
                                ORDER BY [Fecha] ASC;
                ";

                return await connection.QueryAsync<MarcoRegulatorio>(sql);
            }
        }
    }
}







