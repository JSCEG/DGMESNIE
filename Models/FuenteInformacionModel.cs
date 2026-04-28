using System;

namespace NSIE.Models
{
    public class FuenteInformacionModel
    {
        public int ID { get; set; }
        public string Entidad { get; set; }
        public string Tipo { get; set; }
        public string Rubro { get; set; }
        public string Etiqueta { get; set; }
        public string Dato_Informacion { get; set; }
        public string Desagregacion { get; set; }
        public string Sub_desagregacion { get; set; }
        public string Unidades { get; set; }
        public string Periodicidad_Corte_de_Informacion { get; set; }
        public string Fuente_Link { get; set; }
        public string Comentario { get; set; }
    }
}