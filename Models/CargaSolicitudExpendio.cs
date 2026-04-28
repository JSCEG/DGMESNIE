namespace NSIE.Models
{
    public class CargaSolicitudExpendio
    {
        public int ID { get; set; }
        public string EF_ID { get; set; }
        public string MPO_ID { get; set; }
        public DateTime Fecha_solicitud { get; set; }
        public string Turno { get; set; }
        public string Expediente { get; set; }
        public string Razon_social { get; set; }
        public string Actividad_regulada { get; set; }
        public string Direccion { get; set; }
        public double X_Geo { get; set; }
        public double Y_Geo { get; set; }
        public string Marca_solicitada { get; set; }
        public bool Documentos_completos { get; set; }
        public bool Analisis_riesgo { get; set; }
        public bool vecinos_PEMEX { get; set; }
        public bool vecinos_OTRAS { get; set; }
        public string Tipo_Persona { get; set; }
        public string RFC { get; set; }
        public string Codigo_Postal { get; set; }
        public string Comentarios { get; set; }
    }

}
