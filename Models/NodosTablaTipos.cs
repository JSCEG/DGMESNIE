namespace NSIE.Models
{
    public class NodosTablaTipos
    {
        //Nodos Normales
        public float Año { get; set; }
        public float CargaPico { get; set; }
        public float Intermitente { get; set; }
        public float CargaBase { get; set; }
        public float GasSeco { get; set; }
        public float GasLP { get; set; }
        public float Petrolíferos { get; set; }

        // De la Vista a la BD

        public string yearSelect { get; set; }
    }

}
