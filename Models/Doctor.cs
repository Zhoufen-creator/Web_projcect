using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnWeb.Models;

public class Doctor
{
    public int Id { get; set; }

    public int SpecialtyId { get; set; } // Chuyên khoa
    [ForeignKey("SpecialtyId")]
    public Specialty Specialty { get; set; } = null!;
    public string? LicenseNumber { get; set; } // Số chứng chỉ hành nghề
    public string? Qualifications { get; set; } // Trình độ

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey("UserId")]
    public ApplicationUser? User { get; set; }

    public List<DoctorSchedule> DoctorSchedules { get; set; } = new();
    public List<Appointment> Appointments { get; set; } = new();
    public List<MedicalExamination> MedicalExaminations { get; set; } = new();
}