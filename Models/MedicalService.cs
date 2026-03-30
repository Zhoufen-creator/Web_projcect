namespace DoAnWeb.Models
{
    public class MedicalService
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public List<ExaminationService> ExaminationServices { get; set; } = new();
    }
}