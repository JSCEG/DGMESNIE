namespace NSIE.Models
{
    public class NodosGrafica
    {
        //Nodos Normales
        public int UsoFinalID { get; set; }
        public string UsoFinal_Nombre_SE { get; set; }
        public int ConsumoID { get; set; }
        public string Consumo_Nombre_SE { get; set; }
        public int Año { get; set; }
        public float ConsumoUF_Valor { get; set; }

        // De la Vista a la BD

        public string yearSelect { get; set; }
    }

}
