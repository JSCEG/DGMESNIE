namespace NSIE.Models
{
	public class AccesoDetalle
	{
		public long AccesoId { get; set; }
		public string Nombre { get; set; }
		public string TipoAcceso { get; set; }
		public string IP { get; set; }
		public DateTime FechaHoraLocal { get; set; }
		public string UnidadDeAdscripcion { get; set; }
		public string Cargo { get; set; }
	}

}
