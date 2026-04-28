namespace NSIE.Models
{
    public class DatosDashboard_PE
    {
        public int Periodo { get; set; }
        public int? Trimestre { get; set; } // Trimestre como int (nullable para datos anuales)
        public string NumeroPermiso { get; set; }
        public double GeneracionBrutaOImportacionGWh { get; set; }
        public double GeneracionNetaGWh { get; set; }
        public double FactorPlantaReportado { get; set; }
        public double FactorPlantaCalculado { get; set; }
        public double EmisionesCO2 { get; set; }
        public double ConsumoAgua { get; set; }
    }



}
