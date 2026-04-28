using System;
using System.ComponentModel.DataAnnotations;
namespace NSIE.Models
{
    public class BusquedaPermisoViewModel
    {
        [Required(ErrorMessage = "El campo de búsqueda es obligatorio.")]
        [StringLength(100, ErrorMessage = "El campo de búsqueda no puede exceder los 100 caracteres.")]
        [RegularExpression(@"^\S*$", ErrorMessage = "No se permiten espacios en blanco.")]
        public string Busqueda { get; set; }

        public IEnumerable<PermisoVehicular> Permisos { get; set; }
    }

}
