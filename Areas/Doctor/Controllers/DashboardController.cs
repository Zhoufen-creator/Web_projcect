using DoAnWeb.Data;
using DoAnWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnWeb.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    [Authorize(Roles = "Doctor")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account", new { area = "" });

            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == user.Id);

            if (doctor == null)
            {
                TempData["Error"] = "Không tìm thấy hồ sơ bác sĩ.";
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            var today = DateTime.Today;

            var todayAppointments = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Where(a => a.DoctorId == doctor.Id && a.ScheduledDate.Date == today)
                .OrderBy(a => a.ScheduledDate)
                .ToListAsync();

            var totalPatients = await _context.Appointments
                .Where(a => a.DoctorId == doctor.Id)
                .Select(a => a.PatientId)
                .Distinct()
                .CountAsync();

            var totalExaminations = await _context.MedicalExaminations
                .CountAsync(m => m.DoctorId == doctor.Id);

            ViewBag.Doctor = doctor;
            ViewBag.TodayAppointments = todayAppointments;
            ViewBag.TotalPatients = totalPatients;
            ViewBag.TotalExaminations = totalExaminations;

            return View();
        }
    }
}