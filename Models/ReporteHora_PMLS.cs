namespace NSIE.Models
{
	public class ReporteHora_PMLS
	{
		public string ClaveProcesoMercado { get; set; }
		public string ClaveSistema { get; set; }
		public string NombreZonaCarga { get; set; }
		public DateTime Fecha { get; set; }
		public int IdHora { get; set; } // Nueva propiedad para la hora
		public decimal PrecioMarginalLocalPonderadoZC { get; set; }
		public decimal ComponenteEnergiaPonderadoZC { get; set; }
		public decimal ComponentePerdidaPonderadoZC { get; set; }
		public decimal ComponenteCongestionPonderadoZC { get; set; }
		public int PML_2STDEV_FLOOR { get; set; }
		public int PML_MIN_FLOOR { get; set; }
		public decimal Latitud { get; set; }
		public decimal Longitud { get; set; }
	}

}