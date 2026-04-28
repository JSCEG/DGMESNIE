namespace NSIE.Models
{
    public class NodosTransformaciones
    {
        //Nodos Normales
        public int TransformacionID { get; set; }
        public string Transformacion_Nombre_SE { get; set; }
        public int Año { get; set; }
        public string Tipo { get; set; }
        public int Valor { get; set; }

        // De la Vista a la BD

        public string yearSelect { get; set; }
    }

}
