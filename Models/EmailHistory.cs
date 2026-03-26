namespace DoAnWeb.Models;

public class EmailHistory
{
    public int Id { get; set; }
    public  string EmailType { get; set; } = string.Empty; //Loại email (ví dụ: "Appointment Confirmation", "Password Reset", v.v.)
    public string content { get; set; } = string.Empty; //Nội dung email đã gửi
    public DateTime SentAt { get; set; } = DateTime.Now;

    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
}