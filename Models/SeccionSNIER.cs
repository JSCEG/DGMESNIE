namespace NSIE.Models
{
    public class SeccionSNIER
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Articulos { get; set; }
        public string FundamentoLegal { get; set; }
        public string Descripcion { get; set; }
        public string Ayuda { get; set; }
        public string Objetivo { get; set; }
        public string ResponsableNormativo { get; set; }
        public string PublicoObjetivo { get; set; }
        public bool SeccionActiva { get; set; } // Si usas 'Activo AS SeccionActiva'
        public int Orden { get; set; } = 1;
        public List<ModuloSNIER> Modulos { get; set; } = new List<ModuloSNIER>();
    }
}
