namespace NSIE.Models
{
    public class LogEvaluaciones
    {
        public int IdLog { get; set; }
        public int IdUsuario { get; set; }
        public DateTime FechaYHora { get; set; }
        public string Mercado { get; set; }
        public int Año { get; set; }
        public bool Pregunta1 { get; set; }
        public bool Pregunta2 { get; set; }
        public bool Pregunta3 { get; set; }
        public string Pregunta4 { get; set; }
        public string Pregunta5 { get; set; }
        public string Pregunta6 { get; set; }
        public string Pregunta7 { get; set; }
        public string Entidad_Federativa { get; set; }
        public string Municipio { get; set; }
        public string Coordenadas { get; set; }
        public string Evaluacion { get; set; }
    }
}
