namespace DoAnWeb.Models;

public enum AppointmentStatus
{
    Pending,
    Confirmed,
    Completed,
    Cancelled
}

public class Appointment
{
    public int Id { get; set; }

    // Đã thêm trước đó: thời điểm tạo lịch
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime ScheduledDate { get; set; }
    public AppointmentStatus Status { get; set; }

    public string? ReasonForVisit { get; set; }

    public bool IsCheckedIn { get; set; } = false;
    public DateTime? CheckinTime { get; set; }
    public DateTime? CancellationTime { get; set; }
    public string? CancellationReason { get; set; }

    // SỬA: cờ đánh dấu đã gửi nhắc lịch trước 24 giờ chưa
    public bool IsReminder24hSent { get; set; } = false;

    // SỬA: thời điểm thực tế đã gửi nhắc 24 giờ
    public DateTime? Reminder24hSentAt { get; set; }

    // SỬA: cờ đánh dấu đã gửi nhắc lịch trước 2 giờ chưa
    public bool IsReminder2hSent { get; set; } = false;

    // SỬA: thời điểm thực tế đã gửi nhắc 2 giờ
    public DateTime? Reminder2hSentAt { get; set; }

    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    public List<MedicalExamination> MedicalExaminations { get; set; } = new();
    public List<ExaminationService> ExaminationServices { get; set; } = new();
}