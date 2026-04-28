namespace NSIE.Models
{
    public class Modulo
    {
        public int Id { get; set; }
        public int SeccionId { get; set; }
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
        public bool Activo { get; set; }
        // ← NUEVO: para contener las vistas hijas
        public List<VistaSNIER> Vistas { get; set; } = new List<VistaSNIER>();
    }

    public class SeccionConModulos
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = "";
        public string Articulos { get; set; } = "";
        public string FundamentoLegal { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public string Ayuda { get; set; } = "";
        public string Objetivo { get; set; } = "";
        public string ResponsableNormativo { get; set; } = "";
        public string PublicoObjetivo { get; set; } = "";
        public bool Activo { get; set; } = true;
        public int Orden { get; set; } = 1; // AGREGADO

        public List<Modulo> Modulos { get; set; } = new List<Modulo>();

        // Propiedad calculada para contar módulos
        public int TotalModulos => Modulos?.Count ?? 0;
    }
}
