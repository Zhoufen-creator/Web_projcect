using DoAnWeb.Data;
using DoAnWeb.Models;
using DoAnWeb.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace DoAnWeb.Services
{
    public class AppointmentEstimateService : IAppointmentEstimateService
    {
        private readonly ApplicationDbContext _context;

        public AppointmentEstimateService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AppointmentEstimateResult> EstimateAsync(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
            {
                throw new Exception("Không tìm thấy lịch hẹn để ước lượng.");
            }

            var examDurations = await _context.MedicalExaminations
                .Where(m => m.DoctorId == appointment.DoctorId
                         && m.StartTime.HasValue
                         && m.EndTime.HasValue
                         && m.EndTime > m.StartTime)
                .Select(m => EF.Functions.DateDiffMinute(m.StartTime!.Value, m.EndTime!.Value))
                .ToListAsync();

            double averageMinutes = examDurations.Any()
                ? examDurations.Average()
                : 15;

            var appointmentDate = appointment.ScheduledDate.Date;

            var patientsAheadCount = await _context.Appointments
                .Where(a => a.DoctorId == appointment.DoctorId
                         && a.Id != appointment.Id
                         && a.Status != AppointmentStatus.Cancelled
                         && a.ScheduledDate.Date == appointmentDate
                         && a.ScheduledDate < appointment.ScheduledDate)
                .CountAsync();

            var estimatedWaitMinutes = (int)Math.Round(patientsAheadCount * averageMinutes);
            var estimatedStartTime = appointment.ScheduledDate.AddMinutes(estimatedWaitMinutes);

            string arrivalSuggestion;
            if (estimatedWaitMinutes <= 15)
            {
                arrivalSuggestion = "Bạn nên đến trước giờ hẹn khoảng 10 phút để làm thủ tục.";
            }
            else if (estimatedWaitMinutes <= 30)
            {
                arrivalSuggestion = "Bạn nên đến sớm khoảng 15 phút vì hiện có một số bệnh nhân phía trước.";
            }
            else
            {
                arrivalSuggestion = "Lịch khám có thể chờ lâu hơn dự kiến. Bạn nên đến sớm 15-20 phút.";
            }

            return new AppointmentEstimateResult
            {
                AverageExaminationMinutes = Math.Round(averageMinutes, 1),
                PatientsAheadCount = patientsAheadCount,
                EstimatedWaitMinutes = estimatedWaitMinutes,
                EstimatedStartTime = estimatedStartTime,
                ArrivalSuggestion = arrivalSuggestion
            };
        }
    }
}
