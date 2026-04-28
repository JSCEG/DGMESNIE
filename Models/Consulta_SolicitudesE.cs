using Microsoft.AspNetCore.Mvc;
using NSIE.Validaciones;
using System.ComponentModel.DataAnnotations;

namespace NSIE.Models
{
    public class Consulta_SolicitudesE
    {



        //Datos Generales de la Solicitud
        public string ID { get; set; }
        public string EF_ID { get; set; }
        public string EF_Nombre { get; set; }
        public string MPO_ID { get; set; }
        public string Municipio_Nombre { get; set; }
        public string Documentos_completos { get; set; }
        public string Analisis_riesgo { get; set; }
        public float Latitud_GEO { get; set; } //X_GEO en BD
        public float Longitud_GEO { get; set; } //Y_Geo en BD
        public string Turno { get; set; }

        public string Marca_solicitada { get; set; }
        public string Expediente { get; set; }
        public string Razon_social { get; set; }

        //Calculos
        public int Total_de_Solicitudes_a_Evaluar { get; set; }
        public int Con_Documentos_Completos { get; set; }
        public int Con_Analisis_de_Riesgo { get; set; }
        public int Total_de_Permisos_de_Expendio { get; set; }

        // Nueva propiedad para Fecha de Actualización
        public DateTime Fecha_Actualizacion { get; set; }


    }
}
