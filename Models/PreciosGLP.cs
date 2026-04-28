namespace NSIE.Models
{
    public class PreciosGLP
    {
        public DateTime Fecha { get; set; }
        public DateTime FechaAntigua { get; set; }
        public DateTime FechaReciente { get; set; }
        public string Region { get; set; }
        //public string Entidad_Federativa { get; set; }
        //public string Municipio { get; set; }
        public decimal Precio { get; set; }
    }
}
