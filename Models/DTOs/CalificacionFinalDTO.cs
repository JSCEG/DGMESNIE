using System.Collections.Generic;

namespace NSIE.Models.DTOs
{
    public class CalificacionFinalDTO
    {
        public string cve_ent { get; set; }
        public string cve_mun { get; set; }
        public string yearSelect { get; set; }
        public string mercadoSelect { get; set; }
        public string resultado { get; set; }
        public string ResultadoFinal { get; set; }
        public string Umbral_Seleccionado_Nal { get; set; }
        public List<string> Indicadores_Seleccionados_Nal { get; set; }
        public string Umbral_Seleccionado_Mun { get; set; }
        public List<string> Indicadores_Seleccionados_Mun { get; set; }
        public List<CheckboxItemDTO> Checkboxes { get; set; }
        public double UmbralSeleccionado { get; set; }
        public double Umbral { get; set; }
        public List<string> IndicadoresSeleccionados { get; set; }
        public List<string> Indicadores_Seleccionados { get; set; }
        public string Umbral_Seleccionado { get; set; }
        public List<string> Indicadores_Seleccionados_Municipio { get; set; }
        public string Umbral_Seleccionado_Municipio { get; set; }
        public string TotalAprobados { get; set; }
        public string TotalNoAprobados { get; set; }
        public string TotalAmbos { get; set; }
        public string TotalEF_ID { get; set; }
        public string TotalEF_Nombres { get; set; }
        public string Total_Municipios { get; set; }
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
        public string MPO_ID { get; set; }
        public string Municipio_Nombre { get; set; }
        public string FactorMunicipio { get; set; }
    }

    public class CheckboxItemDTO
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
