namespace NSIE.Models
{
	public class ModuloSNIER
	{
		public int ModuloId { get; set; }
		public int SeccionId { get; set; }
		public string Title { get; set; }
		public string FundamentoLegalModulo { get; set; }
		public string Roles { get; set; }
		public string NombresRoles { get; set; }
		public string Perfiles { get; set; }
		public string Etapa { get; set; }
		public string JustificacionOrden { get; set; }
		public string AyudaContextual { get; set; }
		public string Controller { get; set; }
		public string Action { get; set; }
		public string Desc { get; set; }
		public string Img { get; set; }
		public string Btn { get; set; }
		public string ElementosUI { get; set; }
		public string AyudaVista { get; set; }
		public int Orden { get; set; }
		public bool ModuloActivo { get; set; }
		public bool EsExterno { get; set; } // nueva propiedad	


		// ✅ NUEVO: Soporte a vistas hijas
		public List<VistaSNIER> Vistas { get; set; } = new List<VistaSNIER>();
	}

}