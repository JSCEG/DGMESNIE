namespace NSIE.Models
{

	public class MonitoreoViewModel
	{
		public int TotalAccesos { get; set; }
		public List<AccesoDetalle> DetallesAcceso { get; set; }
		public List<TipoAccesoTotal> TotalAccesosPorTipo { get; set; }
		public List<UltimoAccesoUsuario> UltimoAccesoPorUsuario { get; set; }

		// Constructor
		public MonitoreoViewModel()
		{
			DetallesAcceso = new List<AccesoDetalle>();
			TotalAccesosPorTipo = new List<TipoAccesoTotal>();
			UltimoAccesoPorUsuario = new List<UltimoAccesoUsuario>();
		}
	}


}
