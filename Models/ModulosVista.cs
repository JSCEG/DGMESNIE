namespace NSIE.Models
{
	public class ModulosVista
	{
		public int VistaId { get; set; } // Mapea a v.Id as VistaId
		public int ModuloId { get; set; }
		public string Titulo { get; set; } = "";
		public string Controller { get; set; } = "";
		public string Action { get; set; } = "";
		public string Roles { get; set; } = "";
		public string Perfiles { get; set; } = "";
		public int Orden { get; set; } = 1;
		public bool Activa { get; set; } = true; // Mapea a v.Activo as Activa
		public bool EsExterno { get; set; } = false;

		// Propiedades de navegación
		public string? ModuloTitle { get; set; }
		public string? SeccionTitle { get; set; }
	}
}
