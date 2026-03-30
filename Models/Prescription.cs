namespace DoAnWeb.Models
{
    public class Prescription
    {
        public int Id { get; set; }

        public string? Dosage { get; set; } // Liều lượng

        public int Quantity { get; set; } // Số lượng

        public int MedicineId { get; set; }
        public Medicine Medicine { get; set; } = null!;

        public int MedicalExaminationId { get; set; }
        public MedicalExamination MedicalExamination { get; set; } = null!;
    }
}