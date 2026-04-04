using System.Diagnostics;
using System.Text;
using System.Text.Json;
using DoAnWeb.Data;
using DoAnWeb.Models;
using DoAnWeb.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DoAnWeb.Services
{
    public class SeasonalStaffingDetectionService : ISeasonalStaffingDetectionService
    {
        private readonly ApplicationDbContext _context;
        private readonly SeasonalAnomalyDetectionOptions _options;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<SeasonalStaffingDetectionService> _logger;

        public SeasonalStaffingDetectionService(
            ApplicationDbContext context,
            IOptions<SeasonalAnomalyDetectionOptions> options,
            IWebHostEnvironment environment,
            ILogger<SeasonalStaffingDetectionService> logger)
        {
            _context = context;
            _options = options.Value;
            _environment = environment;
            _logger = logger;
        }

        public async Task<IReadOnlyList<SeasonalStaffingAlert>> DetectAsync(CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
            {
                return Array.Empty<SeasonalStaffingAlert>();
            }

            var currentWeekStart = GetWeekStart(DateTime.Today);
            var currentWeekEnd = currentWeekStart.AddDays(7);
            var firstWeekStart = currentWeekStart.AddDays(-7 * (_options.LookbackWeeks - 1));

            var specialties = await _context.Specialties
                .Include(s => s.Doctors)
                .OrderBy(s => s.Name)
                .ToListAsync(cancellationToken);

            if (!specialties.Any())
            {
                return Array.Empty<SeasonalStaffingAlert>();
            }

            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.Status != AppointmentStatus.Cancelled
                         && a.Doctor != null
                         && a.ScheduledDate >= firstWeekStart
                         && a.ScheduledDate < currentWeekEnd)
                .ToListAsync(cancellationToken);

            var request = new DetectionRequest
            {
                LookbackWeeks = _options.LookbackWeeks,
                MinimumHistoryWeeks = _options.MinimumHistoryWeeks,
                TreeCount = _options.TreeCount,
                SampleSize = _options.SampleSize,
                Contamination = _options.Contamination,
                SurgeMultiplier = _options.SurgeMultiplier,
                MinimumCurrentWeekCases = _options.MinimumCurrentWeekCases,
                Specialties = specialties.Select(s => new DetectionSpecialtyRequest
                {
                    SpecialtyId = s.Id,
                    SpecialtyName = s.Name,
                    CurrentDoctors = s.Doctors.Count,
                    MaxPatientsPerWeek = s.MaxPatientsPerWeek,
                    WeeklyCounts = BuildWeeklyCounts(appointments, s.Id, firstWeekStart, _options.LookbackWeeks)
                }).ToList()
            };

            var detectedAlerts = await RunPythonDetectionAsync(request, cancellationToken);

            return detectedAlerts
                .Where(a => a.IsAnomaly)
                .Select(a =>
                {
                    a.WeekStart = currentWeekStart;
                    a.WeekEnd = currentWeekEnd.AddDays(-1);
                    a.SuggestedExtraDoctors = CalculateSuggestedExtraDoctors(a);
                    return a;
                })
                .Where(a => a.CurrentWeekCases >= _options.MinimumCurrentWeekCases)
                .OrderByDescending(a => a.AnomalyScore)
                .ThenByDescending(a => a.CurrentWeekCases)
                .ToList();
        }

        public async Task<int> DetectAndNotifyEmployeesAsync(CancellationToken cancellationToken = default)
        {
            var alerts = await DetectAsync(cancellationToken);
            if (!alerts.Any())
            {
                return 0;
            }

            var employeeUserIds = await _context.Employees
                .Select(e => e.UserId)
                .ToListAsync(cancellationToken);

            if (!employeeUserIds.Any())
            {
                return 0;
            }

            var createdCount = 0;

            foreach (var alert in alerts)
            {
                var title = $"Cảnh báo tải khám theo tuần - {alert.SpecialtyName}";
                var message = BuildNotificationMessage(alert);

                foreach (var userId in employeeUserIds)
                {
                    var exists = await _context.Notifications.AnyAsync(
                        n => n.UserId == userId
                          && n.Title == title
                          && n.Message == message,
                        cancellationToken);

                    if (exists)
                    {
                        continue;
                    }

                    _context.Notifications.Add(new Notification
                    {
                        UserId = userId,
                        Title = title,
                        Message = message,
                        CreatedAt = DateTime.Now,
                        IsRead = false
                    });

                    createdCount++;
                }
            }

            if (createdCount > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }

            return createdCount;
        }

        private async Task<List<SeasonalStaffingAlert>> RunPythonDetectionAsync(DetectionRequest request, CancellationToken cancellationToken)
        {
            var pythonPath = ResolvePath(_options.PythonExecutablePath);
            var scriptPath = ResolvePath(_options.ScriptPath);

            if (!File.Exists(pythonPath))
            {
                _logger.LogWarning("Seasonal detection skipped because python executable was not found at {PythonPath}", pythonPath);
                return new List<SeasonalStaffingAlert>();
            }

            if (!File.Exists(scriptPath))
            {
                _logger.LogWarning("Seasonal detection skipped because script was not found at {ScriptPath}", scriptPath);
                return new List<SeasonalStaffingAlert>();
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = pythonPath,
                Arguments = $"\"{scriptPath}\"",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = _environment.ContentRootPath
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            var requestJson = JsonSerializer.Serialize(request);
            await process.StandardInput.WriteAsync(requestJson.AsMemory(), cancellationToken);
            await process.StandardInput.FlushAsync();
            process.StandardInput.Close();

            var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

            await process.WaitForExitAsync(cancellationToken);

            var output = await outputTask;
            var error = await errorTask;

            if (process.ExitCode != 0)
            {
                _logger.LogWarning("Seasonal detection script returned exit code {ExitCode}. Error: {Error}", process.ExitCode, error);
                return new List<SeasonalStaffingAlert>();
            }

            if (string.IsNullOrWhiteSpace(output))
            {
                return new List<SeasonalStaffingAlert>();
            }

            try
            {
                var response = JsonSerializer.Deserialize<DetectionResponse>(output);
                return response?.Alerts ?? new List<SeasonalStaffingAlert>();
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Seasonal detection output could not be parsed. Output: {Output}", output);
                return new List<SeasonalStaffingAlert>();
            }
        }

        private List<int> BuildWeeklyCounts(List<Appointment> appointments, int specialtyId, DateTime firstWeekStart, int lookbackWeeks)
        {
            var counts = new List<int>(lookbackWeeks);

            for (var i = 0; i < lookbackWeeks; i++)
            {
                var weekStart = firstWeekStart.AddDays(i * 7);
                var weekEnd = weekStart.AddDays(7);

                var count = appointments.Count(a =>
                    a.Doctor.SpecialtyId == specialtyId &&
                    a.ScheduledDate >= weekStart &&
                    a.ScheduledDate < weekEnd);

                counts.Add(count);
            }

            return counts;
        }

        private int CalculateSuggestedExtraDoctors(SeasonalStaffingAlert alert)
        {
            if (!alert.IsAnomaly)
            {
                return 0;
            }

            var effectiveCapacityPerDoctor = Math.Max(1, alert.MaxPatientsPerWeek / 4.0);
            var overload = Math.Max(0, alert.CurrentWeekCases - alert.BaselineCases);
            var extraDoctors = (int)Math.Ceiling(overload / effectiveCapacityPerDoctor);

            return Math.Max(1, extraDoctors);
        }

        private string BuildNotificationMessage(SeasonalStaffingAlert alert)
        {
            var builder = new StringBuilder();
            builder.Append($"Hệ thống phát hiện khoa {alert.SpecialtyName} có dấu hiệu tăng tải bất thường trong tuần {alert.WeekStart:dd/MM} - {alert.WeekEnd:dd/MM}. ");
            builder.Append($"Số ca hiện tại: {alert.CurrentWeekCases}, baseline gần nhất: {alert.BaselineCases:0.##}, điểm bất thường: {alert.AnomalyScore:0.000}. ");
            builder.Append($"Đề xuất bổ sung thêm {alert.SuggestedExtraDoctors} bác sĩ trực cho khoa này. ");
            if (!string.IsNullOrWhiteSpace(alert.Reason))
            {
                builder.Append($"Lý do: {alert.Reason}");
            }

            return builder.ToString();
        }

        private string ResolvePath(string path)
        {
            return Path.IsPathRooted(path)
                ? path
                : Path.GetFullPath(Path.Combine(_environment.ContentRootPath, path));
        }

        private static DateTime GetWeekStart(DateTime date)
        {
            var diff = ((int)date.DayOfWeek + 6) % 7;
            return date.Date.AddDays(-diff);
        }

        private sealed class DetectionRequest
        {
            public int LookbackWeeks { get; set; }
            public int MinimumHistoryWeeks { get; set; }
            public int TreeCount { get; set; }
            public int SampleSize { get; set; }
            public double Contamination { get; set; }
            public double SurgeMultiplier { get; set; }
            public int MinimumCurrentWeekCases { get; set; }
            public List<DetectionSpecialtyRequest> Specialties { get; set; } = new();
        }

        private sealed class DetectionSpecialtyRequest
        {
            public int SpecialtyId { get; set; }
            public string SpecialtyName { get; set; } = string.Empty;
            public int CurrentDoctors { get; set; }
            public int MaxPatientsPerWeek { get; set; }
            public List<int> WeeklyCounts { get; set; } = new();
        }

        private sealed class DetectionResponse
        {
            public List<SeasonalStaffingAlert> Alerts { get; set; } = new();
        }
    }
}
