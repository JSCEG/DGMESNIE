namespace NSIE.Models
{
    public class MenuMecanismoTarjetas
    {
        public string MercadoId { get; set; }
        public string ImagenSrc { get; set; }
        public string Titulo { get; set; }
        public string Controlador { get; set; }
        public string Accion { get; set; }
        public bool EsDisabled { get; set; }
        public bool ShouldDisplay { get; set; }

    }
}
