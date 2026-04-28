using Microsoft.AspNetCore.Mvc;
using NSIE.Validaciones;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace NSIE.Models
{
    public class CalificacionFinal
    {

        //Campos establecidos para la Calificación Pública

        public string cve_ent { get; set; }
        public string cve_mun { get; set; }
        public string yearSelect { get; set; }
        public string mercadoSelect { get; set; }
        public string resultado { get; set; }
        public string ResultadoFinal { get; set; }
        //******************************//


        // Salida a nivel Municipio POST DESDE EL ajax
        // public string EF_ID { get; set; }
        //public string EF_Nombre { get; set; }
        //public string AprobadosMunicipio { get; set; }
        public string Umbral_Seleccionado_Nal { get; set; }
        public List<string> Indicadores_Seleccionados_Nal { get; set; }
        public string Umbral_Seleccionado_Mun { get; set; }
        public List<string> Indicadores_Seleccionados_Mun { get; set; }






        // Propiedades para los checkboxes
        public List<CheckboxItem> Checkboxes { get; set; }
        public double UmbralSeleccionado { get; set; }
        public double Umbral { get; set; }
        public List<string> IndicadoresSeleccionados { get; set; }
        public List<string> Indicadores_Seleccionados { get; set; }
        public string Umbral_Seleccionado { get; set; }

        // Para el Municipio
        public List<string> Indicadores_Seleccionados_Municipio { get; set; }
        public string Umbral_Seleccionado_Municipio { get; set; }

        // Total Nacional
        public string TotalAprobados { get; set; }
        public string TotalNoAprobados { get; set; }
        public string TotalAmbos { get; set; }
        public string TotalEF_ID { get; set; }
        public string TotalEF_Nombres { get; set; }
        public string Total_Municipios { get; set; }

        // De Sp Calificaciones
        public string Aprobados { get; set; }
        public string NoAprobados { get; set; }

        public string AprobadosMunicipio { get; set; }
        public string NoAprobadosMunicipio { get; set; }

        public string Ambos { get; set; }
        public string EF_Nombre { get; set; }
        public string EF_ID { get; set; }
        public string efId { get; set; }
        public string EF_Nombre_Count { get; set; }
        public string Umbral_P1 { get; set; }
        public string UmbralMunicipio_P1 { get; set; }
        public string ColumnasSeleccionadas_P1 { get; set; }
        public string ColumnasSeleccionadasMunicipio_P1 { get; set; }
        public string Entidad_Federativa { get; set; }
        public string Detalle { get; set; }


        // Resultado

        public string MPO_ID { get; set; }
        public string Municipio_Nombre { get; set; }
        public string FactorMunicipio { get; set; }

    }

    public class CheckboxItem
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }


}


