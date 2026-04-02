using System.ComponentModel.DataAnnotations;

namespace DoAnWeb.Areas.Patient.ViewModels
{
    public class AppointmentCreateViewModel
    {
        [Required]
        public int DoctorId { get; set; }

        [Required]
        public DateTime ScheduledDate { get; set; }

        public string? ReasonForVisit { get; set; }

        public List<DoctorSelectViewModel> Doctors { get; set; } = new();

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

    public class DoctorSelectViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Specialty { get; set; }
    }
}