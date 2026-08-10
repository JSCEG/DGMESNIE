using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace NSIE.Models
{
    public static class PamTiposActualizacion
    {
        public const string InformePormenorizado = "InformePormenorizado";
        public const string SeguimientoTransmision = "SeguimientoTransmision";

        public static bool EsValido(string value)
            => value is InformePormenorizado or SeguimientoTransmision;
    }

    public class PamNuevaActualizacionViewModel
    {
        public HeaderViewModel Header { get; set; }
        public PamNuevaActualizacionInput Input { get; set; } = new();
        public List<PamLoteResumen> LotesRecientes { get; set; } = new();
    }

    public class PamNuevaActualizacionInput
    {
        [Required(ErrorMessage = "Selecciona el propósito de la actualización.")]
        [Display(Name = "Propósito de la actualización")]
        public string TipoActualizacion { get; set; } = PamTiposActualizacion.InformePormenorizado;

        [Required(ErrorMessage = "Escribe un nombre para identificar la actualización.")]
        [StringLength(200)]
        [Display(Name = "Nombre de la actualización")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Indica la fecha de corte de los archivos.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de corte")]
        public DateTime? FechaCorte { get; set; } = DateTime.Today;

        [StringLength(1000)]
        [Display(Name = "Nota opcional")]
        public string Notas { get; set; }

        [Required(ErrorMessage = "Selecciona al menos un archivo.")]
        [Display(Name = "Archivos")]
        public List<IFormFile> Archivos { get; set; } = new();
    }

    public class PamLoteResumen
    {
        public long LoteId { get; set; }
        public Guid LoteUid { get; set; }
        public string Nombre { get; set; }
        public string TipoActualizacion { get; set; }
        public DateTime FechaCorte { get; set; }
        public string Estado { get; set; }
        public int TotalArchivos { get; set; }
        public int TotalRegistrosDetectados { get; set; }
        public int TotalCambiosPropuestos { get; set; }
        public int TotalObservados { get; set; }
        public DateTime FechaRegistroUtc { get; set; }
        public string UsuarioNombre { get; set; }
        public string Archivos { get; set; }
        public int PendientesExtraccion { get; set; }
        public int RequierenConversion { get; set; }
    }

    public class PamLoteCreadoResultado
    {
        public long LoteId { get; set; }
        public Guid LoteUid { get; set; }
        public string TipoActualizacion { get; set; }
        public int TotalArchivos { get; set; }
        public int FuentesNuevas { get; set; }
        public int FuentesReutilizadas { get; set; }
    }

    public class PamSeguimientoPreparacionViewModel
    {
        public HeaderViewModel Header { get; set; }
        public long LoteId { get; set; }
        public Guid LoteUid { get; set; }
        public string NombreLote { get; set; }
        public DateTime FechaCorte { get; set; }
        public string EstadoLote { get; set; }
        public string FuenteNombre { get; set; }
        public string PerfilVersion { get; set; }
        public bool YaIncorporado { get; set; }
        public int TotalRegistros { get; set; }
        public int TotalUnidades { get; set; }
        public int TotalClavesPem { get; set; }
        public int ClavesVinculadas { get; set; }
        public int ClavesNoEncontradas { get; set; }
        public int RegistrosConFases { get; set; }
        public decimal ImporteTotalMdp { get; set; }
        public List<PamSeguimientoClavePendiente> Pendientes { get; set; } = new();
        public List<string> Advertencias { get; set; } = new();
    }

    public class PamSeguimientoClavePendiente
    {
        public string CodigoUnico { get; set; }
        public string CodigoPem { get; set; }
        public string NombreProyecto { get; set; }
        public int NumeroFila { get; set; }
        public string Motivo { get; set; }
    }

    public class PamSeguimientoAplicacionResultado
    {
        public long LoteId { get; set; }
        public bool YaExistia { get; set; }
        public int RegistrosIncorporados { get; set; }
        public int UnidadesIncorporadas { get; set; }
        public int VinculacionesRegistradas { get; set; }
        public int ClavesNoEncontradas { get; set; }
    }
}
