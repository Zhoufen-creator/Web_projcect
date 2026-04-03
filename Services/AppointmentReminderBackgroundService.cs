using DoAnWeb.Data;
using DoAnWeb.Models;
using Microsoft.EntityFrameworkCore;
using DoAnWeb.Services.Interface;

namespace DoAnWeb.Services
{
    public class AppointmentReminderBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public AppointmentReminderBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessRemindersAsync(stoppingToken);

                // SỬA: lặp mỗi 5 phút
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private async Task ProcessRemindersAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            // SỬA: thêm service ước lượng thời gian khám
            var estimateService = scope.ServiceProvider.GetRequiredService<IAppointmentEstimateService>();

            var now = DateTime.Now;
            var next24Hours = now.AddHours(24);
            var next2Hours = now.AddHours(2);

            // 1. Reminder trước 24 giờ
            var appointments24h = await context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Where(a =>
                    a.Status != AppointmentStatus.Cancelled &&
                    a.Status != AppointmentStatus.Completed &&
                    !a.IsCheckedIn &&
                    !a.IsReminder24hSent &&
                    a.ScheduledDate > now &&
                    a.ScheduledDate <= next24Hours)
                .ToListAsync(stoppingToken);

            foreach (var appointment in appointments24h)
            {
                var patientUser = appointment.Patient?.User;
                var doctorUser = appointment.Doctor?.User;

                if (patientUser == null) continue;

                // SỬA: tính estimate cho lịch hẹn này
                var estimate = await estimateService.EstimateAsync(appointment.Id);

                var title = "Nhắc lịch khám trước 24 giờ";
                var message =
                    $"Bạn có lịch khám vào lúc {appointment.ScheduledDate:dd/MM/yyyy HH:mm} " +
                    $"với bác sĩ {doctorUser?.Name ?? "chưa cập nhật"}. " +
                    $"Ước tính có {estimate.PatientsAheadCount} bệnh nhân phía trước, " +
                    $"thời gian chờ khoảng {estimate.EstimatedWaitMinutes} phút. " +
                    $"{estimate.ArrivalSuggestion}";

                context.Notifications.Add(new Notification
                {
                    UserId = patientUser.Id,
                    Title = title,
                    Message = message,
                    CreatedAt = DateTime.Now,
                    IsRead = false
                });

                if (!string.IsNullOrWhiteSpace(patientUser.Email))
                {
                    var body = $@"
                        <h2>Nhắc lịch khám trước 24 giờ</h2>
                        <p>Xin chào <strong>{patientUser.Name}</strong>,</p>

                        <p>Bạn có lịch khám sắp tới tại <strong>An Phúc Hospital</strong>.</p>

                        <hr />

                        <p><strong>Thời gian khám:</strong> {appointment.ScheduledDate:dd/MM/yyyy HH:mm}</p>
                        <p><strong>Bác sĩ:</strong> {doctorUser?.Name ?? "Chưa cập nhật"}</p>
                        <p><strong>Chuyên khoa:</strong> {appointment.Doctor?.Specialty.Name ?? "Chưa cập nhật"}</p>
                        <p><strong>Lý do khám:</strong> {(string.IsNullOrWhiteSpace(appointment.ReasonForVisit) ? "Không có ghi chú" : appointment.ReasonForVisit)}</p>

                        <hr />

                        <h4>Ước lượng thời gian khám</h4>
                        <p><strong>Thời gian khám trung bình của bác sĩ:</strong> {estimate.AverageExaminationMinutes} phút</p>
                        <p><strong>Số bệnh nhân phía trước:</strong> {estimate.PatientsAheadCount}</p>
                        <p><strong>Thời gian chờ dự kiến:</strong> {estimate.EstimatedWaitMinutes} phút</p>
                        <p><strong>Khung bắt đầu khám ước tính:</strong> {estimate.EstimatedStartTime:dd/MM/yyyy HH:mm}</p>

                        <div style='margin-top:16px; padding:14px; background:#f4f8ff; border-left:4px solid #0f4c97; border-radius:8px;'>
                            <strong>Khuyến nghị:</strong> {estimate.ArrivalSuggestion}
                        </div>

                        <p style='margin-top:20px;'>Vui lòng đến sớm để làm thủ tục check-in và chuẩn bị khám.</p>

                        <p>Trân trọng,<br /><strong>An Phúc Hospital</strong></p>";

                    await emailService.SendEmailAsync(patientUser.Email, title, body, patientUser.Id);
                }

                appointment.IsReminder24hSent = true;
                appointment.Reminder24hSentAt = DateTime.Now;
            }

            // 2. Reminder trước 2 giờ
            var appointments2h = await context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Where(a =>
                    a.Status != AppointmentStatus.Cancelled &&
                    a.Status != AppointmentStatus.Completed &&
                    !a.IsCheckedIn &&
                    !a.IsReminder2hSent &&
                    a.ScheduledDate > now &&
                    a.ScheduledDate <= next2Hours)
                .ToListAsync(stoppingToken);

            foreach (var appointment in appointments2h)
            {
                var patientUser = appointment.Patient?.User;
                var doctorUser = appointment.Doctor?.User;

                if (patientUser == null) continue;

                // SỬA: tính estimate cho lịch hẹn này
                var estimate = await estimateService.EstimateAsync(appointment.Id);

                var title = "Nhắc lịch khám trước 2 giờ";
                var message =
                    $"Bạn có lịch khám trong vòng 2 giờ tới, lúc {appointment.ScheduledDate:dd/MM/yyyy HH:mm}, " +
                    $"với bác sĩ {doctorUser?.Name ?? "chưa cập nhật"}. " +
                    $"Ước tính có {estimate.PatientsAheadCount} bệnh nhân phía trước, " +
                    $"thời gian chờ khoảng {estimate.EstimatedWaitMinutes} phút. " +
                    $"{estimate.ArrivalSuggestion}";

                context.Notifications.Add(new Notification
                {
                    UserId = patientUser.Id,
                    Title = title,
                    Message = message,
                    CreatedAt = DateTime.Now,
                    IsRead = false
                });

                if (!string.IsNullOrWhiteSpace(patientUser.Email))
                {
                    var body = $@"
                        <h2>Nhắc lịch khám trước 2 giờ</h2>
                        <p>Xin chào <strong>{patientUser.Name}</strong>,</p>

                        <p>Bạn có lịch khám trong vòng <strong>2 giờ tới</strong> tại <strong>An Phúc Hospital</strong>.</p>

                        <hr />

                        <p><strong>Thời gian khám:</strong> {appointment.ScheduledDate:dd/MM/yyyy HH:mm}</p>
                        <p><strong>Bác sĩ:</strong> {doctorUser?.Name ?? "Chưa cập nhật"}</p>
                        <p><strong>Chuyên khoa:</strong> {appointment.Doctor?.Specialty.Name ?? "Chưa cập nhật"}</p>
                        <p><strong>Lý do khám:</strong> {(string.IsNullOrWhiteSpace(appointment.ReasonForVisit) ? "Không có ghi chú" : appointment.ReasonForVisit)}</p>

                        <hr />

                        <h4>Ước lượng thời gian khám</h4>
                        <p><strong>Thời gian khám trung bình của bác sĩ:</strong> {estimate.AverageExaminationMinutes} phút</p>
                        <p><strong>Số bệnh nhân phía trước:</strong> {estimate.PatientsAheadCount}</p>
                        <p><strong>Thời gian chờ dự kiến:</strong> {estimate.EstimatedWaitMinutes} phút</p>
                        <p><strong>Khung bắt đầu khám ước tính:</strong> {estimate.EstimatedStartTime:dd/MM/yyyy HH:mm}</p>

                        <div style='margin-top:16px; padding:14px; background:#fff7ed; border-left:4px solid #f59e0b; border-radius:8px;'>
                            <strong>Khuyến nghị:</strong> {estimate.ArrivalSuggestion}
                        </div>

                        <p style='margin-top:20px;'>Vui lòng chuẩn bị đến đúng giờ hoặc sớm hơn để tránh trễ lịch khám.</p>

                        <p>Trân trọng,<br /><strong>An Phúc Hospital</strong></p>";

                    await emailService.SendEmailAsync(patientUser.Email, title, body, patientUser.Id);
                }

                appointment.IsReminder2hSent = true;
                appointment.Reminder2hSentAt = DateTime.Now;
            }

            await context.SaveChangesAsync(stoppingToken);
        }
    }
}