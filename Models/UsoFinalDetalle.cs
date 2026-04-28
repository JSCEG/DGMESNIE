namespace NSIE.Models
{
	public class UsoFinalDetalle
	{
		public int UsoFinalID { get; set; }
		public string UsoFinal_Nombre { get; set; }
		public int ConsumoID { get; set; }
		public string Consumo_Nombre { get; set; }
		public int Año { get; set; }
		public decimal ConsumoUF_Valor { get; set; }

		// Constructor sin parámetros
		public UsoFinalDetalle()
		{
		}

		// Constructor con parámetros, por si se ocupa
		public UsoFinalDetalle(int usoFinalID, string usoFinalNombre, int consumoID, string consumoNombre, int año, decimal consumoUFValor)
		{
			UsoFinalID = usoFinalID;
			UsoFinal_Nombre = usoFinalNombre;
			ConsumoID = consumoID;
			Consumo_Nombre = consumoNombre;
			Año = año;
			ConsumoUF_Valor = consumoUFValor;
		}
	}


}
