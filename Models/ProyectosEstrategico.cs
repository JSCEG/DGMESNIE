using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NSIE.Models
{
    public class ProyectoEstrategico
    {
        [Key]
        public int IDProyecto { get; set; }


        [Required(ErrorMessage = "La información buscada es un campo es obligatorio.")]
        [RegularExpression(@"^\S.*\S$", ErrorMessage = "El campo información buscada no puede estar vacío, iniciar o terminar solo espacios en blanco y tampoco ser de un solo dígito.")]
        [StringLength(255)]
        public string NombreProyecto { get; set; }

        [Required(ErrorMessage = "La información buscada es un campo es obligatorio.")]
        [RegularExpression(@"^\S.*\S$", ErrorMessage = "El campo información buscada no puede estar vacío, iniciar o terminar solo espacios en blanco y tampoco ser de un solo dígito.")]
        public string Descripción { get; set; }

        [StringLength(50)]
        public string Mercado { get; set; }

        [Required]
        public int Mercado_ID { get; set; }

        [DataType(DataType.Date)]
        public DateTime FechaIngreso { get; set; } //


        public List<TramiteProyectoEstrategico> Tramites { get; set; } = new List<TramiteProyectoEstrategico>();

        public int CalcularAvance()
        {
            if (Tramites == null || Tramites.Count == 0)
                return 0;

            int totalAvance = 0;
            foreach (var tramite in Tramites)
            {
                totalAvance += tramite.CalcularAvance();
            }

            return totalAvance / Tramites.Count;
        }
    }

    public class TramiteProyectoEstrategico
    {
        [Key]
        public int IDTramite { get; set; }



        [Required]
        public int IDProyecto { get; set; }

        [StringLength(255)]
        [RegularExpression(@"^\S.*\S$", ErrorMessage = "El campo información no puede estar vacío, iniciar o terminar solo espacios en blanco y tampoco ser de un solo dígito.")]
        public string Ubicación { get; set; }

        [StringLength(255)]
        [RegularExpression(@"^\S.*\S$", ErrorMessage = "El campo información no puede estar vacío, iniciar o terminar solo espacios en blanco y tampoco ser de un solo dígito.")]
        public string RazonSocial { get; set; }

        [StringLength(255)]
        [RegularExpression(@"^\S.*\S$", ErrorMessage = "El campo información no puede estar vacío, iniciar o terminar solo espacios en blanco y tampoco ser de un solo dígito.")]
        public string Tramite { get; set; }

        [StringLength(255)]
        [RegularExpression(@"^\S.*\S$", ErrorMessage = "El campo información no puede estar vacío, iniciar o terminar solo espacios en blanco y tampoco ser de un solo dígito.")]
        public string Municipio { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime FechaIngreso { get; set; }

        [StringLength(50)]
        [RegularExpression(@"^\S.*\S$", ErrorMessage = "El campo información no puede estar vacío, iniciar o terminar solo espacios en blanco y tampoco ser de un solo dígito.")]
        public string NumeroFolio { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? FechaRevisionJuridica { get; set; } // Mantener como fecha

        [Required]
        [StringLength(255)]
        [RegularExpression(@"^\S.*\S$", ErrorMessage = "El campo información no puede estar vacío, iniciar o terminar solo espacios en blanco y tampoco ser de un solo dígito.")]
        public string ComentariosRevisionJuridica { get; set; } // Nuevo campo para comentarios

        [Required]
        [StringLength(255)]
        [RegularExpression(@"^\S.*\S$", ErrorMessage = "El campo información no puede estar vacío, iniciar o terminar solo espacios en blanco y tampoco ser de un solo dígito.")]
        public string FechaSesionEstimado { get; set; }

        [Required]
        [RegularExpression(@"^\S.*\S$", ErrorMessage = "El campo información no puede estar vacío, iniciar o terminar solo espacios en blanco y tampoco ser de un solo dígito.")]
        public string ComentarioTramite { get; set; }

        public string ComentarioNuevo { get; set; } // Cambiado a ComentarioNuevo


        [Required]
        [RegularExpression(@"^\S.*\S$", ErrorMessage = "El campo información no puede estar vacío, iniciar o terminar solo espacios en blanco y tampoco ser de un solo dígito.")]
        public string Responsables { get; set; }

        public bool TramiteIngresado { get; set; }
        public bool AnalisisEvaluacion { get; set; }
        public bool AutorizadoParaPleno { get; set; }
        public bool PermisoOtorgado { get; set; }
        // Nueva propiedad
        public string Estatus { get; set; }

        public List<ComentarioProyectoEstrategico> Comentarios { get; set; } = new List<ComentarioProyectoEstrategico>();

        public int CalcularAvance()
        {
            if (PermisoOtorgado)
                return 100;
            if (AutorizadoParaPleno)
                return 85;
            if (AnalisisEvaluacion)
                return 70;
            if (TramiteIngresado)
                return 20;
            return 0;
        }

        // Nueva propiedad
        public string NombreProyecto { get; set; }
    }

    public class ComentarioProyectoEstrategico
    {
        [Key]
        public int IDComentario { get; set; }

        [Required]
        public int IDTramite { get; set; }

        [Required(ErrorMessage = "El comentario es obligatorio.")]
        [StringLength(500, ErrorMessage = "El comentario no puede exceder de 500 caracteres.")]
        [RegularExpression(@"^\S.*\S$", ErrorMessage = "El comentario no puede estar vacío, iniciar o terminar solo con espacios en blanco.")]
        public string ComentarioNuevo { get; set; } // Cambiado a ComentarioNuevo

        [DataType(DataType.DateTime)]
        public DateTime FechaComentario { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        // Nueva propiedad
        public string NombreUsuario { get; set; }
    }

}
