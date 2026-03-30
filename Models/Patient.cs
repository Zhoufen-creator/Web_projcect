using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnWeb.Models;

public class Patient
{
    public int Id { get; set; }

    public string? BloodType { get; set; } // Nhóm máu
    public double? Height { get; set; }    // Chiều cao
    public double? Weight { get; set; }    // Cân nặng
    public string? HealthInsuranceNumber { get; set; } // Số BHYT
    public string? MedicalHistory { get; set; } // Tiền sử bệnh
    public string? Allergies { get; set; } // Dị ứng

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; } = null!;

    public List<Appointment> Appointments { get; set; } = new();
    public List<MedicalExamination> MedicalExaminations { get; set; } = new();
}