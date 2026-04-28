using System.ComponentModel.DataAnnotations;
namespace NSIE.Models
{
	public class Encuesta
	{
		public int Id { get; set; }
		[Required(ErrorMessage = "El campo {0} es obligatorio.")]
		[RegularExpression(@"^\S.*\S$", ErrorMessage = "El campo {0} no puede estar vacío, iniciar o terminar solo espacios en blanco y tampoco ser de un solo dígito.")]
		[EmailAddress]
		public string Correo { get; set; }

		[Required(ErrorMessage = "El campo {0} es obligatorio.")]
		[RegularExpression(@"^\S.*\S$", ErrorMessage = "El campo {0} no puede comenzar, iniciar o terminar solo espacios en blanco y tampoco ser de un solo dígito.")]
		public string Nombre { get; set; }

		[Required(ErrorMessage = "El campo {0} es obligatorio.")]
		public bool EncontroInformacion { get; set; }
		[Required(ErrorMessage = "El campo {0} es obligatorio.")] public bool FueUtil { get; set; }

		[Required(ErrorMessage = "La información buscada es un campo es obligatorio.")]
		[RegularExpression(@"^\S.*\S$", ErrorMessage = "El campo información buscada no puede estar vacío, iniciar o terminar solo espacios en blanco y tampoco ser de un solo dígito.")]
		public string InformacionBuscada { get; set; }
		[Required(ErrorMessage = "El campo {0} es obligatorio.")]
		public string CalificacionExperiencia { get; set; }
		[Required(ErrorMessage = "El campo {0} es obligatorio.")]
		public bool AgregarComentario { get; set; }
		//[Required(ErrorMessage = "El comentario adicional es un campo es obligatorio.")]
		[RegularExpression(@"^\S.*\S$", ErrorMessage = "El campo comentario adicional no puede estar vacío, contener, iniciar o terminar solo espacios en blanco y tampoco ser de un solo dígito.")]
		//[RegularExpression(@"\S", ErrorMessage = "El comentario adicional no puede estar vacía o contener solo espacios en blanco.")]
		public string ComentarioAdicional { get; set; }
	}


}
