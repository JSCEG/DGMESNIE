using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using NSIE.Models;
using NSIE.Servicios;
using System.Security.Cryptography;
using System.Text;
using System.Data;
using Dapper;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;
using SendGrid.Helpers.Mail;



namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    public class LaboratoriosyUEController : Controller
    {
        private readonly IRepositorioLaboratoriosyUE repositorioLaboratoriosyUE;


        public LaboratoriosyUEController(IRepositorioLaboratoriosyUE repositorioLaboratoriosyUE)
        {

            this.repositorioLaboratoriosyUE = repositorioLaboratoriosyUE;
        }



        public IActionResult Ver_LaboratoriosyUE()
        {
            return View();
        }


        //A)Obtiene los Campos Visibles de la lista de Laboratorios
        [HttpGet]
        public async Task<IActionResult> GetCamposVisiblesLUV()
        {
            var perfilUsuarioJson = HttpContext.Session.GetString("PerfilUsuario");
            var perfilUsuario = JsonConvert.DeserializeObject<PerfilUsuario>(perfilUsuarioJson);

            //pasando de String a Int
            int rolId = int.Parse(perfilUsuario.Rol);
            int mercadoId = int.Parse(perfilUsuario.Mercado_ID);


            var camposVisibles = await repositorioLaboratoriosyUE.ObtenerCamposVisiblesLUV(rolId, mercadoId);

            return Json(camposVisibles);
        }


        public async Task<IActionResult> GetLUV()
        {
            var ExpendiosJson2 = await repositorioLaboratoriosyUE.ObtenerLUV();
            if (ExpendiosJson2 == null)
            {
                return NotFound(); // Manejar el caso en queregrese null
            }
            return Json(ExpendiosJson2);
        }
        public IActionResult Rutas()
        {
            return View();
        }

        public IActionResult ConsultaxCoordenada()
        {
            return View();
        }




    }


}

