using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using NSIE.Models;
using NSIE.Servicios;

namespace NSIE.Controllers
{
	[ServiceFilter(typeof(ValidacionInputFiltro))]
	[AutorizacionFiltro]
	public class HidrocarburosController : Controller
	{
		private readonly IRepositorioHidrocarburos repositorioHidrocarburos;

		public HidrocarburosController(IRepositorioHidrocarburos repositorioHidrocarburos)
		{

			this.repositorioHidrocarburos = repositorioHidrocarburos;
		}


		public IActionResult GLP_Map_Publico()
		{

			return View();
		}

		public IActionResult GN_Map_Publico()
		{

			return View();
		}

		public IActionResult PermisosExpendio_R()
		{

			return View();
		}


		#region Ver por EF

		public IActionResult SLP_RPEP()
		{

			return View();
		}


		#endregion

		#region CENAGAS
		public IActionResult Cenagas()
		{

			return View();
		}

		public IActionResult Exportaciones()
		{
			return View();
		}


		#endregion
		#region Volumenes para Datos de Importación Gas y Petroleo
		public async Task<IActionResult> Vol_Imp_PetYGN()
		{
			var datos = await repositorioHidrocarburos.Obtener_VolImpPetyGN();

			if (datos == null)
			{
				return NotFound(); // Manejar el caso en que no se encuentre el expendio
			}
			return View(datos);
			//return Json(calificacion);
			//return View();
		}

		#endregion
	}
}
