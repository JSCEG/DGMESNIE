namespace NSIE.Models
{
    public class DetalleCalificacionEF_Municipio
    {
        public int EF_ID { get; set; }
        public string EF_Nombre { get; set; }
        public int Aprobados { get; set; }
        public int NoAprobados { get; set; }
        public decimal Umbral { get; set; }
        public string ColumnasSeleccionadas { get; set; }
    }
}
