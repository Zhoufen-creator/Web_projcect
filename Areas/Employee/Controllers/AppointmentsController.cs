using DoAnWeb.Areas.Employee.ViewModels;
using DoAnWeb.Data;
using DoAnWeb.Models;
using DoAnWeb.Services;
using DoAnWeb.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DoAnWeb.Areas.Employee.Controllers
{
    [Area("Employee")]
    [Authorize(Roles = "Employee,Admin")]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAppointmentValidationService _appointmentValidationService;
        private readonly IEmailService _emailService;

        public AppointmentsController(
            ApplicationDbContext context,
            IAppointmentValidationService appointmentValidationService,
            IEmailService emailService)
        {
            _context = context;
            _appointmentValidationService = appointmentValidationService;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .OrderByDescending(a => a.ScheduledDate)
                .ToListAsync();

            return View(appointments);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound();

            var vm = new AppointmentAssignVM
            {
                Id = appointment.Id,
                PatientName = appointment.Patient.User.Name,
                DoctorId = appointment.DoctorId,
                ScheduledDate = appointment.ScheduledDate,
                ReasonForVisit = appointment.ReasonForVisit,
                Status = appointment.Status
            };

            await LoadDoctors(vm.DoctorId);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AppointmentAssignVM vm)
        {
            if (id != vm.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadDoctors(vm.DoctorId);
                return View(vm);
            }

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound();

            if (vm.Status != AppointmentStatus.Cancelled)
            {
                var validationResult = await _appointmentValidationService.ValidateAppointmentAsync(
                    vm.DoctorId,
                    vm.ScheduledDate,
                    vm.Id);

                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }

                    await LoadDoctors(vm.DoctorId);
                    return View(vm);
                }
            }

            appointment.DoctorId = vm.DoctorId;
            appointment.ScheduledDate = vm.ScheduledDate;
            appointment.Status = vm.Status;

            _context.Update(appointment);
            await _context.SaveChangesAsync();

            _context.Notifications.Add(new Notification
            {
                UserId = appointment.Patient.UserId,
                Title = "Lịch hẹn đã được cập nhật",
                Message = $"Lịch hẹn của bạn đã được cập nhật. Thời gian khám hiện tại: {appointment.ScheduledDate:dd/MM/yyyy HH:mm}. Trạng thái: {appointment.Status}.",
                CreatedAt = DateTime.Now,
                IsRead = false
            });

            await _context.SaveChangesAsync();

            // SỬA: gửi mail thật
            if (!string.IsNullOrWhiteSpace(appointment.Patient.User.Email))
            {
                var subject = "Lịch hẹn đã được cập nhật - An Phúc Hospital";
                var body = $@"
                    <h3>Xin chào {appointment.Patient.User.Name},</h3>
                    <p>Lịch hẹn của bạn đã được cập nhật.</p>
                    <p><strong>Thời gian mới:</strong> {appointment.ScheduledDate:dd/MM/yyyy HH:mm}</p>
                    <p><strong>Trạng thái:</strong> {appointment.Status}</p>
                    <p>Vui lòng kiểm tra lại thông tin và đến đúng giờ.</p>
                    <p>Trân trọng,<br>An Phúc Hospital</p>";

                await _emailService.SendEmailAsync(
                    appointment.Patient.User.Email,
                    subject,
                    body,
                    appointment.Patient.UserId);
            }

            TempData["success"] = "Cập nhật lịch hẹn thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                TempData["error"] = "Không tìm thấy lịch hẹn.";
                return RedirectToAction(nameof(Index));
            }

            if (appointment.Status == AppointmentStatus.Cancelled)
            {
                TempData["error"] = "Không thể check-in cho lịch hẹn đã bị hủy.";
                return RedirectToAction(nameof(Index));
            }

            if (appointment.IsCheckedIn)
            {
                TempData["error"] = "Bệnh nhân đã check-in trước đó.";
                return RedirectToAction(nameof(Index));
            }

            appointment.IsCheckedIn = true;
            appointment.CheckinTime = DateTime.Now;

            if (appointment.Status == AppointmentStatus.Pending)
            {
                appointment.Status = AppointmentStatus.Confirmed;
            }

            await _context.SaveChangesAsync();

            _context.Notifications.Add(new Notification
            {
                UserId = appointment.Patient.UserId,
                Title = "Đã check-in thành công",
                Message = $"Bạn đã được check-in cho lịch hẹn lúc {appointment.ScheduledDate:dd/MM/yyyy HH:mm}. Vui lòng chờ đến lượt khám.",
                CreatedAt = DateTime.Now,
                IsRead = false
            });

            await _context.SaveChangesAsync();

            // SỬA: gửi mail thật
            if (!string.IsNullOrWhiteSpace(appointment.Patient.User.Email))
            {
                var subject = "Check-in thành công - An Phúc Hospital";
                var body = $@"
                    <h3>Xin chào {appointment.Patient.User.Name},</h3>
                    <p>Bạn đã được check-in thành công.</p>
                    <p><strong>Thời gian khám:</strong> {appointment.ScheduledDate:dd/MM/yyyy HH:mm}</p>
                    <p><strong>Thời gian check-in:</strong> {appointment.CheckinTime:dd/MM/yyyy HH:mm}</p>
                    <p>Vui lòng chờ đến lượt khám.</p>
                    <p>Trân trọng,<br>An Phúc Hospital</p>";

                await _emailService.SendEmailAsync(
                    appointment.Patient.User.Email,
                    subject,
                    body,
                    appointment.Patient.UserId);
            }

            TempData["success"] = "Check-in bệnh nhân thành công!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDoctors(int? selectedDoctorId = null)
        {
            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialty)
                .Select(d => new
                {
                    d.Id,
                    Name = (d.User != null ? d.User.Name : "Bác sĩ ẩn danh") + " - " +
                           (d.Specialty != null ? d.Specialty.Name : "Chưa rõ khoa")
                })
                .ToListAsync();

            ViewBag.DoctorId = new SelectList(doctors, "Id", "Name", selectedDoctorId);
        }
    }
}
