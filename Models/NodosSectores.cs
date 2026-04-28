namespace NSIE.Models
{
    public class NodosSectores
    {
        //Nodos Normales
        public int SectorID { get; set; }
        public string Sector_Nombre_SE { get; set; }
        public int Año { get; set; }
        public string Tipo_SE { get; set; }
        public float Valor { get; set; }
        public float Valor_Co2 { get; set; }

        // De la Vista a la BD

        public string yearSelect { get; set; }
    }

}
