using System.ComponentModel.DataAnnotations;

namespace DoAnWeb.Areas.Patient.ViewModels
{
    public class AppointmentCreateViewModel
    {
        [Required]
        public int DoctorId { get; set; }

        [Required]
        public DateTime ScheduledDate { get; set; }

        public string? ReasonForVisit { get; set; }

        public List<DoctorSelectViewModel> Doctors { get; set; } = new();
    }

    public class DoctorSelectViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Specialty { get; set; }
    }
}