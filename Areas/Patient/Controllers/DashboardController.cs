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
            if (user == null)
                return RedirectToAction("Login", "Account", new { area = "" });

            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (patient == null)
            {
                patient = new DoAnWeb.Models.Patient
                {
                    UserId = user.Id
                };

                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();

                patient = await _context.Patients
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.UserId == user.Id);
            }

            var upcomingAppointments = await _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
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