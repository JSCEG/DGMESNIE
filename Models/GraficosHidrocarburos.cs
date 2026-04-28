namespace NSIE.Models
{
	public class GraficosHidrocarburos
	{

		public IEnumerable<GraficosHidrocarburos> ObtieneImpVolumemPorAño_PvsGN;
		public IEnumerable<GraficosHidrocarburos> ObtieneImpVolumemPorMes_PvsGN;
		public IEnumerable<GraficosHidrocarburos> ObtieneImpVolumen_PvsGN;


		//ObtieneImpVolumen_PvsGN
		public int Año { get; set; }
		public string Mes { get; set; }
		public float Petrolíferos_y_gas_licuado { get; set; }
		public float Gas_licuado { get; set; }
		public float Propano { get; set; }
		public float Gasolinas_3 { get; set; }
		public float Naftas { get; set; }
		public float Diesel { get; set; }
		public float Combustóleo { get; set; }
		public float Otros { get; set; }
		public float Gas_natural_seco { get; set; }


		//ObtieneImpVolumemPorAño_PvsGN y 	//ObtieneImpVolumemPorMes_PvsGN


		public decimal Petrolíferos_y_gas_licuado_Total { get; set; }
		public decimal Gas_licuado_Total { get; set; }
		public decimal Propano_Total { get; set; }
		public decimal Gasolinas_3_Total { get; set; }
		public decimal Naftas_Total { get; set; }
		public decimal Diesel_Total { get; set; }
		public decimal Combustóleo_Total { get; set; }
		public decimal Otros_Total { get; set; }
		public decimal Gas_natural_seco_Total { get; set; }




	}
}
