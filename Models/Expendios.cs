namespace NSIE.Models
{
	public class Expendios
	{

		public string NumeroPermiso { get; set; }
		public string EF_ID { get; set; }
		public string EF_Nombre { get; set; }
		public string MPO_ID { get; set; }
		public string Numero_de_Expediente { get; set; }
		public string Razón_social { get; set; }
		public string FechaOtorgamiento { get; set; }
		public float Latitud_GEO { get; set; }
		public float Longitud_GEO { get; set; }
		public string Calle_num_ES { get; set; }
		public string Colonia_ES { get; set; }

		public string Codigo_Postal { get; set; } //En table
		public string CodigoPostal { get; set; }//No se

		public string Estatus { get; set; }
		public string Subestatus { get; set; }
		public string RFC { get; set; }
		public DateTime FechaRecepcion { get; set; }
		public string Estatus_instalacion { get; set; }
		public string Causa_suspension { get; set; }
		public string Marca { get; set; }
		public string Tipo_permiso { get; set; }
		public string Inicio_vigencia { get; set; }
		public string Termino_vigencia { get; set; }
		public string Inicio_operaciones { get; set; }
		public string Capacidad_autorizada_barriles { get; set; }
		public string Inversion_estimada { get; set; }
		public string Productos { get; set; }
		public string Comentarios { get; set; }


		//De Otro Proceso
		public string UltimaImagenComercialReportada { get; set; }

		// Para el Tablero de Electricidad...
		public string Mercado { get; set; }
		// Agrega una lista para los datos del dashboard
		public List<DatosDashboard_PE> DatosDashboard { get; set; }

	}

}
