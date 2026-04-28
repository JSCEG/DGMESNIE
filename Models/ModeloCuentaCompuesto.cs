using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace NSIE.Models
{

    public class ModeloCuentaCompuesto
    {
        public Cuenta_UsuarioViewModel CuentaUsuario { get; set; }
        public Cuenta_MonitoreoViewModel CuentaMonitoreo { get; set; }
    }

    public class Cuenta_UsuarioViewModel
    {
        public int IdUsuario { get; set; }
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@cre\.gob\.mx$", ErrorMessage = "El correo debe terminar con @cre.gob.mx")]
        public string Correo { get; set; }
        public string Clave { get; set; }

        [NoNumbersOrAtSymbol(ErrorMessage = "El nombre no debe contener números ni el símbolo @.")]
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
        public int Rol_ID { get; set; }
        public int Rol_Id_RU { get; set; }
        public int Mercado_ID { get; set; }
        public bool RolUsuario_Vigente { get; set; }
        public string RolUsuario_QuienRegistro { get; set; }
        public DateTime RolUsuario_FechaMod { get; set; }
        public string RolUsuario_Comentarios { get; set; }

        // Roles

        public int Rol_Id_R { get; set; }
        [Required(ErrorMessage = "El Rol es requerido.")]
        public string Rol_Nombre { get; set; }
        public bool Rol_Vigente { get; set; }
        public DateTime Rol_FechaMod { get; set; }
        public string Rol_Comentario { get; set; }

        // Mercados
        public int Mercado_ID_M { get; set; }
        public string Mercado_Nombre { get; set; }
        public bool Mercado_Vigente { get; set; }
        public DateTime Mercado_FechaMod { get; set; }
        public string Mercado_Comentario { get; set; }


    }
    public class Cuenta_MonitoreoViewModel
    {
        //SECCIÓN MONITOREO
        public int TotalAccesos { get; set; }
        public List<AccesoDetalle> DetallesAcceso { get; set; }
        public List<TipoAccesoTotal> TotalAccesosPorTipo { get; set; }
        public List<UltimoAccesoUsuario> UltimoAccesoPorUsuario { get; set; }

        // Constructor
        public Cuenta_MonitoreoViewModel()
        {
            DetallesAcceso = new List<AccesoDetalle>();
            TotalAccesosPorTipo = new List<TipoAccesoTotal>();
            UltimoAccesoPorUsuario = new List<UltimoAccesoUsuario>();
        }
    }

}