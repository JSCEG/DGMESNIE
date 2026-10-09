namespace NSIE.Models
{
    // Lecturas de la vista Seguridad y Auditoría. Acceso y Sesion guardan UTC;
    // ActividadLog guarda hora de la Ciudad de México (de ahí el sufijo Local).

    public class AuditoriaResumen
    {
        public int AccesosTotales { get; set; }
        public int Accesos30Dias { get; set; }
        public int UsuariosDistintos30Dias { get; set; }
        public int SesionesAbiertas { get; set; }
        public int AccionesHoy { get; set; }
    }

    public class AuditoriaUsuario
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Cargo { get; set; }
        public string UnidadAdscripcion { get; set; }
        public DateTime? UltimoAcceso { get; set; }
        public int Accesos { get; set; }
        public int SesionesAbiertas { get; set; }
        public DateTime? UltimaAccionLocal { get; set; }
    }

    public class AuditoriaSesion
    {
        public Guid IdSesion { get; set; }
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string Cargo { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime UltimaActividad { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public bool Activa { get; set; }
        public string Ip { get; set; }
        public string UserAgent { get; set; }
        public DateTime? UltimaAccionLocal { get; set; }
    }

    public class AuditoriaAcceso
    {
        public int IdAcceso { get; set; }
        public DateTime FechaAcceso { get; set; }
        public string TipoAcceso { get; set; }
        public string Ip { get; set; }
        public bool Exitoso { get; set; }
    }

    public class AuditoriaAccion
    {
        public DateTime TimestampLocal { get; set; }
        public string Pagina { get; set; }
        public string Accion { get; set; }
        public string Controlador { get; set; }
        public string Tipo { get; set; }
        public string Elemento { get; set; }
        public string Valor { get; set; }
    }

    public class AuditoriaAccesoFecha
    {
        public DateTime FechaAcceso { get; set; }
        public int? IdUsuario { get; set; }
    }
}
