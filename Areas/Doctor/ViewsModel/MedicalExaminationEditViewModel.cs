using DoAnWeb.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace DoAnWeb.Areas.Doctor.ViewModels
{
    public class MedicalExaminationEditViewModel
    {
        public int? Id { get; set; }

        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }

        [Display(Name = "Triệu chứng")]
        public string? Symptoms { get; set; }

        [Display(Name = "Chẩn đoán")]
        public string? Diagnosis { get; set; }

        [Display(Name = "Lời dặn của bác sĩ")]
        public string? DoctorAvoid { get; set; }

        public ExaminationStatus Status { get; set; } = ExaminationStatus.InProgress;

        // Thuốc
        public int? SelectedMedicineId { get; set; }
        public int MedicineQuantity { get; set; } = 1;
        public string? MedicineDosage { get; set; }

        // Dịch vụ
        public int? SelectedMedicalServiceId { get; set; }
        public int ServiceQuantity { get; set; } = 1;
        public string? ServiceResult { get; set; }

        public List<SelectListItem> Medicines { get; set; } = new();
        public List<SelectListItem> Services { get; set; } = new();
    }
}