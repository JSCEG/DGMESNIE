namespace NSIE.Models
{
	public class UsuarioActivo
	{
		public string Nombre { get; set; }
		public string Cargo { get; set; }
		public string Area { get; set; }
		public bool EsActivo { get; set; }
		public DateTime UltimaHoraActividad { get; set; }  // Nueva propiedad
		public DateTime UltimaActividad { get; set; }  // Nueva propiedad
	}

}
