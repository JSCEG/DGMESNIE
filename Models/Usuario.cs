namespace NSIE.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string Correo { get; set; }
        public string Clave { get; set; }
        public string Nombre { get; set; }


        public string ConfirmarClave { get; set; }
    }

    public class UserViewModel
    {
        public int IdUsuario { get; set; }
        public string Correo { get; set; }
        public string Clave { get; set; }
        public string ConfirmarClave { get; set; }
        public string Nombre { get; set; }
        public string Unidad_de_Adscripcion { get; set; }
        public string Cargo { get; set; }
        public bool SesionActiva { get; set; }
        public DateTime UltimaActualizacion { get; set; }
        public string RFC { get; set; }
        public bool Vigente { get; set; }
        public string ClaveEmpleado { get; set; }
        public DateTime HoraInicioSesion { get; set; }


        // Roles Usuarios
        public int Rol { get; set; }
        //  public int Rol_Id_RU { get; set; }
        public int Mercado_ID { get; set; }
        public bool RolUsuario_Vigente { get; set; }
        public string RolUsuario_QuienRegistro { get; set; }
        public DateTime RolUsuario_FechaMod { get; set; }
        public string RolUsuario_Comentarios { get; set; }

        // Roles
        public int Rol_ID { get; set; }//Se uesa en Roles Usuarios
        public string Rol_Nombre { get; set; }
        public string Rol_Clave { get; set; }
        public bool Rol_Vigente { get; set; }
        public DateTime Rol_FechaMod { get; set; }
        public string Rol_Comentario { get; set; }

        // Mercados
        public int Mercado_ID_M { get; set; }
        public string Mercado_Nombre { get; set; }
        public bool Mercado_Vigente { get; set; }
        public DateTime Mercado_FechaMod { get; set; }
        public string Mercado_Comentario { get; set; }

        // Propiedades para las notificaciones
        public List<Notificacion> Notificaciones { get; set; } = new List<Notificacion>();


    }


}
