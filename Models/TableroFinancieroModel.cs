namespace NSIE.Models
{
	public class TableroFinancieroModel
	{
		public int ciclo { get; set; }
		public int idRamo { get; set; }
		public string descRamo { get; set; }
		public int idUR { get; set; }
		public string descUR { get; set; }
		public string gpoFuncional { get; set; }
		public string descGpoFuncional { get; set; }
		public int idFuncion { get; set; }
		public string descFuncion { get; set; }
		public int idSubfuncion { get; set; }
		public string descSubfuncion { get; set; }
		public string idAi { get; set; }
		public string descAi { get; set; }
		public string idModalidad { get; set; }
		public string descModalidad { get; set; }
		public string idPP { get; set; }
		public string descPP { get; set; }
		public int idCapitulo { get; set; }
		public string descCapitulo { get; set; }
		public int idConcepto { get; set; }
		public string descConcepto { get; set; }
		public int idPartidaGenerica { get; set; }
		public string descPartidaGenerica { get; set; }
		public int idPartidaEspecifica { get; set; }
		public string descPartidaEspecifica { get; set; }
		public int idTipoGasto { get; set; }
		public string descTipoGasto { get; set; }
		public string idFf { get; set; }
		public string descFf { get; set; }
		public int idEntidadFederativa { get; set; }
		public string entidadFederativa { get; set; }
		public string idClaveCartera { get; set; }
		public decimal montoAprobado { get; set; }
		public decimal montoModificado { get; set; }
		public decimal montoAprobadoMensual { get; set; }
		public decimal montoModificadoMensual { get; set; }
		public decimal montoPagado { get; set; }
		public string tipoDeGasto { get; set; }
		public string status { get; set; }
		public DateTime insertDate { get; set; }
		public DateTime updateDate { get; set; }
		public int idSeq { get; set; }
	}

}
