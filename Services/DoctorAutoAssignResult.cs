namespace DoAnWeb.Services
{
    public class DoctorAutoAssignResult
    {
        public int? DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public int CurrentPatientCount { get; set; }
        public int RemainingSlots { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsAssigned { get; set; }
    }
}