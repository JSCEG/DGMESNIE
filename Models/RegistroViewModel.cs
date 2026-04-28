using System.ComponentModel.DataAnnotations;

namespace NSIE.Models
{
    public class RegistroViewModel
    {
        [Required(ErrorMessage = "El campo de {0} es obligatorio")]
        [EmailAddress(ErrorMessage = "El campo debe ser un correo electrónico válido")]
        public string Usuario { get; set; }

        [Required(ErrorMessage = "El campo de {0} es obligatorio")]
        [DataType(DataType.Password)]
        public string Contraseña { get; set; }
    }
}
