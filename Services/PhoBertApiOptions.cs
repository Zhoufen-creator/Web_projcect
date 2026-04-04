namespace DoAnWeb.Services
{
    public class PhoBertApiOptions
    {
        public bool Enabled { get; set; }
        public string Url { get; set; } = "http://127.0.0.1:5000";
        public int TimeoutSeconds { get; set; } = 10;
        public int TopK { get; set; } = 3;
    }
}
