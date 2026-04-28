namespace NSIE.Models
{
	public class VistaSNIER
	{
		//Para el SP
		public int VistaId { get; set; }
		public string VistaTitle { get; set; }
		public string VistaController { get; set; }
		public string VistaAction { get; set; }
		public bool EsExterno { get; set; }
		public int VistaOrden { get; set; }
		public bool VistaActivo { get; set; }

		//Relacion con modulos
		public int ModuloId { get; set; }

		//Para CRUD desde el Controlador

		public string Titulo { get; set; }
		public string Roles { get; set; }
		public string Perfiles { get; set; }
		public string Controller { get; set; }
		public string Action { get; set; }

		// ← aquí faltaban estas dos propiedades:
		public int Orden { get; set; }
		public bool Activa { get; set; }
	}

}
