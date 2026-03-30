namespace DoAnWeb.Models
{
    public enum ExaminationStatus
    {
        Waiting,      // Đang chờ khám
        InProgress,   // Đang khám
        Completed,    // Đã khám xong
        Cancelled     // Đã hủy
    }

    public class MedicalExamination
    {
        public int Id { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public string? Symptoms { get; set; } // Triệu chứng

        public string? Diagnosis { get; set; } // Chẩn đoán

        public string? DoctorAvoid { get; set; } // Lời dặn của bác sĩ

        public ExaminationStatus Status { get; set; } = ExaminationStatus.Waiting;

        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; } = null!;

        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;

        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;

        public List<Prescription> Prescriptions { get; set; } = new();
    }
}