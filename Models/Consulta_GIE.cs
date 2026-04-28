using Microsoft.AspNetCore.Mvc;
using NSIE.Validaciones;
using System.ComponentModel.DataAnnotations;

namespace NSIE.Models
{
    public class Consulta_GIE
    {

        //Como estan en la vista de la BD -"[dbo].[vRazones_sociales]"
        public string Razon_social { get; set; }
        public int Autorizados { get; set; }
        public int Solicitados { get; set; }
        public int Total { get; set; }
        public int Color_Autorizado { get; set; }
        public int Color_SyA { get; set; }


    }
}
