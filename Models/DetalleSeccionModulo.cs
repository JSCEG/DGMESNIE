namespace NSIE.Models
{
    public class DetalleSeccionModulo
    {
        // Sección
        public int SeccionId { get; set; }
        public string Titulo { get; set; }
        public string Articulos { get; set; }
        public string FundamentoLegal { get; set; }
        public string Descripcion { get; set; }
        public string Ayuda { get; set; }
        public string Objetivo { get; set; }
        public string ResponsableNormativo { get; set; }
        public string PublicoObjetivo { get; set; }
        public bool SeccionActiva { get; set; }

        // Módulo
        public int ModuloId { get; set; }
        public string Title { get; set; }
        public string FundamentoLegalModulo { get; set; }
        public string Roles { get; set; }
        public string NombresRoles { get; set; }
        public string Perfiles { get; set; }
        public string Etapa { get; set; }
        public string JustificacionOrden { get; set; }
        public string AyudaContextual { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Desc { get; set; }
        public string Img { get; set; }
        public string Btn { get; set; }
        public string ElementosUI { get; set; }
        public string AyudaVista { get; set; }
        public int Orden { get; set; }
        public bool ModuloActivo { get; set; }
    }
}
