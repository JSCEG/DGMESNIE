using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using NSIE.Models;
using NSIE.Servicios;
using System.Security.Cryptography;
using System.Text;
using System.Data;
using System.Globalization;



namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    public class PermisosPVController : Controller
    {
        private readonly IRepositorioPermisosPV repositorioPermisosPV;


        public PermisosPVController(IRepositorioPermisosPV repositorioPermisosPV)
        {

            this.repositorioPermisosPV = repositorioPermisosPV;
        }

        public IActionResult MenuPV()
        {
            return View();
        }



        public async Task<IActionResult> Permisos_PV_GLP()
        {
            var permisosPVGLP = await repositorioPermisosPV.ObtenerPermisosPV_GLP();
            var viewModel = new TotalPermisosVehiculares
            {
                Permisos = permisosPVGLP,
                TotalAutotanques = permisosPVGLP.Count(p => p.TIPO_DE_VEHICULO == "Autotanque"),
                TotalBuquetanques = permisosPVGLP.Count(p => p.TIPO_DE_VEHICULO == "Buquetanque"),
                TotalSemirremolques = permisosPVGLP.Count(p => p.TIPO_DE_VEHICULO == "Semirremolque"),
                TotalGeneralVehiculos = permisosPVGLP.Count(),
                TotalCapacidadAutotanques = permisosPVGLP.Where(p => p.TIPO_DE_VEHICULO == "Autotanque").Sum(p => (long)p.CAPACIDAD_DEL_RECIPIENTE_LITROS),
                TotalCapacidadBuquetanques = permisosPVGLP.Where(p => p.TIPO_DE_VEHICULO == "Buquetanque").Sum(p => (long)p.CAPACIDAD_DEL_RECIPIENTE_LITROS),
                TotalCapacidadSemirremolques = permisosPVGLP.Where(p => p.TIPO_DE_VEHICULO == "Semirremolque").Sum(p => (long)p.CAPACIDAD_DEL_RECIPIENTE_LITROS),
                TotalGeneralCapacidad = permisosPVGLP.Sum(p => (long)p.CAPACIDAD_DEL_RECIPIENTE_LITROS)
            };

            return View(viewModel);
        }



        public async Task<IActionResult> Permisos_PV_GLP_DIST()
        {
            var permisosPVGLP_DIST = await repositorioPermisosPV.ObtenerPermisosPV_GLP_DIST();
            var viewModel = new TotalPermisosVehiculares
            {
                Permisos = permisosPVGLP_DIST,
                TotalAutotanques = permisosPVGLP_DIST.Count(p => p.TIPO_DE_VEHICULO == "Autotanque"),
                TotalVReparto = permisosPVGLP_DIST.Count(p => p.TIPO_DE_VEHICULO == "Vehículo de reparto"),
                // TotalSemirremolques = permisosPVGLP_DIST.Count(p => p.TIPO_DE_VEHICULO == "Semirremolque"),
                TotalGeneralVehiculos = permisosPVGLP_DIST.Count(),
                TotalCapacidadAutotanques = permisosPVGLP_DIST.Where(p => p.TIPO_DE_VEHICULO == "Autotanque").Sum(p => (long)p.CAPACIDAD_DEL_RECIPIENTE_LITROS),
                TotalCapacidadVReparto = permisosPVGLP_DIST.Where(p => p.TIPO_DE_VEHICULO == "Vehículo de reparto").Sum(p => (long)p.CAPACIDAD_DEL_RECIPIENTE_LITROS),
                // TotalCapacidadSemirremolques = permisosPVGLP_DIST.Where(p => p.TIPO_DE_VEHICULO == "Semirremolque").Sum(p => (long)p.CAPACIDAD_DEL_RECIPIENTE_LITROS),
                TotalGeneralCapacidad = permisosPVGLP_DIST.Sum(p => (long)p.CAPACIDAD_DEL_RECIPIENTE_LITROS)
            };

            return View(viewModel);
        }

        // Busqueda por placa o permiso
        public async Task<IActionResult> BuscarPV()
        {
            // Obtener los totales de permisos
            var (totalPermisosGLP, totalPermisosGLPDist) = await repositorioPermisosPV.ObtenerTotalesPermisos();

            // Inicializar el modelo vacío para la búsqueda
            var model = new BusquedaPermisoViewModel
            {
                Permisos = new List<PermisoVehicular>()
            };

            // Pasar los totales a la vista a través de ViewBag
            ViewBag.TotalPermisosGLP = totalPermisosGLP;
            ViewBag.TotalPermisosGLPDist = totalPermisosGLPDist;

            return View(model);
        }

        public async Task<IActionResult> BuscarPermisos(BusquedaPermisoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Inicializar la colección si el modelo no es válido
                model.Permisos = new List<PermisoVehicular>();

                // Obtener los totales de permisos
                var (totalesGLPInicial, totalesGLPDistInicial) = await repositorioPermisosPV.ObtenerTotalesPermisos();
                ViewBag.TotalPermisosGLP = totalesGLPInicial;
                ViewBag.TotalPermisosGLPDist = totalesGLPDistInicial;

                return View("BuscarPV", model);
            }

            model.Busqueda = model.Busqueda?.Trim();

            var (permisos, ultimaFecha) = await repositorioPermisosPV.BuscarPermisosPV(model.Busqueda);

            if (!permisos.Any())
            {
                ViewBag.Mensaje = "No se encontró ningún permiso o número de placa con la búsqueda proporcionada.";
            }

            model.Permisos = permisos;

            // Obtener los totales de permisos para que no se pierdan
            var (totalesGLPBusqueda, totalesGLPDistBusqueda) = await repositorioPermisosPV.ObtenerTotalesPermisos();
            ViewBag.TotalPermisosGLP = totalesGLPBusqueda;
            ViewBag.TotalPermisosGLPDist = totalesGLPDistBusqueda;

            // Formatear la fecha correctamente
            ViewBag.UltimaFechaActualizacion = ultimaFecha.ToString("dd 'de' MMMM 'de' yyyy", new CultureInfo("es-ES"));

            return View("BuscarPV", model);
        }


    }
}
