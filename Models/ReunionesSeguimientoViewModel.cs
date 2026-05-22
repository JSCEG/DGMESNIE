namespace NSIE.Models
{
    public class ReunionesSeguimientoViewModel
    {
        public HeaderViewModel Header { get; set; }
        public List<ReunionesSeguimientoItem> Asuntos { get; set; } = new();
        public int TotalAsuntos { get; set; }
        public int TotalAtendidos { get; set; }
        public int TotalPendientes { get; set; }
        public int TotalPorVencer { get; set; }
        public int TotalVencidos { get; set; }
        public int TotalPendientesEnvioMonica { get; set; }
        public List<string> Notes { get; set; } = new();
    }
}