namespace NSIE.Models
{
    public class HeaderViewModel
    {
        public string Title { get; set; }
        public string IconPath { get; set; }
        public string Description { get; set; }
        public string Section { get; set; }
        public string ModuleInfo { get; set; }
        public string LegalDescription { get; set; }
        public List<FundamentoLegal> Fundamentos { get; set; }
    }

    public class FundamentoLegal
    {
        public string Reference { get; set; }
        public string Description { get; set; }
    }
}