namespace NSIE.Models
{
    public class InformePormenorizadoViewModel
    {
        public HeaderViewModel Header { get; set; }
        public List<ProyectoModernizacionRegistro> Registros { get; set; } = new();
        public string Busqueda { get; set; }
        public string Etapa { get; set; }
        public string TipoFinanciamiento { get; set; }
        public string Universo { get; set; }
        public List<string> EtapasDisponibles { get; set; } = new();
        public List<string> TiposFinanciamientoDisponibles { get; set; } = new();
        public List<string> UniversosDisponibles { get; set; } = new();
        public int TotalRegistros { get; set; }
        public decimal MontoTotalMdp { get; set; }
        public decimal AvancePromedio { get; set; }
        public int ProyectosOperacion { get; set; }
        public int ProyectosPriorizados { get; set; }
        public List<InformePormenorizadoResumenItem> ResumenEtapas { get; set; } = new();
        public List<InformePormenorizadoResumenItem> ResumenFinanciamiento { get; set; } = new();
    }

    public class InformePormenorizadoResumenItem
    {
        public string Etiqueta { get; set; }
        public int Total { get; set; }
        public decimal Monto { get; set; }
    }
}