namespace NSIE.Models
{
    public class NodosUsoFinal
    {
        //Nodos Normales
        public int Año { get; set; }
        public float Hogares { get; set; }
        public float Transporte { get; set; }
        public float SerPubCom { get; set; }
        public float Agricultura { get; set; }
        public float Industrial { get; set; }
        public int SectorEnergia { get; set; }
        public float Hogares_Co2 { get; set; }
        public float Transporte_Co2 { get; set; }
        public float SerPubCom_Co2 { get; set; }
        public float Agricultura_Co2 { get; set; }
        public float Industrial_Co2 { get; set; }
        public float SectorEnergia_Co2 { get; set; }

        // De la Vista a la BD

        public string yearSelect { get; set; }
    }

}
