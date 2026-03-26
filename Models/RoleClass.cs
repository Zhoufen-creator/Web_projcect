

namespace DoAnWeb.Models;

public class Patient
{
    public int Id { get; set; }
    public string? BloodType { get; set; }
    public string? Height { get; set; }
    public string? Weight { get; set; }
    public string? HealthInsuranceNumber { get; set; } //Mã bảo hiểm y tế
    public string? MedicalHistory { get; set; } //Tiền sử bệnh
    public string? Allergies { get; set; } //Dị ứng

     public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public List<Appointment> Appointments { get; set; } = new List<Appointment>();
}

public class Doctor
{
    public int Id { get; set; }
    public string? Specialty { get; set; } //Chuyên khoa
    public string? LicenseNumber { get; set; } //Số giấy phép hành nghề
    public string ? Qualifications { get; set; } //Trình độ chuyên môn

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public List<Appointment> Appointments { get; set; } = new List<Appointment>();
    
}

public class Employee
{
    public int Id { get; set; }
    public string? Position { get; set; } //Chức vụ
    public string? Department { get; set; } //Phòng ban

    public ApplicationUser User { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
}