using Microsoft.AspNetCore.Mvc;
using NSIE.Validaciones;
using System.ComponentModel.DataAnnotations;

namespace NSIE.Models
{
    public class Consulta_GIE
    {
        // Propiedades alineadas con la proyeccion usada por los reportes GIE.
        public string Razon_social { get; set; }
        public int Autorizados { get; set; }
        public int Solicitados { get; set; }
        public int Total { get; set; }
        public int Color_Autorizado { get; set; }
        public int Color_SyA { get; set; }


    }
}
