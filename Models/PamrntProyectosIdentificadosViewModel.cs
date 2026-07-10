namespace NSIE.Models
{
    public class PamrntProyectosIdentificadosViewModel
    {
        public HeaderViewModel Header { get; set; }
        public string Fuente { get; set; }
        public string VersionDatos { get; set; }
        public bool CruceBaseDisponible { get; set; }
        public string AvisoCruce { get; set; }
        public List<PamrntProyectoIdentificado> Proyectos { get; set; } = new();

        public int TotalProyectos => Proyectos.Count;
        public int TotalEncontrados => Proyectos.Count(x => x.EnBaseInformePormenorizado == true);
        public int TotalNoEncontrados => Proyectos.Count(x => x.EnBaseInformePormenorizado == false);
        public decimal InversionTotalMdp => Proyectos.Sum(x => x.InversionMdp);
    }

    public class PamrntProyectoIdentificado
    {
        public int Prioridad { get; set; }
        public string Gcr { get; set; }
        public string ClavePem { get; set; }
        public string Proyecto { get; set; }
        public string FechaNecesaria { get; set; }
        public int EjercicioPlaneacion { get; set; }
        public string ZonaAtendida { get; set; }
        public decimal InversionMdp { get; set; }
        public bool FichaDisponible { get; set; }

        public bool? EnBaseInformePormenorizado { get; set; }
        public int? ProyectoModernizacionId { get; set; }
        public string EtapaBase { get; set; }
        public decimal? MontoBaseMdp { get; set; }
        public DateTime? FechaCargaBase { get; set; }
    }

    public class PamrntFichaProyectoViewModel
    {
        public HeaderViewModel Header { get; set; }
        public PamrntProyectoIdentificado Proyecto { get; set; }
        public PamrntFichaProyecto Ficha { get; set; }
    }

    public class PamrntFichaProyecto
    {
        public string ClavePem { get; set; }
        public string Titulo { get; set; }
        public string ResumenEjecutivo { get; set; }
        public string AlternativaSeleccionada { get; set; }
        public decimal InversionMdp { get; set; }
        public string FechaNecesaria { get; set; }
        public string FechaFactible { get; set; }
        public decimal RelacionBeneficioCosto { get; set; }
        public int TotalObras { get; set; }
        public string CorredorPrincipal { get; set; }
        public string Fuente { get; set; }
        public List<string> Estados { get; set; } = new();
        public List<PamrntMetricaProyecto> MetasFisicas { get; set; } = new();
        public List<string> PendientesValidacion { get; set; } = new();
    }

    public class PamrntMetricaProyecto
    {
        public string Concepto { get; set; }
        public string Valor { get; set; }
        public string Unidad { get; set; }
    }
}
