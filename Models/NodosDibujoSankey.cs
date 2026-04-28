namespace NSIE.Models
{
    public class NodosDibujoSankey
    {
        //Nodos Normales
        public string FEP_ID { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string FEP_Nombre { get; set; }
        public string ColorKey { get; set; }
        public string Imagen { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string InfoDataImp { get; set; } // JSON como cadena
        public string InfoDataExp { get; set; }
        public string TooltipPos { get; set; }

        // De la Vista a la BD
    }

}
