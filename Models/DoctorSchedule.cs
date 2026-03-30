using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnWeb.Models
{
    public class DoctorSchedule
    {
        public int Id { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public int MaxPatient { get; set; }

        public int DoctorId { get; set; }

        [ForeignKey("DoctorId")]
        public Doctor? Doctor { get; set; }
    }
}