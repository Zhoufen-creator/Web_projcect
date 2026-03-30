namespace DoAnWeb.Models
{
    public class SiteVisit
    {
        public int Id { get; set; }

        public DateTime VisitTime { get; set; } = DateTime.Now;

        public string IPAddress { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
    }
}