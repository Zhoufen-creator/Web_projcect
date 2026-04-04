namespace DoAnWeb.Services
{
    public class PhoBertApiHealthResult
    {
        public bool Enabled { get; set; }
        public string Url { get; set; } = string.Empty;
        public bool IsHealthy { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; }
    }
}
