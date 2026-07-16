using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace NSIE.Models
{
    public class PamNuevaActualizacionViewModel
    {
        public HeaderViewModel Header { get; set; }
        public PamNuevaActualizacionInput Input { get; set; } = new();
        public List<PamLoteResumen> LotesRecientes { get; set; } = new();
    }

    public class PamNuevaActualizacionInput
    {
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
        public int TotalArchivos { get; set; }
        public int FuentesNuevas { get; set; }
        public int FuentesReutilizadas { get; set; }
    }
}
