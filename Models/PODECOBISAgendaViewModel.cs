namespace NSIE.Models
{
    public class PODECOBISAgendaViewModel
    {
        public HeaderViewModel Header { get; set; }
        public List<PODECOBISAgendaItem> Registros { get; set; } = new List<PODECOBISAgendaItem>();
        public string Busqueda { get; set; }
    }
}