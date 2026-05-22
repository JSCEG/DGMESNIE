namespace NSIE.Models
{
    public class ReunionesSeguimientoItem
    {
        public int AsuntoId { get; set; }
        public Guid GuidAsunto { get; set; }
        public int? NumeroRegistro { get; set; }
        public string TituloAsunto { get; set; }
        public string Descripcion { get; set; }
        public string Expediente { get; set; }
        public DateTime? FechaSolicitud { get; set; }
        public int? DiasTranscurridos { get; set; }
        public string Responsable { get; set; }
        public string Encargado { get; set; }
        public string Minuta { get; set; }
        public string FichaInformativaOficio { get; set; }
        public string AreaResponsable { get; set; }
        public string Estatus { get; set; }
        public string EstadoActual { get; set; }
        public string Prioridad { get; set; }
        public string TipoAsunto { get; set; }
        public string Semaforo { get; set; }
        public string SemaforoManual { get; set; }
        public DateTime? FechaReunion { get; set; }
        public DateTime? FechaCompromiso { get; set; }
        public DateTime? FechaAtencion { get; set; }
        public string CarpetaSharePointUrl { get; set; }
        public string UbicacionCarpeta { get; set; }
        public string DatosContacto { get; set; }
        public string FechaTextoReunion { get; set; }
        public bool RequiereEnvioMonica { get; set; }
        public bool EnviadoAMonica { get; set; }
        public DateTime? FechaEnvioMonica { get; set; }
        public int? EnviadoAMonicaPorIdUsuario { get; set; }
        public string EnviadoAMonicaPorNombre { get; set; }
        public string ObservacionesEnvioMonica { get; set; }
        public string DestinatarioJuridico { get; set; }
        public bool Activo { get; set; }
        public string NombreUsuarioCreacion { get; set; }
        public string NombreUsuarioUltimaActualizacion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaActualizacion { get; set; }
        public string AlertaEnvioMonica { get; set; }
        public int TotalComentarios { get; set; }
    }
}