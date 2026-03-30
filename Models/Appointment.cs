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
    public DateTime ScheduledDate { get; set; }
    public AppointmentStatus Status { get; set; }

    public string? ReasonForVisit { get; set; }

    public bool IsCheckedIn { get; set; } = false;
    public DateTime? CheckinTime { get; set; }
    public DateTime? CancellationTime { get; set; }
    public string? CancellationReason { get; set; }


    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    public List<MedicalExamination> MedicalExaminations { get; set; } = new();
    public List<ExaminationService> ExaminationServices { get; set; } = new();
    
}