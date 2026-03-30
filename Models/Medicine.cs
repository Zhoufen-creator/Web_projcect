namespace DoAnWeb.Models
{
    public class Medicine
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public List<Prescription> Prescriptions { get; set; } = new();
    }
}