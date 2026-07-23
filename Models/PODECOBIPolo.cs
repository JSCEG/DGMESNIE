using System.ComponentModel.DataAnnotations;

namespace NSIE.Models
{
    /// <summary>
    /// Cabecera del polo. Mapea dgmesnie.PODECOBI_Polo.
    /// </summary>
    public class PODECOBIPolo
    {
        public int PoloId { get; set; }

        [Required, StringLength(2)]
        public string Numero { get; set; }                    // '01'..'14'

        [Required, StringLength(400)]
        public string NombreOficial { get; set; }

        [StringLength(400)]
        public string NombreManual { get; set; }

        [Required, StringLength(120)]
        public string Estado { get; set; }

        [StringLength(200)]
        public string Municipio { get; set; }

        public bool Activo { get; set; } = true;

        // Jurídico
        public DateTime? FechaDeclaracion { get; set; }
        [StringLength(600)] public string UrlDeclaracion { get; set; }
        [StringLength(400)] public string Modificacion { get; set; }
        [StringLength(600)] public string UrlModificacion { get; set; }
        [StringLength(400)] public string Convenio { get; set; }
        [StringLength(600)] public string UrlConvenio { get; set; }
        [StringLength(400)] public string ComiteSesion { get; set; }

        // Superficie
        public decimal? AreaOficialHa { get; set; }
        [StringLength(80)] public string AreaManualRaw { get; set; }
        public decimal? AreaGeojsonHa { get; set; }
        public decimal? GeojsonDeltaHa { get; set; }
        public decimal? GeojsonDeltaPct { get; set; }

        // Geoespacial
        public int? GeojsonFeatureCount { get; set; }
        public bool? GeojsonValid { get; set; }
        [StringLength(400)] public string GeojsonValidity { get; set; }
        public decimal? CentroidLon { get; set; }
        public decimal? CentroidLat { get; set; }

        // Operativo
        [StringLength(40)] public string Etapa { get; set; }
        [StringLength(120)] public string Subetapa { get; set; }
        public DateTime? FechaRevisionPublica { get; set; }
        [StringLength(600)] public string UrlProyectosMexico { get; set; }
        public decimal? AvanceManualPct { get; set; }

        // Inversión / empleo
        [StringLength(400)] public string Inversion { get; set; }
        [StringLength(400)] public string Empleos { get; set; }

        // Electricidad
        [StringLength(80)]  public string DemandaElectrica { get; set; }
        [StringLength(400)] public string DemandaElectricaNota { get; set; }
        [StringLength(80)]  public string DemandaMaxima { get; set; }
        [StringLength(400)] public string DemandaMaximaNota { get; set; }
        [StringLength(80)]  public string Tension { get; set; }
        [StringLength(800)] public string Conexion { get; set; }

        // Gas
        [StringLength(200)] public string GasDisponibilidad { get; set; }
        [StringLength(400)] public string GasNota { get; set; }
        [StringLength(400)] public string Ducto { get; set; }

        // Trazabilidad
        [StringLength(40)] public string CorteFuente { get; set; }
        [StringLength(40)] public string Verificacion { get; set; }
        [StringLength(2000)] public string ComentarioVerificacion { get; set; }

        // CSV derivado (de sp_PODECOBI_Polo_Listar)
        [StringLength(2000)] public string VocacionesCsv { get; set; }
        [StringLength(600)] public string ContactoFederal { get; set; }
        [StringLength(600)] public string ContactoEstatal { get; set; }

        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaActualizacion { get; set; }
    }

    public class PODECOBIVocacion
    {
        public int VocacionId { get; set; }
        public int PoloId { get; set; }
        [StringLength(200)] public string Vocacion { get; set; }
        public int Orden { get; set; }
    }

    public class PODECOBIContacto
    {
        public int ContactoId { get; set; }
        public int PoloId { get; set; }
        [Required, StringLength(20)] public string Ambito { get; set; }    // FEDERAL|ESTATAL|MUNICIPAL
        [StringLength(250)] public string Nombre { get; set; }
        [StringLength(200)] public string Cargo { get; set; }
        [StringLength(250), EmailAddress] public string Correo { get; set; }
        [StringLength(120)] public string Telefono { get; set; }
        [StringLength(500)] public string Notas { get; set; }
    }

    public class PODECOBIFuente
    {
        public int FuenteId { get; set; }
        public int PoloId { get; set; }
        [Required, StringLength(40)] public string Tipo { get; set; }
        [StringLength(800)] public string Url { get; set; }
        public DateTime? Fecha { get; set; }
        [StringLength(800)] public string Descripcion { get; set; }
        [StringLength(64)] public string Sha256 { get; set; }
    }

    public class PODECOBIGeometria
    {
        public int GeometriaId { get; set; }
        public int PoloId { get; set; }
        public int FeatureIndex { get; set; }
        [StringLength(40)] public string GeometryType { get; set; }
        public string GeometryJson { get; set; }
        public string PropertiesJson { get; set; }
        public bool? IsValid { get; set; }
        [StringLength(400)] public string ValidityNote { get; set; }
        public decimal? AreaHa { get; set; }
        public decimal? PerimetroM { get; set; }
        [StringLength(600)] public string SourceUrl { get; set; }
        [StringLength(64)] public string Sha256 { get; set; }
        public DateTime? FechaDescarga { get; set; }
    }

    public class PODECOBIPoloDetalle
    {
        public PODECOBIPolo Polo { get; set; }
        public List<PODECOBIVocacion> Vocaciones { get; set; } = new();
        public List<PODECOBIContacto> Contactos { get; set; } = new();
        public List<PODECOBIFuente> Fuentes { get; set; } = new();
        public List<PODECOBIGeometria> Geometrias { get; set; } = new();
    }

    public class PODECOBIPoloResumen
    {
        public int TotalPolos { get; set; }
        public int EstadosCount { get; set; }
        public decimal SuperficieTotalHa { get; set; }
        public int EnEjecucion { get; set; }
        public int EnPreinversion { get; set; }
        public string UltimoCorte { get; set; }
        public DateTime? UltimaActualizacion { get; set; }
    }
}
