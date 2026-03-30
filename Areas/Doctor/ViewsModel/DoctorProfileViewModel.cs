using System.ComponentModel.DataAnnotations;

namespace DoAnWeb.Areas.Doctor.ViewModels
{
    public class DoctorProfileViewModel
    {
        public int DoctorId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }

        public string Gender { get; set; } = string.Empty;

        public string? Address { get; set; }

        [Required]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string Specialty { get; set; } = string.Empty;

        public string? LicenseNumber { get; set; }

        public string? Qualifications { get; set; }
    }
}