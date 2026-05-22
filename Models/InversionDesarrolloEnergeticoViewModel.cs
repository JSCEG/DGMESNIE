namespace NSIE.Models
{
    public class InversionDesarrolloEnergeticoViewModel
    {
        public HeaderViewModel Header { get; set; }
        public string EntidadSeleccionada { get; set; }
        public string TipoInversionSeleccionado { get; set; }
        public List<string> EntidadesDisponibles { get; set; } = new();
        public List<string> TiposInversionDisponibles { get; set; } = new();
        public List<PvirseRegistro> Registros { get; set; } = new();
        public int TotalRegistros { get; set; }
        public decimal CapacidadTotalMw { get; set; }
        public int TotalAdiciones { get; set; }
        public int TotalSustituciones { get; set; }
        public int TotalCfe { get; set; }
        public int TotalParticulares { get; set; }
        public InversionEnergeticaEntidad ReporteActual { get; set; } = new();
        public IReadOnlyList<InversionEnergeticaEntidad> Reportes { get; set; } = Array.Empty<InversionEnergeticaEntidad>();
        public DateTime FechaActualizacion { get; set; }
        public string FuenteDatos { get; set; }
    }

    public class PvirseRegistro
    {
        public int PvirseRegistroId { get; set; }
        public string Status { get; set; }
        public string StatusVf { get; set; }
        public string NombreReal { get; set; }
        public string NoConsiderar { get; set; }
        public int? Anio { get; set; }
        public string AdicionesOSustituciones { get; set; }
        public string ContratoOUnidad { get; set; }
        public string Tipo { get; set; }
        public string TipoVf { get; set; }
        public string Renovable { get; set; }
        public decimal? Mw { get; set; }
        public string Mes { get; set; }
        public string GerenciaDeControl { get; set; }
        public string RegionDeTransmision { get; set; }
        public string EntidadFederativa { get; set; }
        public string Municipio { get; set; }
        public string Firmes { get; set; }
        public string FuenteArchivo { get; set; }
        public DateTime FechaCarga { get; set; }
        public bool Activo { get; set; }
    }

    public class InversionEnergeticaDataset
    {
        public DateTime FechaActualizacion { get; set; }
        public string FuenteDatos { get; set; }
        public List<InversionEnergeticaEntidad> Reportes { get; set; } = new();
    }

    public class InversionEnergeticaEntidad
    {
        public string EntidadFederativa { get; set; }
        public string Subtitulo { get; set; }
        public bool TieneDatos { get; set; } = true;
        public string NotaSinDatos { get; set; }
        public decimal InversionTotalMdp { get; set; }
        public decimal CapacidadInstaladaMw { get; set; }
        public decimal DemandaMaximaMw { get; set; }
        public GeneracionElectricaResumen GeneracionElectrica { get; set; } = new();
        public List<OfertaDemandaAnual> OfertaDemanda { get; set; } = new();
        public InfraestructuraResumen Transmision { get; set; } = new();
        public InfraestructuraResumen Distribucion { get; set; } = new();
        public List<ResumenSectorItem> ResumenInversion { get; set; } = new();
        public List<string> HallazgosClave { get; set; } = new();
    }

    public class GeneracionElectricaResumen
    {
        public decimal InversionPublicaCfeMdp { get; set; }
        public decimal InversionPrivadaMdp { get; set; }
        public decimal InversionPublicoPrivadaMdp { get; set; }
        public decimal TotalMdp => InversionPublicaCfeMdp + InversionPrivadaMdp + InversionPublicoPrivadaMdp;
        public List<ProyectoEnergeticoItem> Proyectos { get; set; } = new();
    }

    public class InfraestructuraResumen
    {
        public string Tipo { get; set; }
        public decimal InversionEstimadaMdp { get; set; }
        public List<ProyectoEnergeticoItem> Proyectos { get; set; } = new();
    }

    public class ProyectoEnergeticoItem
    {
        public string Nombre { get; set; }
        public string Categoria { get; set; }
        public string Estatus { get; set; }
        public decimal InversionMdp { get; set; }
        public string Descripcion { get; set; }
        public string Cobertura { get; set; }
    }

    public class OfertaDemandaAnual
    {
        public int Anio { get; set; }
        public decimal CapacidadInstaladaMw { get; set; }
        public decimal DemandaMaximaMw { get; set; }
    }

    public class ResumenSectorItem
    {
        public string Sector { get; set; }
        public decimal MontoMdp { get; set; }
        public decimal Porcentaje { get; set; }
    }
}