namespace DoAnWeb.Services
{
    public class SeasonalStaffingAlert
    {
        public int SpecialtyId { get; set; }
        public string SpecialtyName { get; set; } = string.Empty;
        public int CurrentWeekCases { get; set; }
        public double BaselineCases { get; set; }
        public double AnomalyScore { get; set; }
        public bool IsAnomaly { get; set; }
        public int CurrentDoctors { get; set; }
        public int SuggestedExtraDoctors { get; set; }
        public int MaxPatientsPerWeek { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }
    }
}
