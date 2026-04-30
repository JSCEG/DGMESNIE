using System.ComponentModel.DataAnnotations;

namespace NSIE.Models
{
    public class PODECOBISAgendaItem
    {
        public int AgendaId { get; set; }

        [Display(Name = "No.")]
        [Range(1, 9999, ErrorMessage = "El número debe ser mayor a cero.")]
        public int Numero { get; set; }

        [Required(ErrorMessage = "El polo es obligatorio.")]
        [StringLength(200)]
        public string Polo { get; set; }

        [Required(ErrorMessage = "El nombre oficial es obligatorio.")]
        [Display(Name = "Nombre oficial en declaratoria")]
        [StringLength(400)]
        public string NombreOficialDeclaratoria { get; set; }

        [Display(Name = "Organismo de seguimiento")]
        [StringLength(200)]
        public string OrganismoSeguimiento { get; set; }

        [Display(Name = "Nombre de contacto")]
        [StringLength(250)]
        public string NombreContacto { get; set; }

        [StringLength(150)]
        public string Cargo { get; set; }

        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [StringLength(250)]
        public string Correo { get; set; }

        [Display(Name = "Número telefónico")]
        [StringLength(120)]
        public string NumeroTelefonico { get; set; }

        public bool Activo { get; set; } = true;
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaActualizacion { get; set; }
    }
}