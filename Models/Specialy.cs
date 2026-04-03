namespace DoAnWeb.Models;

public class Specialty
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int AveragePatientLoad { get; set; } // Tải bệnh nhân trung bình hàng ngày
    public int MaxPatientsPerWeek { get; set; } = 100; // Định mức năng suất tối đa của 1 bác sĩ trong 1 tuần
    public List<Doctor> Doctors { get; set; } = new();
}