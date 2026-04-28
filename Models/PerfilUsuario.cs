namespace NSIE.Models
{
    public class PerfilUsuario
    {
        //Tabla de Usuarios: [dbo].[USUARIO]
        public string IdUsuario { get; set; }
        public string Correo { get; set; }
        public string Clave { get; set; }
        public string Nombre { get; set; }
        public string Unidad_de_Adscripcion { get; set; }
        public string Cargo { get; set; }
        public string SesionActiva { get; set; }
        public string UltimaActualizacion { get; set; }
        public string RFC { get; set; }
        public string Vigente { get; set; }
        public string ClaveEmpleado { get; set; }
        public string HoraInicioSesion { get; set; }

        //Tabla "Roles de Usuarios":[dbo].[Roles_Usuarios]
        public string Rol { get; set; }
        public string Mercado_ID { get; set; }
        public string RolUsuario_Vigente { get; set; }
        public string RolUsuario_QuienRegistro { get; set; }
        public string RolUsuario_FechaMod { get; set; }
        public string RolUsuario_Comentarios { get; set; }

        //Tabla "Roles":[dbo].[Roles]
        public string Rol_ID { get; set; }
        public string Rol_Nombre { get; set; }
        public string Rol_Vigente { get; set; }
        public string Rol_FechaMod { get; set; }
        public string Rol_Comentario { get; set; }


        //Tabla "Mercados":[dbo].[Mercados]


        // public string Mercado_ID { get ;set;} -->//Tabla "Roles de Usuarios"
        public string Mercado_Nombre { get; set; }
        public string Mercado_Vigente { get; set; }
        public string Mercado_FechaMod { get; set; }
        public string Mercado_Comentario { get; set; }
    }
}
