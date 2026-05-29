using System;
using System.Collections.Generic;

namespace NSIE.Models.ProyectosPrivados
{
    public class ProyectoSummaryDto
    {
        public int ProyectoId { get; set; }
        public string NombreOficial { get; set; } = string.Empty;
        public string? NombreCorto { get; set; }
        public string? TecnologiaNombre { get; set; }
        public string? EstatusNombre { get; set; }
        public decimal? CapacidadInstaladaMW { get; set; }
        public string? UbicacionesConcatenadas { get; set; }
        public string? PromoventePrincipal { get; set; }
        public string? SemaforoColorHex { get; set; }
        public string? SemaforoNombre { get; set; }
    }

    public class IngestionResultDto
    {
        public string ArchivoNombre { get; set; } = string.Empty;
        public int TotalFilasProcesadas { get; set; }
        public int NuevosProyectosInsertados { get; set; }
        public int IdentificadoresAsociados { get; set; } // Fusión automática
        public int ConflictosDetectados { get; set; } // Cola manual
        public List<string> MensajesLog { get; set; } = new();
    }

    public class ConflictResolutionDto
    {
        public int TempRecordId { get; set; } // ID en staging
        public string NombreProyectoOrigen { get; set; } = string.Empty;
        public string? RazonSocialOrigen { get; set; }
        public string? TecnologiaOrigen { get; set; }
        public decimal? CapacidadOrigen { get; set; }
        public string? ClaveExternaOrigen { get; set; }
        
        // Datos del proyecto canónico sugerido
        public int ProyectoCanonicoId { get; set; }
        public string NombreCanonico { get; set; } = string.Empty;
        public string? RazonSocialCanonico { get; set; }
        public string? TecnologiaCanonica { get; set; }
        public decimal? CapacidadCanonica { get; set; }
        
        public double MatchScore { get; set; } // Porcentaje de similitud

        // Campos para comparación detallada en la UI
        public string? EsHibridoOrigen { get; set; }
        public bool? EsHibridoCanonico { get; set; }
        public string? FoliosCanonicos { get; set; }
    }

    public class ConflictResolutionActionDto
    {
        public int StagingId { get; set; }
        public string Accion { get; set; } = string.Empty; // "FUSIONAR" o "NUEVO"
        public int? ProyectoCanonicoId { get; set; }
    }
}
