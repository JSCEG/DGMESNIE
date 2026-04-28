namespace NSIE.Models
{
    public class EnergiaSankeyUsosFinales
    {
        public int Año { get; set; }
        public double Hogares { get; set; }
        public double Transporte { get; set; }
        public double SerPubCom { get; set; }
        public double Agricultura { get; set; }
        public double Industrial { get; set; }
        public double SectorEnergia { get; set; }


        public double Hogares_Co2 { get; set; }
        public double Transporte_Co2 { get; set; }
        public double SerPubCom_Co2 { get; set; }
        public double Agricultura_Co2 { get; set; }
        public double Industrial_Co2 { get; set; }
        public double SectorEnergia_Co2 { get; set; }
    }
}
