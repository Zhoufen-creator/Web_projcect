using DoAnWeb.Data;
using DoAnWeb.Models;
using Microsoft.EntityFrameworkCore;
using DoAnWeb.Services.Interface;
namespace DoAnWeb.Services
{
    public class SpecialtyLoadAnalysisService : ISpecialtyLoadAnalysisService
    {
        private readonly ApplicationDbContext _context;

        public SpecialtyLoadAnalysisService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<SpecialtyLoadInsight>> AnalyzeSpecialtyLoadsAsync()
        {
            var now = DateTime.Now;

            // Tuần hiện tại: 7 ngày gần nhất
            var currentWeekStart = now.Date.AddDays(-6);
            var currentWeekEnd = now.Date.AddDays(1);

            // Lịch sử: 12 tuần trước đó
            var historyStart = currentWeekStart.AddDays(-84);

            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                .Where(a =>
                    a.Status != AppointmentStatus.Cancelled &&
                    a.Doctor != null &&
                    a.Doctor.Specialty != null &&
                    a.ScheduledDate >= historyStart &&
                    a.ScheduledDate < currentWeekEnd)
                .ToListAsync();

            var result = new List<SpecialtyLoadInsight>();

            var specialties = appointments
                .Where(a => !string.IsNullOrWhiteSpace(a.Doctor!.Specialty.Name))
                .Select(a => a.Doctor!.Specialty!)
                .Distinct()
                .ToList();

            foreach (var specialty in specialties)
            {
                var specialtyAppointments = appointments
                    .Where(a => a.Doctor!.Specialty == specialty)
                    .ToList();

                // Số ca tuần hiện tại
                var currentWeekCount = specialtyAppointments
                    .Count(a => a.ScheduledDate >= currentWeekStart && a.ScheduledDate < currentWeekEnd);

                // Tính trung bình 12 tuần trước
                var historicalWeeklyCounts = new List<int>();

                for (int i = 1; i <= 12; i++)
                {
                    var weekStart = currentWeekStart.AddDays(-7 * i);
                    var weekEnd = weekStart.AddDays(7);

                    var count = specialtyAppointments
                        .Count(a => a.ScheduledDate >= weekStart && a.ScheduledDate < weekEnd);

                    historicalWeeklyCounts.Add(count);
                }

                var averageWeeklyCount = historicalWeeklyCounts.Any()
                    ? historicalWeeklyCounts.Average()
                    : 0;

                double increaseRatio = 0;
                if (averageWeeklyCount > 0)
                {
                    increaseRatio = currentWeekCount / averageWeeklyCount;
                }
                else if (currentWeekCount > 0)
                {
                    increaseRatio = 999; // Trường hợp chưa có lịch sử mà tuần này bùng lên
                }

                // Ngưỡng bất thường: tăng >= 1.5 lần
                var isAbnormal = averageWeeklyCount > 0
                    ? increaseRatio >= 1.5
                    : currentWeekCount >= 5;

                var recommendation = isAbnormal
                    ? $"Khoa {specialty} đang có dấu hiệu tăng ca bất thường. Nên cân nhắc tăng bác sĩ trực hoặc mở rộng ca khám."
                    : $"Khoa {specialty} đang hoạt động ổn định.";

                result.Add(new SpecialtyLoadInsight
                {
                    Specialty = specialty.Name,
                    CurrentWeekCount = currentWeekCount,
                    AverageWeeklyCount = Math.Round(averageWeeklyCount, 2),
                    IncreaseRatio = Math.Round(increaseRatio, 2),
                    IsAbnormal = isAbnormal,
                    Recommendation = recommendation
                });
            }

            return result
                .OrderByDescending(x => x.IsAbnormal)
                .ThenByDescending(x => x.IncreaseRatio)
                .ThenByDescending(x => x.CurrentWeekCount)
                .ToList();
        }
    }
}