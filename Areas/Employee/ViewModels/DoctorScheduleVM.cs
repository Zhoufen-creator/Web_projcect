using System.ComponentModel.DataAnnotations;

namespace DoAnWeb.Areas.Employee.ViewModels
{
    public class DoctorScheduleVM
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn chuyên khoa")]
        public int SpecialtyId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn bác sĩ")]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập thời gian bắt đầu")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập thời gian kết thúc")]
        public DateTime EndTime { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số bệnh nhân tối đa")]
        [Range(1, 100, ErrorMessage = "Số bệnh nhân phải từ 1 đến 100")]
        public int MaxPatient { get; set; }
    }
}
