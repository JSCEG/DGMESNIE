using System.ComponentModel.DataAnnotations;

namespace NSIE.Models
{
    public class ReunionesCrearAsuntoInput
    {
        [Required]
        [StringLength(300)]
        public string TituloAsunto { get; set; }

        public string Descripcion { get; set; }
        [StringLength(100)] public string Expediente { get; set; }
        public DateTime? FechaSolicitud { get; set; }
        public int? DiasTranscurridos { get; set; }
        public string Responsable { get; set; }
        public string Encargado { get; set; }
        public string Minuta { get; set; }
        public string FichaInformativaOficio { get; set; }
        public string AreaResponsable { get; set; }
        public string Estatus { get; set; }
        public string EstadoActual { get; set; }
        public string TipoAsunto { get; set; }
        public string SemaforoManual { get; set; }
        public DateTime? FechaReunion { get; set; }
        public DateTime? FechaCompromiso { get; set; }
        public DateTime? FechaAtencion { get; set; }
        public string UbicacionCarpeta { get; set; }
        public string DatosContacto { get; set; }
        public string FechaTextoReunion { get; set; }
        public bool RequiereEnvioMonica { get; set; } = true;
    }

    public class ReunionesActualizarEstatusInput
    {
        [Required]
        public int AsuntoId { get; set; }

        [Required]
        [StringLength(50)]
        public string Estatus { get; set; }

        public DateTime? FechaCompromiso { get; set; }
        public DateTime? FechaAtencion { get; set; }
    }

    public class ReunionesAgregarComentarioInput
    {
        [Required]
        public int AsuntoId { get; set; }

        [Required]
        public string Mensaje { get; set; }

        [StringLength(50)]
        public string TipoMensaje { get; set; } = "Comentario";
    }

    public class ReunionesMarcarEnvioMonicaInput
    {
        [Required]
        public int AsuntoId { get; set; }

        [StringLength(1000)]
        public string ObservacionesEnvioMonica { get; set; }
    }

    public class ReunionesDesactivarAsuntoInput
    {
        [Required]
        public int AsuntoId { get; set; }
    }
}