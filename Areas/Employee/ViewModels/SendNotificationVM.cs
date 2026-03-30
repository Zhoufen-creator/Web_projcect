using System.ComponentModel.DataAnnotations;

namespace DoAnWeb.Areas.Employee.ViewModels
{
    public class SendNotificationVM
    {
        [Required(ErrorMessage = "Vui lòng chọn bệnh nhân")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập nội dung")]
        public string Message { get; set; } = string.Empty;
    }
}