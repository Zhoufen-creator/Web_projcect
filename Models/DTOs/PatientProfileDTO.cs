namespace DoAnWeb.DTOs;

public class PatientProfileDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set ;} = string.Empty;
    public string? BloodType { get; set; }
    public string? Height { get; set; }
    public string? Weight { get; set; }
}