namespace DoAnWeb.Models
{
    public class ExaminationService
    {
        public int Id { get; set; }

        public int Quantity { get; set; } = 1;

        public string? Result { get; set; } // Kết quả của dịch vụ khám

        public DateTime? CompletedAt { get; set; }

        public int MedicalServiceId { get; set; }
        public MedicalService MedicalService { get; set; } = null!;

        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; } = null!;
    }
}