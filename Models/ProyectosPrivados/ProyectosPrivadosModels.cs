using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NSIE.Models.ProyectosPrivados
{
    public class Proyecto
    {
        public int ProyectoId { get; set; }
        public string Status { get; set; }
        public string Nombre { get; set; }
        public string NombreNormalizado { get; set; }
        public int? EmpresaId { get; set; }
        public string Promovente { get; set; }
        public string GrupoEconomico { get; set; }
        public string Origen { get; set; }
        public int? Anio { get; set; }
        public string AdicionesSustituciones { get; set; }
        public string ContratoUnidad { get; set; }
        public string Tipo { get; set; }
        public bool? Renovable { get; set; }
        public decimal? CapacidadMW { get; set; }
        public string Mes { get; set; }
        public string GerenciaControl { get; set; }
        public string RegionTransmision { get; set; }
        public string EntidadFederativa { get; set; }
        public string Municipio { get; set; }
        public decimal? Longitud { get; set; }
        public decimal? Latitud { get; set; }
        public string Firmes { get; set; }
        public decimal? PorcentajeConstruccion { get; set; }
        public string FolioEvIS_MISSE { get; set; }
        public string StatusEvIS_MISSE { get; set; }
        public string StatusCPLI { get; set; }
        public string ConflictosSociales { get; set; }
        public string NumeroPermiso { get; set; }
        public DateTime? FechaInicioObras { get; set; }
        public DateTime? FechaTerminacionObras { get; set; }
        public DateTime? FechaEntradaOperacion { get; set; }
        public string EstadoProgramaObras { get; set; }
        public string TramiteCNE { get; set; }
        public string ObservacionesCNE { get; set; }
        public string Categoria { get; set; }
        public bool? InteresadaEnContinuar { get; set; }
        public string ObservacionesUEVISPI { get; set; }
        public string ResumenCaso { get; set; }
        public string PropuestaAtencion { get; set; }
        public string RequiereAlmacenamiento { get; set; }
        public string SiguientesPasos { get; set; }
        public int? ClasificacionId { get; set; }
        public int? PrioridadId { get; set; }
        public int? SemaforoId { get; set; }
        public string RazonesBreves { get; set; }
        public string TramitesSemarnat { get; set; }
        public string EstatusSemarnat { get; set; }
        public string ObservacionesSemarnat { get; set; }
        public string Fuente { get; set; }
        public string SemaforoPPT { get; set; }
        public DateTime? FechaUltimaActualizacion { get; set; }
        public string FuenteUltimaActualizacion { get; set; }
        public bool Activo { get; set; }
        public DateTime CreadoEn { get; set; }
        public string CreadoPor { get; set; }
        public DateTime? ActualizadoEn { get; set; }
        public string ActualizadoPor { get; set; }

        // Navigation properties or mapped text
        public string EmpresaNombre { get; set; }
        public string ClasificacionNombre { get; set; }
        public string PrioridadNombre { get; set; }
        public string SemaforoNombre { get; set; }
        public string SemaforoColorHex { get; set; }
    }

    public class Empresa
    {
        public int EmpresaId { get; set; }
        public string Nombre { get; set; }
        public string GrupoEconomico { get; set; }
        public string PaisOrigen { get; set; }
        public string Observaciones { get; set; }
        public bool Activo { get; set; }
        public DateTime CreadoEn { get; set; }
        public string CreadoPor { get; set; }
    }

    public class TramiteProyecto
    {
        public int TramiteProyectoId { get; set; }
        public int ProyectoId { get; set; }
        public string TipoTramite { get; set; }
        public string Folio { get; set; }
        public int? EstatusTramiteId { get; set; }
        public string EstatusTramiteNombre { get; set; }
        public string EstatusTexto { get; set; }
        public DateTime? FechaIngreso { get; set; }
        public DateTime? FechaResolucion { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public string Observaciones { get; set; }
        public string Autoridad { get; set; }
        public string Fuente { get; set; }
        public bool Activo { get; set; }
    }

    public class Documento
    {
        public int DocumentoId { get; set; }
        public int TipoDocumentoId { get; set; }
        public string TipoDocumentoNombre { get; set; }
        public string Titulo { get; set; }
        public string SharePointUrl { get; set; }
        public string SharePointItemId { get; set; }
        public string SharePointDriveId { get; set; }
        public string NombreArchivo { get; set; }
        public DateTime? FechaDocumento { get; set; }
        public string Descripcion { get; set; }
        public string SubidoPor { get; set; }
        public DateTime SubidoEn { get; set; }
        public bool Activo { get; set; }
    }

    public class Reunion
    {
        public int ReunionId { get; set; }
        public string Titulo { get; set; }
        public DateTime FechaReunion { get; set; }
        public string Modalidad { get; set; }
        public string Lugar { get; set; }
        public string Objetivo { get; set; }
        public string Asistentes { get; set; }
        public int? DocumentoId { get; set; }
        public string DocumentoTitulo { get; set; }
        public string DocumentoSharePointUrl { get; set; }
    }

    public class BitacoraProyecto
    {
        public int BitacoraProyectoId { get; set; }
        public int ProyectoId { get; set; }
        public string ProyectoNombre { get; set; }
        public int? ReunionId { get; set; }
        public string ReunionTitulo { get; set; }
        public DateTime? ReunionFecha { get; set; }
        public int? DocumentoId { get; set; }
        public DateTime FechaEvento { get; set; }
        public int? ValoracionMinutaId { get; set; }
        public string ValoracionMinutaNombre { get; set; }
        public string ValoracionTexto { get; set; }
        public string ResumenAcuerdos { get; set; }
        public string CompromisosSiguientesPasos { get; set; }
        public string RiesgosObservaciones { get; set; }
        public string SnapshotProyectoJSON { get; set; }
        public string ProcesadoPor { get; set; }
        public string CreadoPor { get; set; }
        public DateTime CreadoEn { get; set; }
    }

    public class AccionSeguimiento
    {
        public int AccionId { get; set; }
        public int ProyectoId { get; set; }
        public string ProyectoNombre { get; set; }
        public int? BitacoraProyectoId { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string ResponsableUsuarioId { get; set; }
        public string ResponsableNombre { get; set; }
        public DateTime? FechaCompromiso { get; set; }
        public DateTime? FechaCierre { get; set; }
        public string Estatus { get; set; } // Pendiente, En proceso, Bloqueada, Cerrada, Cancelada
        public int? SemaforoId { get; set; }
        public string SemaforoNombre { get; set; }
        public string SemaforoColorHex { get; set; }
        public string Comentarios { get; set; }
        public DateTime CreadoEn { get; set; }
        public string CreadoPor { get; set; }
    }

    public class HistorialProyecto
    {
        public int HistorialProyectoId { get; set; }
        public int ProyectoId { get; set; }
        public string ProyectoNombre { get; set; }
        public int? BitacoraProyectoId { get; set; }
        public int? DocumentoId { get; set; }
        public string Campo { get; set; }
        public string ValorAnterior { get; set; }
        public string ValorNuevo { get; set; }
        public string MotivoCambio { get; set; }
        public DateTime CambiadoEn { get; set; }
        public string CambiadoPor { get; set; }
    }

    // Catalog items
    public class CatalogoItem
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
    }

    // VM and Forms
    public class ProyectoForm
    {
        public int? ProyectoId { get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }
        public string Status { get; set; }
        public int? EmpresaId { get; set; }
        public string Promovente { get; set; }
        public string GrupoEconomico { get; set; }
        public string Origen { get; set; }
        public int? Anio { get; set; }
        public string AdicionesSustituciones { get; set; }
        public string ContratoUnidad { get; set; }
        public string Tipo { get; set; }
        public bool? Renovable { get; set; }
        public decimal? CapacidadMW { get; set; }
        public string Mes { get; set; }
        public string GerenciaControl { get; set; }
        public string RegionTransmision { get; set; }
        public string EntidadFederativa { get; set; }
        public string Municipio { get; set; }
        public decimal? Longitud { get; set; }
        public decimal? Latitud { get; set; }
        public string Firmes { get; set; }
        public decimal? PorcentajeConstruccion { get; set; }
        public string FolioEvIS_MISSE { get; set; }
        public string StatusEvIS_MISSE { get; set; }
        public string StatusCPLI { get; set; }
        public string ConflictosSociales { get; set; }
        public string NumeroPermiso { get; set; }
        public DateTime? FechaInicioObras { get; set; }
        public DateTime? FechaTerminacionObras { get; set; }
        public DateTime? FechaEntradaOperacion { get; set; }
        public string EstadoProgramaObras { get; set; }
        public string TramiteCNE { get; set; }
        public string ObservacionesCNE { get; set; }
        public string Categoria { get; set; }
        public bool? InteresadaEnContinuar { get; set; }
        public string ObservacionesUEVISPI { get; set; }
        public string ResumenCaso { get; set; }
        public string PropuestaAtencion { get; set; }
        public string RequiereAlmacenamiento { get; set; }
        public string SiguientesPasos { get; set; }
        public int? ClasificacionId { get; set; }
        public int? PrioridadId { get; set; }
        public int? SemaforoId { get; set; }
        public string RazonesBreves { get; set; }
        public string TramitesSemarnat { get; set; }
        public string EstatusSemarnat { get; set; }
        public string ObservacionesSemarnat { get; set; }
        public string Fuente { get; set; }
        public string SemaforoPPT { get; set; }
        public DateTime? FechaUltimaActualizacion { get; set; }
        public string FuenteUltimaActualizacion { get; set; }
    }

    public class ReunionForm
    {
        public int? ReunionId { get; set; }
        [Required(ErrorMessage = "El título de la reunión es obligatorio")]
        public string Titulo { get; set; }
        [Required(ErrorMessage = "La fecha de la reunión es obligatoria")]
        public DateTime FechaReunion { get; set; }
        public string Modalidad { get; set; }
        public string Lugar { get; set; }
        public string Objetivo { get; set; }
        public string Asistentes { get; set; }
        public string SharePointUrl { get; set; }
        public List<BitacoraEntradaForm> ProyectoEntradas { get; set; } = new List<BitacoraEntradaForm>();
    }

    public class BitacoraEntradaForm
    {
        public int ProyectoId { get; set; }
        public int? ValoracionMinutaId { get; set; }
        public string ValoracionTexto { get; set; }
        public string ResumenAcuerdos { get; set; }
        public string CompromisosSiguientesPasos { get; set; }
        public string RiesgosObservaciones { get; set; }
        // Fields that can be updated directly from the meeting form
        public string Status { get; set; }
        public decimal? CapacidadMW { get; set; }
        public string Categoria { get; set; }
        public int? ClasificacionId { get; set; }
        public int? PrioridadId { get; set; }
        public int? SemaforoId { get; set; }
        public string SiguientesPasos { get; set; }
        // Follow-up actions to spawn
        public List<AccionProyectoForm> AccionesAInsertar { get; set; } = new List<AccionProyectoForm>();
    }

    public class AccionProyectoForm
    {
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string ResponsableUsuarioId { get; set; }
        public string ResponsableNombre { get; set; }
        public DateTime? FechaCompromiso { get; set; }
    }

    public class ProyectoDetailVM
    {
        public Proyecto Proyecto { get; set; }
        public List<TramiteProyecto> Tramites { get; set; } = new List<TramiteProyecto>();
        public List<BitacoraProyecto> Bitacoras { get; set; } = new List<BitacoraProyecto>();
        public List<AccionSeguimiento> Acciones { get; set; } = new List<AccionSeguimiento>();
        public List<Documento> Documentos { get; set; } = new List<Documento>();
        public List<HistorialProyecto> Historial { get; set; } = new List<HistorialProyecto>();
    }

    public class DashboardKPIsVM
    {
        public int TotalProyectos { get; set; }
        public int TotalRenovables { get; set; }
        public decimal CapacidadTotalMW { get; set; }
        public int AccionesPendientes { get; set; }
        public List<Proyecto> ProyectosRecientes { get; set; } = new List<Proyecto>();
    }
}
