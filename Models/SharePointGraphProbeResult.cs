namespace NSIE.Models
{
    public class SharePointGraphProbeResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string SiteId { get; set; }
        public string SiteName { get; set; }
        public string WebUrl { get; set; }
        public string RequestUrl { get; set; }
        public List<SharePointGraphProbeItem> Items { get; set; } = new();
        public string RawError { get; set; }
    }

    public class SharePointGraphProbeItem
    {
        public string Name { get; set; }
        public string WebUrl { get; set; }
        public bool IsFolder { get; set; }
        public string LastModifiedDateTime { get; set; }
    }
}