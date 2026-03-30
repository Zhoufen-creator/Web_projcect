using System.ComponentModel.DataAnnotations;
using DoAnWeb.Models;

namespace DoAnWeb.Areas.Employee.ViewModels
{
    public class AppointmentAssignVM
    {
        public int Id { get; set; }

        public string PatientName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn bác sĩ")]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thời gian khám")]
        public DateTime ScheduledDate { get; set; }

        public string? ReasonForVisit { get; set; }

        [Required]
        public AppointmentStatus Status { get; set; }
    }
}