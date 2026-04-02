namespace DoAnWeb.Services
{
    public class SpecialtyLoadInsight
    {
        public string Specialty { get; set; } = string.Empty;
        public int CurrentWeekCount { get; set; }
        public double AverageWeeklyCount { get; set; }
        public double IncreaseRatio { get; set; }
        public bool IsAbnormal { get; set; }
        public string Recommendation { get; set; } = string.Empty;
    }
}