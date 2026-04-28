namespace NSIE.Models
{
    public class NodosSankey
    {
        //Nodos Normales
        public string FEP_ID { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string FEP_Nombre_sin_espacios { get; set; }
        public int Valor { get; set; }
        public string ColorKey { get; set; }
        public string Imagen_sin_espacios { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string InfoDataImp { get; set; } // JSON como cadena
        public string InfoDataExp { get; set; }
        public int Valor_importaciones { get; set; }
        public int Valor_exportaciones { get; set; }

        public string TooltipPos { get; set; }

        public int Año { get; set; }

        // De la Vista a la BD

        public string yearSelect { get; set; }
    }

}
