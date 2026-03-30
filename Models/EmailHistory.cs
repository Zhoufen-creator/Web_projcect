namespace DoAnWeb.Models
{
    public class EmailHistory
    {
        public int Id { get; set; }

        public string EmailType { get; set; } = string.Empty; // Loại email

        public string Content { get; set; } = string.Empty; // Nội dung email đã gửi

        public DateTime SentAt { get; set; } = DateTime.Now;

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
    }
}