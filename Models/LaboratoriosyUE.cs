namespace NSIE.Models
{
    public class LaboratoriosyUE
    {
        public int ID { get; set; }
        public string Nombre { get; set; }
        public string Elemento_tipo { get; set; }
        public string Entidad_Federativa { get; set; }
        public string Municipio { get; set; }
        public string Dirección { get; set; }
        public string Tipo { get; set; }
        public string Norma { get; set; }
        public string Resolucion { get; set; } // Campo añadido
        public string Nombre_corto { get; set; } // Campo añadido
        public string Actividad { get; set; }
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public string Comentarios { get; set; }
        public string Estatus { get; set; } // Campo añadido
        public DateTime Fecha_actualizacion { get; set; } // Campo añadido
    }
}
