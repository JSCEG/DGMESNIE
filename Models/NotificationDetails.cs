namespace NSIE.Models
{
    public class NotificationDetails
    {
        public string Titulo { get; set; }
        public string Mensaje { get; set; }
        public DateTime Fecha { get; set; }
        public string Link { get; set; }
        public string Imagen { get; set; } // Nueva propiedad para imagenes
    }
}