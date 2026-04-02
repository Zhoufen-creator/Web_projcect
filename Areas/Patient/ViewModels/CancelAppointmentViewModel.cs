using System.ComponentModel.DataAnnotations;

namespace DoAnWeb.Areas.Patient.ViewModels
{
    public class CancelAppointmentViewModel
    {
        public int Id { get; set; }

        public DateTime ScheduledDate { get; set; }

        public string DoctorName { get; set; } = string.Empty;

        public string? Specialty { get; set; }

        public string? ReasonForVisit { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lý do hủy lịch.")]
        public string CancellationReason { get; set; } = string.Empty;
    }
}