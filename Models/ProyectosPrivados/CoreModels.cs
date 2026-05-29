using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NSIE.Models.ProyectosPrivados.Core
{
    /* ==========================================
       1. ENTIDADES DE CATÁLOGO (core.*)
       ========================================== */

    public class CatTecnologia
    {
        public int TecnologiaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool EsRenovable { get; set; } = true;
        public bool Activo { get; set; } = true;
    }

    public class CatEstatusProyecto
    {
        public int EstatusProyectoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }

    public class CatNivelMadurez
    {
        public int NivelMadurezId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Orden { get; set; }
    }

    public class CatClasificacion
    {
        public int ClasificacionId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; } = true;
    }

    public class CatPrioridad
    {
        public int PrioridadId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Orden { get; set; }
        public bool Activo { get; set; } = true;
    }

    public class CatSemaforo
    {
        public int SemaforoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string ColorHex { get; set; } = "#808080";
        public string? Descripcion { get; set; }
        public bool Activo { get; set; } = true;
    }

    public class CatAutoridad
    {
        public int AutoridadId { get; set; }
        public string Acronimo { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
    }

    public class CatTipoTramite
    {
        public int TipoTramiteId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }

    public class CatEstatusTramite
    {
        public int EstatusTramiteId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
    }

    public class CatTipoActor
    {
        public int TipoActorId { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    public class CatMoneda
    {
        public int MonedaId { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
    }

    public class CatEntidadFederativa
    {
        public int EntidadFederativaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    public class CatMunicipio
    {
        public int MunicipioId { get; set; }
        public int EntidadFederativaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public CatEntidadFederativa? EntidadFederativa { get; set; }
    }

    public class CatOrigenDatos
    {
        public int OrigenDatosId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string TipoFuente { get; set; } = string.Empty; // Excel, SharePoint, API
    }

    public class CatValoracionMinuta
    {
        public int ValoracionMinutaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    public class CatTipoDocumento
    {
        public int TipoDocumentoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    /* ==========================================
       2. ENTIDADES OPERATIVAS MAESTRAS (core.*)
       ========================================== */

    public class Proyecto
    {
        public int ProyectoId { get; set; }
        public string? Status { get; set; }
        public string NombreOficial { get; set; } = string.Empty;
        public string? NombreCorto { get; set; }
        public string? NombreNormalizado { get; set; }
        public string? Descripcion { get; set; }
        
        public int? TecnologiaId { get; set; }
        public int? EstatusProyectoId { get; set; }
        public int? NivelMadurezId { get; set; }
        public int? ClasificacionId { get; set; }
        public int? PrioridadId { get; set; }
        public int? SemaforoId { get; set; }
        public bool? Renovable { get; set; }
        public decimal? CapacidadMW { get; set; }

        public bool Activo { get; set; } = true;
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
        public string? CreadoPor { get; set; }
        public DateTime? ActualizadoEn { get; set; }
        public string? ActualizadoPor { get; set; }

        // Propiedades de navegación opcionales
        public string? TecnologiaNombre { get; set; }
        public string? EstatusProyectoNombre { get; set; }
        public string? ClasificacionNombre { get; set; }
        public string? PrioridadNombre { get; set; }
        public string? SemaforoNombre { get; set; }
        public string? SemaforoColorHex { get; set; }

        // Relaciones
        public List<ProyectoIdentificador> Identificadores { get; set; } = new();
        public List<ProyectoActor> ProyectoActores { get; set; } = new();
        public List<ProyectoUbicacion> Ubicaciones { get; set; } = new();
        public List<ProyectoCoordenada> Coordenadas { get; set; } = new();
        public List<ProyectoTramite> Tramites { get; set; } = new();
        public List<ProyectoHito> Hitos { get; set; } = new();
        
        public ProyectoDatosTecnicos? DatosTecnicos { get; set; }
        public ProyectoDatosFinancieros? DatosFinancieros { get; set; }
    }

    public class ProyectoIdentificador
    {
        public int IdentificadorId { get; set; }
        public int ProyectoId { get; set; }
        public int OrigenDatosId { get; set; }
        public string ClaveExterna { get; set; } = string.Empty;
        public string? NombreEnOrigen { get; set; }
        
        // Propiedades de navegación mapeadas en Dapper
        public string? OrigenDatosNombre { get; set; }
    }

    public class GrupoInteresEconomico
    {
        public int GrupoEconomicoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }

    public class Actor
    {
        public int ActorId { get; set; }
        public string RazonSocial { get; set; } = string.Empty;
        public string? RFC { get; set; }
        public string? PaisOrigen { get; set; }
        public int? GrupoEconomicoId { get; set; }
        public string? DomicilioFiscal { get; set; }
        public string? EmailContacto { get; set; }
        public string? TelefonoContacto { get; set; }
        public string? Observaciones { get; set; }
        public bool Activo { get; set; } = true;
        
        // Propiedades de navegación
        public string? GrupoEconomicoNombre { get; set; }
    }

    public class ProyectoActor
    {
        public int ProyectoActorId { get; set; }
        public int ProyectoId { get; set; }
        public int ActorId { get; set; }
        public int TipoActorId { get; set; }

        public string? ActorRazonSocial { get; set; }
        public string? ActorRFC { get; set; }
        public string? TipoActorNombre { get; set; }
    }

    public class ProyectoUbicacion
    {
        public int UbicacionId { get; set; }
        public int ProyectoId { get; set; }
        public int EntidadFederativaId { get; set; }
        public int MunicipioId { get; set; }
        public string? Localidad { get; set; }
        public string? Predio { get; set; }
        public bool EsPrincipal { get; set; } = false;
        public string? SubestacionAsociada { get; set; }
        public string? PuntoInterconexion { get; set; }
        public string? RegionTransmision { get; set; }

        // Propiedades de navegación
        public string? EntidadFederativaNombre { get; set; }
        public string? MunicipioNombre { get; set; }
    }

    public class ProyectoCoordenada
    {
        public int CoordenadaId { get; set; }
        public int ProyectoId { get; set; }
        public int? UbicacionId { get; set; }
        public decimal Latitud { get; set; }
        public decimal Longitud { get; set; }
        public int Secuencia { get; set; } = 1;
        public string? Descripcion { get; set; }
    }

    public class ProyectoDatosTecnicos
    {
        public int ProyectoId { get; set; }
        public decimal CapacidadInstaladaMW { get; set; }
        public decimal? PotenciaAC_MW { get; set; }
        public decimal? PotenciaDC_MW { get; set; }
        public bool AlmacenamientoBess { get; set; } = false;
        public decimal? CapacidadBessMW { get; set; }
        public decimal? CapacidadBessMWh { get; set; }
        public decimal? HorasAlmacenamiento { get; set; }
        public decimal? ProduccionAnualEsperadaGWh { get; set; }
        public decimal? NivelTensionKV { get; set; }
        public bool EsHibrido { get; set; } = false;
        public string? ComentariosTecnicos { get; set; }
    }

    public class ProyectoDatosFinancieros
    {
        public int ProyectoId { get; set; }
        public decimal? CAPEX { get; set; }
        public int MonedaId { get; set; }
        public string? FuenteFinanciamiento { get; set; }
        public string? NombreEPC { get; set; }
        public string? ProveedoresPrincipales { get; set; }
        public string? TipoFinanciamiento { get; set; }
        public decimal? MontoPresupuestalMDP { get; set; }
        public string? EquiposAsociadosNarrativo { get; set; }
        
        // Propiedades de navegación
        public string? MonedaCodigo { get; set; }
    }

    public class ProyectoTramite
    {
        public int TramiteId { get; set; }
        public int ProyectoId { get; set; }
        public int AutoridadId { get; set; }
        public int TipoTramiteId { get; set; }
        public int EstatusTramiteId { get; set; }
        public string? Folio { get; set; }
        public DateTime? FechaIngreso { get; set; }
        public DateTime? FechaResolucion { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public string? Resultado { get; set; }
        public string? Observaciones { get; set; }
        public string? DocumentoUrl { get; set; }

        public bool Activo { get; set; } = true;
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
        public string? CreadoPor { get; set; }
        public DateTime? ActualizadoEn { get; set; }
        public string? ActualizadoPor { get; set; }

        // Propiedades de navegación
        public string? AutoridadAcronimo { get; set; }
        public string? TipoTramiteNombre { get; set; }
        public string? EstatusTramiteNombre { get; set; }
    }

    public class ProyectoHito
    {
        public int HitoId { get; set; }
        public int ProyectoId { get; set; }
        public string NombreHito { get; set; } = string.Empty;
        public DateTime FechaProgramada { get; set; }
        public DateTime? FechaReprogramada { get; set; }
        public DateTime? FechaReal { get; set; }
        public decimal AvancePorcentaje { get; set; } = 0.00m;
        public bool EsHitoCritico { get; set; } = false;
        public string? Observaciones { get; set; }
    }

    public class ProyectoBitacoraCarga
    {
        public int BitacoraCargaId { get; set; }
        public int ProyectoId { get; set; }
        public int OrigenDatosId { get; set; }
        public string ArchivoOrigen { get; set; } = string.Empty;
        public string HojaOrigen { get; set; } = string.Empty;
        public int FilaOrigen { get; set; }
        public DateTime FechaCarga { get; set; } = DateTime.UtcNow;
        public string UsuarioCarga { get; set; } = string.Empty;
        public string HashDeduplicacion { get; set; } = string.Empty;
        public string DatosRawJson { get; set; } = string.Empty;
        public string EstatusValidacion { get; set; } = "Aprobado";

        public string? OrigenDatosNombre { get; set; }
    }
}
