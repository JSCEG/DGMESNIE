using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
    }

    public sealed class CarteraConvocatoriaProyecto
    {
        public long ProjectId { get; set; }
        public string Folio { get; set; } = string.Empty;
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
