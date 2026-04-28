namespace NSIE.Models
{
	public class Credito
	{
		public int CreditoID { get; set; }
		public string Nombre { get; set; }
		public string Cargo { get; set; }
		public string ImagenUrl { get; set; }
		public string Seccion { get; set; }
		public string PaginaWeb { get; set; }
		public string Actividades { get; set; } // Nueva propiedad para actividades
		public string Resena { get; set; } // Nueva propiedad para reseña
	}

}
