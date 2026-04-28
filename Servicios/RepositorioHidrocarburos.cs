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

namespace NSIE.Servicios
{
	public interface IRepositorioHidrocarburos
	{
		Task<GraficosHidrocarburos> Obtener_VolImpPetyGN();
	}

	//El Select SCOPE IDENTITY devuelve el ID impactado en la BD
	public class RepositorioHidrocarburos : IRepositorioHidrocarburos
	{
		private readonly string connectionString; //Variable en la que se coloca el valor del CS por default desde el proveedor de configuraciones
		public RepositorioHidrocarburos(IConfiguration configuration)
		{
			connectionString = configuration.GetConnectionString("DefaultConnection");
		}


		public async Task<GraficosHidrocarburos> Obtener_VolImpPetyGN()
		{
			using (var connection = new SqlConnection(connectionString))
			{
				var query = @"
							EXEC [dbo].[ObtieneImpVolumen_PvsGN];
							EXEC [dbo].[ObtieneImpVolumemPorAño_PvsGN];
							EXEC [dbo].[ObtieneImpVolumemPorMes_PvsGN]
							
        ";

				await connection.OpenAsync();

				using (var multi = await connection.QueryMultipleAsync(query))
				{
					var datosGrafico = new GraficosHidrocarburos
					{
						ObtieneImpVolumen_PvsGN = await multi.ReadAsync<GraficosHidrocarburos>().ConfigureAwait(false),
						ObtieneImpVolumemPorAño_PvsGN = await multi.ReadAsync<GraficosHidrocarburos>().ConfigureAwait(false),
						ObtieneImpVolumemPorMes_PvsGN = await multi.ReadAsync<GraficosHidrocarburos>().ConfigureAwait(false),

					};

					return datosGrafico;
				}
			}
		}




	}

}











