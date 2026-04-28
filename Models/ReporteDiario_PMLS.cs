namespace NSIE.Models
{
	public class ReporteDIario_PMLS
	{
		public string ClaveProcesoMercado { get; set; }
		public string ClaveSistema { get; set; }
		public string NombreZonaCarga { get; set; }
		public DateTime Fecha { get; set; }
		public int Hora { get; set; } // Nueva propiedad para la hora
		public decimal PrecioZonal_AVG { get; set; }
		public decimal CompEnergia_AVG { get; set; }
		public decimal CompPerdida_AVG { get; set; }
		public decimal CompCongestion_AVG { get; set; }
		public decimal? LimInf { get; set; }
		public int Step { get; set; }
		public int LimSup { get; set; }
		public decimal Latitud { get; set; }
		public decimal Longitud { get; set; }
	}

}
