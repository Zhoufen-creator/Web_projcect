namespace DoAnWeb.Models;

public class DoctorSchedule
{
    public int Id { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int MaxPatient { get; set; } //Số lượng bệnh nhân tối đa trong khung giờ này

    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

}