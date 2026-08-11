namespace NSIE.Models
{
    /// <summary>
    /// Estados vacíos, sin permiso y de error, dibujados con el mismo patrón:
    /// ícono, título, una o dos líneas que dicen qué pasó y qué se conserva, y
    /// hasta dos acciones —la que resuelve y la que sale—.
    /// Se rinde con la parcial Shared/_Estado.cshtml.
    /// </summary>
    public class EstadoViewModel
    {
        /// <summary>Clase del ícono, por ejemplo "fas fa-inbox".</summary>
        public string Icono { get; set; } = "fas fa-inbox";

        public string Titulo { get; set; } = "";

        /// <summary>Qué pasó y, sobre todo, qué se conserva.</summary>
        public string Texto { get; set; } = "";

        /// <summary>
        /// "vacio" para un filtro sin resultados (caja normal) o "pendiente"
        /// para un módulo que aún no tiene capturas (caja punteada).
        /// </summary>
        public string Variante { get; set; } = "vacio";

        public string AccionTexto { get; set; }
        public string AccionUrl { get; set; }
        public string AccionIcono { get; set; }

        public string SalidaTexto { get; set; }
        public string SalidaUrl { get; set; }
    }
}
