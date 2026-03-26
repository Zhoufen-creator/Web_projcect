using Microsoft.AspNetCore.Identity;

namespace DoAnWeb.Models;

public class ApplicationUser : IdentityUser
{
    public string Name { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool IsSpam { get; set; } = false;
    public DateTime CreateAt { get; set; } = DateTime.Now;
    public Patient Patient { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
    public Employee Employee { get; set; }  = null!;
}