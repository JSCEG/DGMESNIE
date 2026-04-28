namespace NSIE.Models
{
	public class FactorEmisionViewModel
	{
		public List<FactorEmision> FactoresEmision { get; set; }
	}

	public class FactorEmision
	{
		public int Anio { get; set; }
		public double Valor { get; set; }
		public string PdfUrl { get; set; }
	}

}
