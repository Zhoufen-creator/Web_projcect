using DoAnWeb.Data;
using DoAnWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace DoAnWeb.Services
{
    public class DoctorAutoAssignmentService : IDoctorAutoAssignmentService
    {
        private readonly ApplicationDbContext _context;

        public DoctorAutoAssignmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DoctorAutoAssignResult> SuggestDoctorAsync(string? predictedSpecialty, DateTime scheduledDate)
        {
            if (string.IsNullOrWhiteSpace(predictedSpecialty))
            {
                return new DoctorAutoAssignResult
                {
                    IsAssigned = false,
                    Message = "Chưa có chuyên khoa dự đoán để gợi ý bác sĩ."
                };
            }

            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Where(d => d.Specialty != null && d.Specialty == predictedSpecialty)
                .ToListAsync();

            if (!doctors.Any())
            {
                return new DoctorAutoAssignResult
                {
                    IsAssigned = false,
                    Message = $"Không tìm thấy bác sĩ thuộc chuyên khoa {predictedSpecialty}."
                };
            }

            var availableDoctors = new List<DoctorAutoAssignResult>();

            foreach (var doctor in doctors)
            {
                var schedule = await _context.DoctorSchedules
                    .Where(s => s.DoctorId == doctor.Id
                             && s.StartTime <= scheduledDate
                             && s.EndTime >= scheduledDate)
                    .OrderBy(s => s.StartTime)
                    .FirstOrDefaultAsync();

                if (schedule == null)
                {
                    continue;
                }

                var currentPatientCount = await _context.Appointments
                    .CountAsync(a => a.DoctorId == doctor.Id
                                  && a.Status != AppointmentStatus.Cancelled
                                  && a.ScheduledDate >= schedule.StartTime
                                  && a.ScheduledDate <= schedule.EndTime);

                if (currentPatientCount >= schedule.MaxPatient)
                {
                    continue;
                }

                var isDuplicate = await _context.Appointments
                    .AnyAsync(a => a.DoctorId == doctor.Id
                                && a.Status != AppointmentStatus.Cancelled
                                && a.ScheduledDate == scheduledDate);

                if (isDuplicate)
                {
                    continue;
                }

                availableDoctors.Add(new DoctorAutoAssignResult
                {
                    DoctorId = doctor.Id,
                    DoctorName = doctor.User?.Name ?? "Chưa cập nhật",
                    Specialty = doctor.Specialty ?? "",
                    CurrentPatientCount = currentPatientCount,
                    RemainingSlots = schedule.MaxPatient - currentPatientCount,
                    IsAssigned = true,
                    Message = $"Hệ thống gợi ý bác sĩ {doctor.User?.Name} thuộc khoa {doctor.Specialty}."
                });
            }

            if (!availableDoctors.Any())
            {
                return new DoctorAutoAssignResult
                {
                    IsAssigned = false,
                    Message = $"Hiện chưa có bác sĩ phù hợp thuộc khoa {predictedSpecialty} còn trống lịch trong khung giờ này."
                };
            }

            var bestDoctor = availableDoctors
                .OrderBy(d => d.CurrentPatientCount)
                .ThenByDescending(d => d.RemainingSlots)
                .First();

            return bestDoctor;
        }
    }
}