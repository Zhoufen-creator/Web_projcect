namespace DoAnWeb.Models;

public class Medicine
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public List<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}

public class Prescription
{
    public int Id { get; set; }
    public string? Dosage { get; set; } = string.Empty; //Liều lượng
    public int Quantity { get; set; } //Số lượng

    public int MedicineId { get; set; }
    public Medicine Medicine { get; set; } = null!;
    public int MedicalExaminationId { get; set; }
    public MedicalExamination MedicalExamination { get; set; } = null!;
}