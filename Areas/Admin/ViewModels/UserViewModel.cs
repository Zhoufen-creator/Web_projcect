using System.ComponentModel.DataAnnotations;

namespace DoAnWeb.Areas.Admin.ViewModels
{
    public class UserViewModel
    {
        public string? Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string Gender { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }

        public string? Address { get; set; }

        public bool IsSpam { get; set; }

        [Required]
        public string Role { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}