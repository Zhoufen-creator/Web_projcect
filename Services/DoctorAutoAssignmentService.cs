using System.Text;
using DoAnWeb.Data;
using DoAnWeb.Models;
using DoAnWeb.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace DoAnWeb.Services
{
    public class DoctorAutoAssignmentService : IDoctorAutoAssignmentService
    {
        private readonly ApplicationDbContext _context;

        private static readonly Dictionary<string, string[]> SpecialtyAliases = new()
        {
            ["noi tong quat"] = ["noi tong quat", "khoa noi", "noi"],
            ["tieu hoa"] = ["tieu hoa", "khoa tieu hoa"],
            ["tim mach"] = ["tim mach", "khoa tim mach"],
            ["da lieu"] = ["da lieu", "khoa da lieu"],
            ["mat"] = ["mat", "khoa mat"],
            ["tai mui hong"] = ["tai mui hong", "khoa tai mui hong", "tmh"],
            ["xuong khop"] = ["xuong khop", "co xuong khop", "khoa co xuong khop"]
        };

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

            var normalizedPredictedSpecialty = NormalizeSpecialty(predictedSpecialty);

            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialty)
                .ToListAsync();

            doctors = doctors
                .Where(d => SpecialtyMatches(d.Specialty?.Name, normalizedPredictedSpecialty))
                .ToList();

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
                    Specialty = doctor.Specialty?.Name ?? string.Empty,
                    CurrentPatientCount = currentPatientCount,
                    RemainingSlots = schedule.MaxPatient - currentPatientCount,
                    IsAssigned = true,
                    Message = $"Hệ thống gợi ý bác sĩ {doctor.User?.Name} thuộc khoa {doctor.Specialty?.Name}."
                });
            }

            if (!availableDoctors.Any())
            {
                return new DoctorAutoAssignResult
                {
                    IsAssigned = false,
                    Message = $"Hiện chưa có bác sĩ phù hợp thuộc khoa {predictedSpecialty} còn trong lịch ở khung giờ này."
                };
            }

            return availableDoctors
                .OrderBy(d => d.CurrentPatientCount)
                .ThenByDescending(d => d.RemainingSlots)
                .First();
        }

        private static bool SpecialtyMatches(string? doctorSpecialty, string normalizedPredictedSpecialty)
        {
            if (string.IsNullOrWhiteSpace(doctorSpecialty) || string.IsNullOrWhiteSpace(normalizedPredictedSpecialty))
            {
                return false;
            }

            var normalizedDoctorSpecialty = NormalizeSpecialty(doctorSpecialty);
            if (normalizedDoctorSpecialty == normalizedPredictedSpecialty)
            {
                return true;
            }

            var predictedAliases = ExpandAliases(normalizedPredictedSpecialty);
            var doctorAliases = ExpandAliases(normalizedDoctorSpecialty);

            return predictedAliases.Overlaps(doctorAliases);
        }

        private static HashSet<string> ExpandAliases(string normalizedSpecialty)
        {
            var aliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                normalizedSpecialty
            };

            foreach (var item in SpecialtyAliases)
            {
                if (item.Key == normalizedSpecialty || item.Value.Contains(normalizedSpecialty, StringComparer.OrdinalIgnoreCase))
                {
                    aliases.Add(item.Key);
                    foreach (var alias in item.Value)
                    {
                        aliases.Add(alias);
                    }
                }
            }

            return aliases;
        }

        private static string NormalizeSpecialty(string value)
        {
            var normalized = value.ToLowerInvariant().Trim();
            var replacements = new Dictionary<string, string>
            {
                ["Ã¡"] = "a", ["Ã "] = "a", ["áº£"] = "a", ["Ã£"] = "a", ["áº¡"] = "a",
                ["Äƒ"] = "a", ["áº¯"] = "a", ["áº±"] = "a", ["áº³"] = "a", ["áºµ"] = "a", ["áº·"] = "a",
                ["Ã¢"] = "a", ["áº¥"] = "a", ["áº§"] = "a", ["áº©"] = "a", ["áº«"] = "a", ["áº­"] = "a",
                ["Ã©"] = "e", ["Ã¨"] = "e", ["áº»"] = "e", ["áº½"] = "e", ["áº¹"] = "e",
                ["Ãª"] = "e", ["áº¿"] = "e", ["á»"] = "e", ["á»ƒ"] = "e", ["á»…"] = "e", ["á»‡"] = "e",
                ["Ã­"] = "i", ["Ã¬"] = "i", ["á»‰"] = "i", ["Ä©"] = "i", ["á»‹"] = "i",
                ["Ã³"] = "o", ["Ã²"] = "o", ["á»"] = "o", ["Ãµ"] = "o", ["á»"] = "o",
                ["Ã´"] = "o", ["á»‘"] = "o", ["á»“"] = "o", ["á»•"] = "o", ["á»—"] = "o", ["á»™"] = "o",
                ["Æ¡"] = "o", ["á»›"] = "o", ["á»"] = "o", ["á»Ÿ"] = "o", ["á»¡"] = "o", ["á»£"] = "o",
                ["Ãº"] = "u", ["Ã¹"] = "u", ["á»§"] = "u", ["Å©"] = "u", ["á»¥"] = "u",
                ["Æ°"] = "u", ["á»©"] = "u", ["á»«"] = "u", ["á»­"] = "u", ["á»¯"] = "u", ["á»±"] = "u",
                ["Ã½"] = "y", ["á»³"] = "y", ["á»·"] = "y", ["á»¹"] = "y", ["á»µ"] = "y",
                ["Ä‘"] = "d"
            };

            var builder = new StringBuilder(normalized);
            foreach (var replacement in replacements)
            {
                builder.Replace(replacement.Key, replacement.Value);
            }

            return builder.ToString();
        }
    }
}
