using DoAnWeb.Areas.Patient.ViewModels;
using DoAnWeb.Data;
using DoAnWeb.Models;
using DoAnWeb.Services;
using DoAnWeb.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace DoAnWeb.Areas.Patient.Controllers
{
    [Area("Patient")]
    [Authorize(Roles = "Patient")]
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAppointmentValidationService _appointmentValidationService;
        private readonly IEmailService _emailService;
        private readonly ISpecialtyPredictionService _specialtyPredictionService;
        private readonly IDoctorAutoAssignmentService _doctorAutoAssignmentService;

        public AppointmentController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IAppointmentValidationService appointmentValidationService,
            IEmailService emailService,
            ISpecialtyPredictionService specialtyPredictionService,
            IDoctorAutoAssignmentService doctorAutoAssignmentService)
        {
            _context = context;
            _userManager = userManager;
            _appointmentValidationService = appointmentValidationService;
            _emailService = emailService;
            _specialtyPredictionService = specialtyPredictionService;
            _doctorAutoAssignmentService = doctorAutoAssignmentService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (patient == null) return NotFound();

            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Where(a => a.PatientId == patient.Id)
                .OrderByDescending(a => a.ScheduledDate)
                .ToListAsync();

            return View(appointments);
        }

        public async Task<IActionResult> Create()
        {
            var model = new AppointmentCreateViewModel
            {
                ScheduledDate = DateTime.Now.AddDays(1)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentCreateViewModel model)
        {
            var prediction = _specialtyPredictionService.PredictSpecialty(model.ReasonForVisit);
            model.PredictedSpecialty = prediction.PredictedSpecialty;
            model.PredictionScore = prediction.MatchScore;
            model.MatchedKeywords = prediction.MatchedKeywords;
            model.PredictionMessage = prediction.Message;

            // SỬA: tự động gợi ý bác sĩ từ chuyên khoa dự đoán
            var doctorSuggestion = await _doctorAutoAssignmentService.SuggestDoctorAsync(
                model.PredictedSpecialty,
                model.ScheduledDate);

            model.SuggestedDoctorId = doctorSuggestion.DoctorId;
            model.SuggestedDoctorName = doctorSuggestion.DoctorName;
            model.SuggestedDoctorMessage = doctorSuggestion.Message;
            model.SuggestedRemainingSlots = doctorSuggestion.RemainingSlots;

            // SỬA: nếu hệ thống tìm được bác sĩ phù hợp thì tự gán vào DoctorId
            if (doctorSuggestion.IsAssigned && doctorSuggestion.DoctorId.HasValue)
            {
                model.DoctorId = doctorSuggestion.DoctorId.Value;
            }

            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (patient == null) return NotFound();

            if (user.IsSpam)
            {
                ModelState.AddModelError("", "Tài khoản của bạn hiện đang bị đánh dấu spam. Vui lòng liên hệ bệnh viện để được hỗ trợ.");
                return View(model);
            }

            var todayStart = DateTime.Today;
            var todayEnd = todayStart.AddDays(1);

            var totalAppointmentsToday = await _context.Appointments
                .CountAsync(a => a.PatientId == patient.Id
                              && a.CreatedAt >= todayStart
                              && a.CreatedAt < todayEnd);

            if (totalAppointmentsToday >= 5)
            {
                user.IsSpam = true;
                await _userManager.UpdateAsync(user);

                _context.Notifications.Add(new Notification
                {
                    UserId = user.Id,
                    Title = "Tài khoản bị đánh dấu spam",
                    Message = "Tài khoản của bạn đã bị tạm đánh dấu spam do đặt lịch quá 5 lần trong ngày. Vui lòng liên hệ bệnh viện để được hỗ trợ.",
                    CreatedAt = DateTime.Now,
                    IsRead = false
                });

                await _context.SaveChangesAsync();

                ModelState.AddModelError("", "Bạn đã vượt quá số lần đặt lịch cho phép trong ngày. Tài khoản đã bị đánh dấu spam.");
                return View(model);
            }

            if (model.DoctorId <= 0)
            {
                ModelState.AddModelError("", "Hệ thống chưa tìm được bác sĩ phù hợp cho khung giờ này.");
                return View(model);
            }

            Appointment appointment;
            await using (var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable))
            {
                var validationResult = await _appointmentValidationService.ValidateAppointmentAsync(
                    model.DoctorId,
                    model.ScheduledDate);

                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }

                    return View(model);
                }

                appointment = new Appointment
                {
                    PatientId = patient.Id,
                    DoctorId = model.DoctorId,
                    CreatedAt = DateTime.Now,
                    ScheduledDate = model.ScheduledDate,
                    ReasonForVisit = model.ReasonForVisit,
                    Status = AppointmentStatus.Pending
                };

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }

            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == model.DoctorId);

            _context.Notifications.Add(new Notification
            {
                UserId = user.Id,
                Title = "Đặt lịch khám thành công",
                Message = $"Bạn đã đặt lịch khám vào lúc {appointment.ScheduledDate:dd/MM/yyyy HH:mm} với bác sĩ {(doctor?.User?.Name ?? "được hệ thống sắp xếp")}.",
                CreatedAt = DateTime.Now,
                IsRead = false
            });

            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                var subject = "Đặt lịch khám thành công - An Phúc Hospital";
                var body = $@"
                    <h3>Xin chào {user.Name},</h3>
                    <p>Bạn đã đặt lịch khám thành công.</p>
                    <p><strong>Thời gian khám:</strong> {appointment.ScheduledDate:dd/MM/yyyy HH:mm}</p>
                    <p><strong>Lý do khám:</strong> {appointment.ReasonForVisit}</p>
                    <p><strong>Chuyên khoa được hệ thống gợi ý:</strong> {model.PredictedSpecialty}</p>
                    <p><strong>Bác sĩ được gợi ý:</strong> {doctor?.User?.Name ?? model.SuggestedDoctorName}</p>
                    <p>Trân trọng,<br>An Phúc Hospital</p>";

                await _emailService.SendEmailAsync(user.Email, subject, body, user.Id);
            }

            TempData["Success"] = "Đặt lịch khám thành công.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Cancel(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (patient == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(a => a.Id == id && a.PatientId == patient.Id);

            if (appointment == null)
            {
                TempData["Error"] = "Không tìm thấy lịch hẹn.";
                return RedirectToAction(nameof(Index));
            }

            if (appointment.Status == AppointmentStatus.Cancelled)
            {
                TempData["Error"] = "Lịch hẹn này đã bị hủy trước đó.";
                return RedirectToAction(nameof(Index));
            }

            if (appointment.IsCheckedIn)
            {
                TempData["Error"] = "Không thể hủy lịch đã check-in.";
                return RedirectToAction(nameof(Index));
            }

            if (appointment.Status == AppointmentStatus.Completed)
            {
                TempData["Error"] = "Không thể hủy lịch đã hoàn thành.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new CancelAppointmentViewModel
            {
                Id = appointment.Id,
                ScheduledDate = appointment.ScheduledDate,
                DoctorName = appointment.Doctor?.User?.Name ?? "Chưa cập nhật",
                Specialty = appointment.Doctor?.Specialty?.Name ?? "Chưa cập nhật",
                ReasonForVisit = appointment.ReasonForVisit
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(CancelAppointmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (patient == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(a => a.Id == model.Id && a.PatientId == patient.Id);

            if (appointment == null)
            {
                TempData["Error"] = "Không tìm thấy lịch hẹn.";
                return RedirectToAction(nameof(Index));
            }

            if (appointment.Status == AppointmentStatus.Cancelled)
            {
                TempData["Error"] = "Lịch hẹn này đã bị hủy trước đó.";
                return RedirectToAction(nameof(Index));
            }

            if (appointment.IsCheckedIn)
            {
                TempData["Error"] = "Không thể hủy lịch đã check-in.";
                return RedirectToAction(nameof(Index));
            }

            if (appointment.Status == AppointmentStatus.Completed)
            {
                TempData["Error"] = "Không thể hủy lịch đã hoàn thành.";
                return RedirectToAction(nameof(Index));
            }

            appointment.Status = AppointmentStatus.Cancelled;
            appointment.CancellationTime = DateTime.Now;
            appointment.CancellationReason = model.CancellationReason;

            await _context.SaveChangesAsync();

            _context.Notifications.Add(new Notification
            {
                UserId = user.Id,
                Title = "Hủy lịch hẹn thành công",
                Message = $"Bạn đã hủy lịch khám vào lúc {appointment.ScheduledDate:dd/MM/yyyy HH:mm}. Lý do hủy: {model.CancellationReason}",
                CreatedAt = DateTime.Now,
                IsRead = false
            });

            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                var subject = "Hủy lịch khám thành công - An Phúc Hospital";
                var body = $@"
                    <h3>Xin chào {user.Name},</h3>
                    <p>Bạn đã hủy lịch khám thành công.</p>
                    <p><strong>Thời gian lịch cũ:</strong> {appointment.ScheduledDate:dd/MM/yyyy HH:mm}</p>
                    <p><strong>Lý do hủy:</strong> {model.CancellationReason}</p>
                    <p>Trân trọng,<br>An Phúc Hospital</p>";

                await _emailService.SendEmailAsync(user.Email, subject, body, user.Id);
            }

            TempData["Success"] = "Hủy lịch hẹn thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}
