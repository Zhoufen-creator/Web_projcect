namespace DoAnWeb.Services
{
    public class AppointmentEstimateResult
    {
        public double AverageExaminationMinutes { get; set; }
        public int PatientsAheadCount { get; set; }
        public int EstimatedWaitMinutes { get; set; }
        public DateTime EstimatedStartTime { get; set; }
        public string ArrivalSuggestion { get; set; } = string.Empty;
    }
}