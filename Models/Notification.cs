namespace DoAnWeb.Models;

public class Notification
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public bool IsRead { get; set; } = false;

    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
}