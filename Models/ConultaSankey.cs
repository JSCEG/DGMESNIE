using Microsoft.AspNetCore.Mvc;
using NSIE.Validaciones;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace NSIE.Models
{
    public class ConsultaSankey
    {
        [Key]
        public int FEP_ID { get; set; }

        public string FEP_Nombre { get; set; }

        public string FEP_Tipo { get; set; }

        public int Año { get; set; }

        public decimal Valor { get; set; }

        public decimal Valor_importaciones { get; set; }

        public decimal Valor_exportaciones { get; set; }

        // De la Vista a la BD
        public string yearSelect { get; set; }
    }


}


