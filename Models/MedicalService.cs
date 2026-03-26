namespace DoAnWeb.Models;

public class MedicalService
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public List<ExaminationService> ExaminationServices { get; set; } = new List<ExaminationService>();
}

public class ExaminationService
{
    public int Id { get; set; }
    public int Quantity { get; set; } = 1;
    public string? Result { get; set; } //Kết quả của dịch vụ khám
    public DateTime? CompletedAt { get; set; }

    public int MedicalServiceId { get; set; }
    public MedicalService MedicalService { get; set; } = null!;
    public int AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;
}