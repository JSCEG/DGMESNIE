using Microsoft.AspNetCore.Mvc;
using NSIE.Validaciones;
using System.ComponentModel.DataAnnotations;

namespace NSIE.Models
{
    public class Indicador2_Clase
    {
        //Datos para Obtener la calificación Final

        [Required(ErrorMessage = "El Campo {0} es obligatorio, no puede estar vacío,ni ser 0 ni mayor a 1")]
        public float Umbral { get; set; }

        //Prueba del Insert en la tabla dbo.Usuarios
        public int id { get; set; }
        [Remote(action: "VerificarExisteUsuario", controller: "Indicadores")]
        public string Usuario { get; set; }
        [Required(ErrorMessage = "El Campo {0} es obligatorio, no puede estar vacío")]
        [EmailAddress(ErrorMessage = "El Campo debe ser un correo elecrtrónico válido")]
        public string Email { get; set; }
        public string EmailNormalizado { get; set; }
        public string PasswordHash { get; set; }


        //Lo Referente a el Indicador 2

        //[Required (ErrorMessage ="El Campo {0} es obligatorio, no puede estar vacío")]
        //[StringLength(maximumLength: 10, MinimumLength =4,  ErrorMessage ="La logitid de {0} debe de ser entre {2} y {1} caracteres") ]
        //[Display(Name ="Entidad Federativa")]
        //[PrimeraLetraMayuscula(ErrorMessage = "¡La primera leta del campo {0} debe ser mayúscula!")]
        public string EF_ID { get; set; }
        public string EF_Nombre { get; set; }
        public float Año1 { get; set; }
        public float Año2 { get; set; }
        public float Año3 { get; set; }
        public float Año4 { get; set; }
        public float Año5 { get; set; }
        public float Año6 { get; set; }
        public float Año7 { get; set; }
        public float Año8 { get; set; }
        public float Año9 { get; set; }
        public float Año10 { get; set; }
        public float Año11 { get; set; }
        public float Año12 { get; set; }
        public float Año13 { get; set; }
        public float Año14 { get; set; }
        public float Año15 { get; set; }
        public float Año16 { get; set; }


        //Colores
        public string C_Año1 { get; set; }
        public string C_Año2 { get; set; }
        public string C_Año3 { get; set; }
        public string C_Año4 { get; set; }
        public string C_Año5 { get; set; }
        public string C_Año6 { get; set; }
        public string C_Año7 { get; set; }
        public string C_Año8 { get; set; }
        public string C_Año9 { get; set; }
        public string C_Año10 { get; set; }
        public string C_Año11 { get; set; }
        public string C_Año12 { get; set; }
        public string C_Año13 { get; set; }
        public string C_Año14 { get; set; }
        public string C_Año15 { get; set; }
        public string C_Año16 { get; set; }
        public string C { get; set; }

        //Se pueden declarar mas valores

        /*Ejemplos de Anotations*/
        // [Required(ErrorMessage = "El Campo {0} es obligatorio, no puede estar vacío")]
        //   [EmailAddress(ErrorMessage = "El Campo debe ser un correo elecrtrónico válido")]
        //  public string Email { get; set; }
        //[Required(ErrorMessage = "El Campo {0} es obligatorio, no puede estar vacío")]
        //[Range(minimum: 18, maximum: 40, ErrorMessage = "El valor debe estar entre  {1} y {2}")]
        //public int Edad { get; set; }

        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    if (ID_EF != null && ID_EF.Length > 0) 
        //    {
        //    var primeraletra = ID_EF[0].ToString();

        //        if(primeraletra != primeraletra.ToUpper())
        //        {
        //            yield return new ValidationResult("La primera letra debe ser mayúscula", new[] { nameof(ID_EF) });
        //        }
        //    }
        //}

        //[CreditCard(ErrorMessage = "La tarjeta de Credito no es válida")]
        //public int TarjetaCredito { get; set; }
        //[Required(ErrorMessage = "El Campo {0} es obligatorio, no puede estar vacío")]
        //[Url(ErrorMessage = "El Campo {0} debe ser una URL válida")]
        //public string URL { get; set; }



    }
}
