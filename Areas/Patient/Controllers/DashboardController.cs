using DoAnWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAnWeb.Models;

namespace DoAnWeb.Areas.Patient.Controllers
{
    [Area("Patient")]
    [Authorize(Roles = "Patient")]
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

            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (patient == null)
            {
                TempData["Error"] = "Không tìm thấy hồ sơ bệnh nhân.";
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            var upcomingAppointments = await _context.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.PatientId == patient.Id && a.ScheduledDate >= DateTime.Now)
                .OrderBy(a => a.ScheduledDate)
                .Take(5)
                .ToListAsync();

            var totalAppointments = await _context.Appointments
                .CountAsync(a => a.PatientId == patient.Id);

            var totalExaminations = await _context.MedicalExaminations
                .CountAsync(m => m.PatientId == patient.Id);

            ViewBag.Patient = patient;
            ViewBag.UpcomingAppointments = upcomingAppointments;
            ViewBag.TotalAppointments = totalAppointments;
            ViewBag.TotalExaminations = totalExaminations;

            return View();
        }
    }
}