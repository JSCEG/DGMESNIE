using Microsoft.AspNetCore.Mvc;
using NSIE.Validaciones;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace NSIE.Models
{
    public class ResultadoMunicipios
    {

        //Fecha de Solicitud
        public string Fecha_solicitud { get; set; }

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
        // public string EF_Nombre { get; set; }
        // public string EF_ID { get; set; }
        public string efId { get; set; }
        public string EF_Nombre_Count { get; set; }
        //    public string Umbral_P1 { get; set; }
        //  public string UmbralMunicipio_P1 { get; set; }
        // public string ColumnasSeleccionadas_P1 { get; set; }
        // public string ColumnasSeleccionadasMunicipio_P1 { get; set; }
        public string Entidad_Federativa { get; set; }
        public string Detalle { get; set; }


        // Resultado

        //      public string MPO_ID { get; set; }
        // public string Municipio_Nombre { get; set; }
        //  public string FactorMunicipio { get; set; }


        //Salida a detalle

        public string ID { get; set; }
        public string EF_ID { get; set; }
        public string EF_Nombre { get; set; }
        public string MPO_ID { get; set; }
        public string Municipio_Nombre { get; set; }
        public string Documentos_completos { get; set; }
        public string Analisis_riesgo { get; set; }
        public string X_Geo { get; set; }
        public string Y_Geo { get; set; }
        public string _2023_01 { get; set; }
        public string _2023_02 { get; set; }
        public string _2023_03 { get; set; }
        public string _2023_04 { get; set; }
        public string _2023_05 { get; set; }
        public string _2023_06 { get; set; }
        public string _2023_07 { get; set; }
        public string _2023_08 { get; set; }
        public string _2023_09 { get; set; }
        public string _2023_10 { get; set; }
        public string _2023_11 { get; set; }
        public string _2023_12 { get; set; }
        public string _2023_13 { get; set; }
        public string _2023_14 { get; set; }
        public string _2023_15 { get; set; }
        public string _2023_01_m { get; set; }
        public string _2023_02_m { get; set; }
        public string _2023_03_m { get; set; }
        public string _2023_04_m { get; set; }
        public string _2023_05_m { get; set; }
        public string _2023_06_m { get; set; }
        public string _2023_08_m { get; set; }
        public string _2023_12_m { get; set; }
        public string Turno { get; set; }
        public string Marca_solicitada { get; set; }
        public string Expediente { get; set; }
        public string Razon_social { get; set; }
        public string SumaTotal { get; set; }
        public string TotalColumnas { get; set; }
        public string Factor { get; set; }
        public string Resultado { get; set; }
        public string Umbral_P1 { get; set; }
        public string ColumnasSeleccionadas_P1 { get; set; }
        public string SumaTotalMunicipio { get; set; }
        public string TotalColumnasMunicipio { get; set; }
        public string FactorMunicipio { get; set; }
        public string ResultadoMunicipio { get; set; }
        public string UmbralMunicipio_P1 { get; set; }
        public string ColumnasSeleccionadasMunicipio_P1 { get; set; }
        public string Categoria { get; set; }





    }

    //public class CheckboxItem
    //{
    //    public string Name { get; set; }
    //    public string Value { get; set; }
    //}


}


