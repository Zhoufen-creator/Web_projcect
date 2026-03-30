using System.ComponentModel.DataAnnotations;

namespace DoAnWeb.Areas.Patient.ViewModels
{
    public class PatientProfileViewModel
    {
        public int PatientId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }

        public string? BloodType { get; set; }
        public double? Height { get; set; }
        public double? Weight { get; set; }
        public string? HealthInsuranceNumber { get; set; }
        public string? MedicalHistory { get; set; }
        public string? Allergies { get; set; }
    }
}