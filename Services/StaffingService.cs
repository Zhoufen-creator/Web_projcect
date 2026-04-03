using DoAnWeb.Data;
using DoAnWeb.Models;
using DoAnWeb.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace DoAnWeb.Services
{
    public class StaffingService : IStaffingService
    {
        private readonly ApplicationDbContext _context;

        public StaffingService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tính số lượng bác sĩ cần trực cho chuyên khoa vào tuần tới.
        /// 
        /// Giải thuật: Trung bình động 4 tuần
        /// 1. Lấy tổng bệnh nhân của chuyên khoa trong 28 ngày gần nhất
        /// 2. Tính trung bình 1 tuần = Tổng / 4
        /// 3. Tính bác sĩ cần = Ceiling(Trung bình / Định mức năng suất 1 bác sĩ)
        /// 4. Đảm bảo >= 1
        /// </summary>
        public async Task<int> CalculateRequiredDoctorsAsync(int specialtyId)
        {
            // Lấy thông tin chuyên khoa
            var specialty = await _context.Specialties.FindAsync(specialtyId);
            if (specialty == null)
            {
                return 1; // Mặc định 1 bác sĩ nếu không tìm thấy chuyên khoa
            }

            // Tính thời điểm bắt đầu 28 ngày gần nhất
            var today = DateTime.Now.Date;
            var fourWeeksAgo = today.AddDays(-28);

            // Tính tổng bệnh nhân của chuyên khoa trong 28 ngày gần nhất
            var totalAppointments = await _context.Appointments
                .Where(a => a.Doctor.SpecialtyId == specialtyId
                         && a.ScheduledDate.Date >= fourWeeksAgo
                         && a.ScheduledDate.Date <= today
                         && a.Status != AppointmentStatus.Cancelled) // Không tính bệnh nhân hủy
                .CountAsync();

            // Tính trung bình bệnh nhân 1 tuần
            double averagePatientsPerWeek = (double)totalAppointments / 4;

            // Tính định mức năng suất 1 bác sĩ
            int maxPatientsPerWeek = specialty.MaxPatientsPerWeek > 0 ? specialty.MaxPatientsPerWeek : 100;

            // Tính số bác sĩ cần = Làm tròn lên (Math.Ceiling)
            int requiredDoctors = (int)Math.Ceiling(averagePatientsPerWeek / maxPatientsPerWeek);

            // Đảm bảo luôn >= 1
            return Math.Max(1, requiredDoctors);
        }

        /// <summary>
        /// Tính số lượng bác sĩ cần trực cho tất cả chuyên khoa
        /// </summary>
        public async Task<Dictionary<int, int>> CalculateRequiredDoctorsForAllSpecialtiesAsync()
        {
            var specialties = await _context.Specialties.ToListAsync();
            var result = new Dictionary<int, int>();

            foreach (var specialty in specialties)
            {
                var requiredDoctors = await CalculateRequiredDoctorsAsync(specialty.Id);
                result[specialty.Id] = requiredDoctors;
            }

            return result;
        }
    }
}
