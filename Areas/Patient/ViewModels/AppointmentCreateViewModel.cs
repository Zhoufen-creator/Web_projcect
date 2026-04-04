using System.ComponentModel.DataAnnotations;

namespace DoAnWeb.Areas.Patient.ViewModels
{
    public class AppointmentCreateViewModel
    {
        public int DoctorId { get; set; }

        [Required]
        public DateTime ScheduledDate { get; set; }

        public string? ReasonForVisit { get; set; }

        public string? PredictedSpecialty { get; set; }
        public int PredictionScore { get; set; }
        public List<string> MatchedKeywords { get; set; } = new();
        public string? PredictionMessage { get; set; }

        // SỬA: thông tin bác sĩ được hệ thống gợi ý
        public int? SuggestedDoctorId { get; set; }
        public string? SuggestedDoctorName { get; set; }
        public string? SuggestedDoctorMessage { get; set; }
        public int SuggestedRemainingSlots { get; set; }
    }
}
