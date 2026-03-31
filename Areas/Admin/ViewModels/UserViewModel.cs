using System.ComponentModel.DataAnnotations;

namespace DoAnWeb.Areas.Admin.ViewModels
{
    public class UserViewModel
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn giới tính")]
        public string Gender { get; set; } = "Nam";

        [Required(ErrorMessage = "Vui lòng chọn ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        public string Role { get; set; } = string.Empty;

        public bool IsSpam { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu nhập lại không khớp")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}