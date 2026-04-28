using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace NSIE.Models
{

    public class EditUserViewModel
    {
        public int IdUsuario { get; set; }
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@energia\.gob\.mx$", ErrorMessage = "El correo debe terminar con @energia.gob.mx")]
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
    ///Validaciones
    public class NoNumbersOrAtSymbolAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                string valueAsString = value.ToString();
                if (System.Text.RegularExpressions.Regex.IsMatch(valueAsString, @"[\d@]"))
                {
                    return new ValidationResult("El nombre no debe contener números ni el símbolo @.");
                }
            }
            return ValidationResult.Success;
        }
    }

}
