using System;
using System.ComponentModel.DataAnnotations;

namespace NSIE.Models
{
    public class ProyectosEstrategicos
    {
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "El campo Sector es obligatorio.")]
        [StringLength(100, ErrorMessage = "El campo Sector no debe exceder los 100 caracteres.")]
        [RegularExpression(@"^\S(.*\S)?$", ErrorMessage = "El campo no debe tener espacios al inicio ni al final y no debe contener múltiples espacios en blanco seguidos.")]
        public string Sector { get; set; }

        [Required(ErrorMessage = "El campo Trámite Solicitado es obligatorio.")]
        [RegularExpression(@"^\S(.*\S)?$", ErrorMessage = "El campo no debe tener espacios al inicio ni al final y no debe contener múltiples espacios en blanco seguidos.")]
        public string TramiteSolicitado { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Ingreso")]
        public DateTime? FechaIngreso { get; set; }

        [Required(ErrorMessage = "El Número de Folio es requerido.")]
        [RegularExpression(@"^\S*$", ErrorMessage = "No se permiten espacios en blanco.")]
        public string NumeroFolio { get; set; }

        public float AvanceActual { get; set; }

        public float PermisoAutorizacionOtorgado { get; set; }
        public float EvaluacionYAnalisis { get; set; }
        public float AutorizadoPleno { get; set; }
        public float TramiteIngresado { get; set; }

        [Required(ErrorMessage = "El campo Sector es obligatorio.")]
        [StringLength(500, ErrorMessage = "El detalle no debe exceder los 1500 caracteres.")]
        [RegularExpression(@"^\S(.*\S)?$", ErrorMessage = "El campo no debe tener espacios al inicio ni al final y no debe contener múltiples espacios en blanco seguidos.")]
        public string Detalles { get; set; }

        [Required(ErrorMessage = "La ubicación es requerida.")]
        public string Ubicacion { get; set; }

        [Range(-90.0, 90.0, ErrorMessage = "Ingrese una latitud válida.")]
        public float Latitud { get; set; }

        [Range(-180.0, 180.0, ErrorMessage = "Ingrese una longitud válida.")]
        public float Longitud { get; set; }

        public DateTime UltimaActualizacion { get; set; }
    }
}


