namespace NSIE.Models
{
    public class ManualSharePointImportViewModel
    {
        public HeaderViewModel Header { get; set; }
        public string StatusMessage { get; set; }
        public bool IsSuccess { get; set; }
        public string FileName { get; set; }
        public string SourceType { get; set; }
        public string WorksheetName { get; set; }
        public int TotalRows { get; set; }
        public int PreviewRows { get; set; }
        public List<string> Columns { get; set; } = new();
        public List<List<string>> Rows { get; set; } = new();
        public List<string> Notes { get; set; } = new();
    }
}