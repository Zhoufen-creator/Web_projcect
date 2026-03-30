namespace DoAnWeb.Areas.Patient.ViewModels
{
    public class MedicalHistoryViewModel
    {
        public int ExaminationId { get; set; }
        public DateTime? ExaminationDate { get; set; }

        public string DoctorName { get; set; } = string.Empty;
        public string? Specialty { get; set; }

        public string? Symptoms { get; set; }
        public string? Diagnosis { get; set; }
        public string? DoctorAvoid { get; set; }
        public string? Status { get; set; }

        public List<PrescriptionItemViewModel> Prescriptions { get; set; } = new();
        public List<ServiceItemViewModel> Services { get; set; } = new();
    }

    public class PrescriptionItemViewModel
    {
        public string MedicineName { get; set; } = string.Empty;
        public string? Dosage { get; set; }
        public int Quantity { get; set; }
    }

    public class ServiceItemViewModel
    {
        public string ServiceName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string? Result { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}