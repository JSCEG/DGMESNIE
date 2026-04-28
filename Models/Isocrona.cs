namespace NSIE.Models
{
    public class Isocrona
    {
        //Para saber que mercado es
        public string mercadoSelect { get; set; }
        //Consulta
        public double x1 { get; set; }
        public double y1 { get; set; }
        public double x2 { get; set; }
        public double y2 { get; set; }

        //Respuesta
        public int TotalAutomoviles { get; set; }
        public int TotalMotos { get; set; }

        // Lista de expendios autorizados en la zona
        public List<ExpendioAutorizadoCP> ExpendiosAutorizadosCP { get; set; }
    }

    public class ExpendioAutorizadoCP
    {
        public string NumeroPermiso { get; set; }
        public string RazonSocial { get; set; }
        public double LatitudGeo { get; set; }
        public double LongitudGeo { get; set; }
    }
}
