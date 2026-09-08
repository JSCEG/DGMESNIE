using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace NSIE.Models.ProyectosPrivados
{
    public sealed class CarteraConvocatoriaSeed
    {
        public string SourceVersion { get; set; } = string.Empty;
        public object? Source { get; set; }
        public List<CarteraConvocatoriaProyecto> Projects { get; set; } = new();
        public List<CarteraConvocatoriaComentario> Notes { get; set; } = new();
    }

    public sealed class CarteraConvocatoriaDatos
    {
        public string SourceVersion { get; set; } = string.Empty;
        public object? Source { get; set; }
        public List<CarteraConvocatoriaProyecto> Projects { get; set; } = new();
        public List<CarteraConvocatoriaComentario> Notes { get; set; } = new();
        public CarteraConvocatoriaSesion Session { get; set; } = new();
        public CarteraConvocatoriaCarga? LatestImport { get; set; }
    }

    public sealed class CarteraConvocatoriaProyecto
    {
        public long ProjectId { get; set; }
        public string Folio { get; set; } = string.Empty;
        public string? CanonicalFolio { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string? Technology { get; set; }
        public string? Company { get; set; }
        public string? InterestGroup { get; set; }
        public string? Substation { get; set; }
        public string? InterconnectionPoint { get; set; }
        public decimal Mw { get; set; }
        public int Rank { get; set; }
        public int Priority { get; set; }
        public string? PrioritySource { get; set; }
        public string Decision { get; set; } = "revision";
        public string? AnalysisClassification { get; set; }
        public string? TechnicalViability { get; set; }
        public string? TechnicalAnalysis { get; set; }
        public DateTime? ProjectSignedAt { get; set; }
        public string? DuplicateGroup { get; set; }
        public string Consideration { get; set; } = "revision";
        public string? UniverseStatus { get; set; }
        public string? Municipality { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? SubstationLatitude { get; set; }
        public decimal? SubstationLongitude { get; set; }
        [JsonIgnore]
        public string? ProjectKmlUrl { get; set; }
        [JsonIgnore]
        public string? SubstationKmlUrl { get; set; }
        public bool HasProjectKml { get; set; }
        public bool HasSubstationKml { get; set; }
        public bool SameKmlReference =>
            !string.IsNullOrWhiteSpace(ProjectKmlUrl) &&
            string.Equals(ProjectKmlUrl, SubstationKmlUrl, StringComparison.OrdinalIgnoreCase);
        public decimal? NetworkCostUsd { get; set; }
        public decimal? NetworkCostMxn { get; set; }
        public int? WorksCount { get; set; }
        public string? WorksDescription { get; set; }
        public long? ImportId { get; set; }
        public string? Source { get; set; }
        public int? SourceRow { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public sealed class CarteraConvocatoriaComentario
    {
        public long CommentId { get; set; }
        public string Folio { get; set; } = string.Empty;
        public string Session { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Text { get; set; } = string.Empty;
        public string? User { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public sealed class CarteraConvocatoriaSesion
    {
        public string Name { get; set; } = "Sesión de trabajo";
        public string Date { get; set; } = DateTime.Today.ToString("yyyy-MM-dd");
    }

    public sealed class CarteraConvocatoriaCarga
    {
        public long ImportId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string Sha256 { get; set; } = string.Empty;
        public string SourceVersion { get; set; } = string.Empty;
        public DateTime? CutoffDate { get; set; }
        public int SourceRows { get; set; }
        public int CatalogRows { get; set; }
        public int FirmCount { get; set; }
        public int ReviewCount { get; set; }
        public int RejectedCount { get; set; }
        public int ProjectKmlCount { get; set; }
        public int SubstationKmlCount { get; set; }
        public DateTime ImportedAt { get; set; }
        public string? ImportedBy { get; set; }
    }

    public sealed class CarteraConvocatoriaImportDocument
    {
        public Dictionary<string, CarteraConvocatoriaExpediente> Dossiers { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public string FileName { get; set; } = string.Empty;
        public long FileBytes { get; set; }
        public string Sha256 { get; set; } = string.Empty;
        public string SourceVersion { get; set; } = string.Empty;
        public DateTime? CutoffDate { get; set; }
        public int SourceRows { get; set; }
        public List<CarteraConvocatoriaProyecto> Projects { get; set; } = new();
    }

    public sealed class CarteraConvocatoriaImportResult
    {
        public bool AlreadyImported { get; set; }
        public long ImportId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string SourceVersion { get; set; } = string.Empty;
        public DateTime? CutoffDate { get; set; }
        public int Total { get; set; }
        public int Firm { get; set; }
        public int Review { get; set; }
        public int Rejected { get; set; }
        public int ProjectKml { get; set; }
        public int SubstationKml { get; set; }
        public int Added { get; set; }
        public int Removed { get; set; }
        public int ConsiderationChanges { get; set; }
    }

    public sealed class CarteraConvocatoriaFichaViewModel
    {
        public CarteraConvocatoriaExpediente? Dossier { get; set; }
        public List<CarteraConvocatoriaResumenGcr> Portfolio { get; set; } = new();
        public CarteraConvocatoriaProyecto Project { get; set; } = new();
        public List<CarteraConvocatoriaComentario> Notes { get; set; } = new();
        public List<NSIE.Models.PamUsuarioDestinatario> Recipients { get; set; } = new();
        public string SourceVersion { get; set; } = string.Empty;
        public CarteraConvocatoriaCarga? LatestImport { get; set; }
        // Fecha prevista de operación comercial de toda la cartera firme (para el programa por GCR).
        public List<CarteraConvocatoriaOperacionProgramada> OperationSchedule { get; set; } = new();
        // Clúster candidato y grupos excluyentes identificados por CFE (libro complementario).
        public CarteraConvocatoriaMarca? Marks { get; set; }
        // Decisión del área (libro de selección): preferencia, estudios CENACE, obras onerosas, excluyentes y sustitutos.
        public CarteraConvocatoriaSeleccion? Seleccion { get; set; }
        public CarteraConvocatoriaSeleccionCarga? SeleccionCarga { get; set; }
        // Integrantes del clúster y del grupo excluyente (marcas CFE) del folio, con datos de la cartera vigente.
        public List<CarteraConvocatoriaGrupoMiembro> ClusterMembers { get; set; } = new();
        public List<CarteraConvocatoriaGrupoMiembro> ExclusiveMembers { get; set; } = new();
    }

    public sealed class CarteraConvocatoriaGrupoMiembro
    {
        public string Folio { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string? Substation { get; set; }
        public string? InterestGroup { get; set; }
        public string? Technology { get; set; }
        public decimal Mw { get; set; }
        public bool HasCfeCost { get; set; }
        public decimal? NetworkCostUsd { get; set; }
        public string? Viability { get; set; }
        public string Consideration { get; set; } = "revision";
        public string? Cluster { get; set; }
        public string? Excluyente1 { get; set; }
        public string? Excluyente2 { get; set; }
        public bool IsCurrent { get; set; }
    }

    public sealed class CarteraConvocatoriaOperacionProgramada
    {
        public string Folio { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public decimal Mw { get; set; }
        public string? OperationText { get; set; }
        public DateTime? OperationDate => DateTime.TryParseExact(OperationText, "yyyy-MM-dd",
            System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var date) ? date : null;
    }

    public sealed class CarteraConvocatoriaResumenGcr
    {
        public string Region { get; set; } = string.Empty;
        public int Projects { get; set; }
        public decimal Mw { get; set; }
        public decimal NetworkCostMxn { get; set; }
        public decimal NetworkCostUsd { get; set; }
        public int ProjectsWithCostMxn { get; set; }
        public int ProjectsWithCostUsd { get; set; }
        public int Works { get; set; }
        public int ProjectsWithWorks { get; set; }
    }

    public sealed class CarteraConvocatoriaEnviarFichaInput
    {
        [Required, StringLength(60)]
        public string Folio { get; set; } = string.Empty;

        [Required, StringLength(10)]
        public string Formato { get; set; } = "pdf";

        [Required]
        public string ArchivoBase64 { get; set; } = string.Empty;

        [StringLength(240)]
        public string? NombreArchivo { get; set; }

        [Required]
        public List<int> UsuarioIds { get; set; } = new();

        [StringLength(2000)]
        public string? MensajeAdicional { get; set; }
    }

    public sealed class ActualizarEstadoConvocatoriaRequest
    {
        [Required]
        public string Folio { get; set; } = string.Empty;

        [Required]
        public string Estado { get; set; } = string.Empty;
    }

    public sealed class AgregarComentarioConvocatoriaRequest
    {
        [Required]
        public string Folio { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        public string Sesion { get; set; } = string.Empty;

        [Required]
        public DateTime Fecha { get; set; }

        [Required]
        [StringLength(4000)]
        public string Comentario { get; set; } = string.Empty;
    }

    public sealed class ActualizarPrioridadConvocatoriaRequest
    {
        [Required]
        public string Folio { get; set; } = string.Empty;

        [Range(1, 4)]
        public int Prioridad { get; set; }
    }

    public sealed class CrearProyectoConvocatoriaRequest
    {
        [Required, StringLength(60)]
        public string Folio { get; set; } = string.Empty;

        [Required, StringLength(500)]
        public string Nombre { get; set; } = string.Empty;

        [Required, StringLength(40)]
        public string Tipo { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Gerencia { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Entidad { get; set; } = string.Empty;

        [StringLength(150)]
        public string? Tecnologia { get; set; }

        [StringLength(500)]
        public string? RazonSocial { get; set; }

        [StringLength(500)]
        public string? GrupoInteres { get; set; }

        [StringLength(500)]
        public string? PuntoInterconexion { get; set; }

        [Range(0, 100000)]
        public decimal CapacidadMw { get; set; }

        [Range(1, 4)]
        public int Prioridad { get; set; } = 4;
    }
}
