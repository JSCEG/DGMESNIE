namespace NSIE.Models
{
    public class InformePormenorizadoViewModel
    {
        public HeaderViewModel Header { get; set; }
        public List<ProyectoModernizacionListado> Registros { get; set; } = new();
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

        // Paginación server-side
        public int Pagina { get; set; } = 1;
        public int TamanoPagina { get; set; } = 20;
        public int TotalPaginas => TamanoPagina <= 0 ? 1 : Math.Max(1, (int)Math.Ceiling((double)TotalRegistros / TamanoPagina));
        public int RegistroDesde => TotalRegistros == 0 ? 0 : ((Pagina - 1) * TamanoPagina) + 1;
        public int RegistroHasta => Math.Min(Pagina * TamanoPagina, TotalRegistros);
    }

    public class InformePormenorizadoResumenItem
    {
        public string Etiqueta { get; set; }
        public int Total { get; set; }
        public decimal Monto { get; set; }
    }

    /// <summary>Proyección ligera para la tabla: sin los textos largos (esos se cargan bajo demanda).</summary>
    public class ProyectoModernizacionListado
    {
        public int ProyectoModernizacionId { get; set; }
        public int Numero { get; set; }
        public string NombreProyecto { get; set; }
        public string GRT { get; set; }
        public string ClavePem { get; set; }
        public string EtapaProyecto { get; set; }
        public string TipoFinanciamiento { get; set; }
        public decimal? MontoProyectoMdp { get; set; }
        public decimal? PorcentajeAvanceEjecucion { get; set; }
        public string UniversoPresentacionPresidencia { get; set; }
        public int TotalFiltrado { get; set; }
    }

    public class InformePormenorizadoAgregados
    {
        public int TotalRegistros { get; set; }
        public decimal MontoTotalMdp { get; set; }
        public decimal AvancePromedio { get; set; }
        public int ProyectosOperacion { get; set; }
        public int ProyectosPriorizados { get; set; }
        public List<InformePormenorizadoResumenItem> ResumenEtapas { get; set; } = new();
        public List<InformePormenorizadoResumenItem> ResumenFinanciamiento { get; set; } = new();
    }
}
