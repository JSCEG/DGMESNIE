namespace NSIE.Models
{
    public class TarifaMediaInflacion
    {
        public int Id { get; set; }
        public int Año { get; set; }
        public string Mes { get; set; }
        public DateTime Fecha { get; set; }
        public decimal? INPC { get; set; }
        public decimal? VarAcumINPC { get; set; }
        public decimal? TM_Nacional_CAVEN { get; set; }
        public decimal? TM_Nacional_CRE { get; set; }
        public decimal? VarAcumTM_CFE_SSB { get; set; }
        public decimal? VarAcumTM_CRE { get; set; }
    }


}
