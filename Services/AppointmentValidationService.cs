using DoAnWeb.Data;
using DoAnWeb.Models;
using DoAnWeb.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace DoAnWeb.Services
{
    public class AppointmentValidationService : IAppointmentValidationService
    {
        private readonly ApplicationDbContext _context;

        public AppointmentValidationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(bool IsValid, List<string> Errors)> ValidateAppointmentAsync(
            int doctorId,
            DateTime scheduledDate,
            int? excludeAppointmentId = null)
        {
            var errors = new List<string>();

            // 1. Kiểm tra bác sĩ có tồn tại không
            var doctorExists = await _context.Doctors.AnyAsync(d => d.Id == doctorId);
            if (!doctorExists)
            {
                errors.Add("Bác sĩ không tồn tại trong hệ thống.");
                return (false, errors);
            }

            // 2. Kiểm tra bác sĩ có ca trực tại thời điểm này không
            var matchedSchedule = await _context.DoctorSchedules
                .Where(ds => ds.DoctorId == doctorId
                          && ds.StartTime <= scheduledDate
                          && scheduledDate < ds.EndTime)
                .OrderBy(ds => ds.StartTime)
                .FirstOrDefaultAsync();

            if (matchedSchedule == null)
            {
                errors.Add("Bác sĩ không có ca trực tại thời gian bạn chọn.");
                return (false, errors);
            }

            // 3. Kiểm tra trùng giờ
            var duplicatedAppointment = await _context.Appointments
                .Where(a => a.DoctorId == doctorId
                         && a.ScheduledDate == scheduledDate
                         && a.Status != AppointmentStatus.Cancelled
                         && (!excludeAppointmentId.HasValue || a.Id != excludeAppointmentId.Value))
                .AnyAsync();

            if (duplicatedAppointment)
            {
                errors.Add("Bác sĩ đã có lịch khám trùng đúng thời gian này.");
            }

            // 4. Kiểm tra số lượng bệnh nhân tối đa trong ca
            var totalAppointmentsInShift = await _context.Appointments
                .Where(a => a.DoctorId == doctorId
                         && a.ScheduledDate >= matchedSchedule.StartTime
                         && a.ScheduledDate < matchedSchedule.EndTime
                         && a.Status != AppointmentStatus.Cancelled
                         && (!excludeAppointmentId.HasValue || a.Id != excludeAppointmentId.Value))
                .CountAsync();

            if (totalAppointmentsInShift >= matchedSchedule.MaxPatient)
            {
                errors.Add("Ca trực này đã đủ số lượng bệnh nhân tối đa.");
            }

            return (!errors.Any(), errors);
        }
    }
}
