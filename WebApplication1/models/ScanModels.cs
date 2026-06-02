namespace WebApplication1.models
{
    public class ScanResponse 
    {
        public scanresultDto scanResult { get; set; } = null!;
        public string Message  { get; set; } = string.Empty;
    }

    public class scanresultDto
    {
        public bool IsClean { get; set; }
        public List<VirusDetails> FoundViruses { get; set; } = new List<VirusDetails>();
    }

    public class VirusDetails
    {
        public string VirusName { get; set; } = string.Empty;
    }
}
