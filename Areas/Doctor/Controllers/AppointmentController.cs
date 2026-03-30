using DoAnWeb.Data;
using DoAnWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoctorModel = DoAnWeb.Models.Doctor; 

namespace DoAnWeb.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    [Authorize(Roles = "Doctor")]
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            DoctorModel doctor = await GetCurrentDoctorAsync();
            if (doctor == null) return RedirectToAction("Login", "Account", new { area = "" });

            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Where(a => a.DoctorId == doctor.Id)
                .OrderByDescending(a => a.ScheduledDate)
                .ToListAsync();

            return View(appointments);
        }

        public async Task<IActionResult> Today()
        {
            DoctorModel doctor = await GetCurrentDoctorAsync();
            if (doctor == null) return RedirectToAction("Login", "Account", new { area = "" });

            var today = DateTime.Today;

            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Where(a => a.DoctorId == doctor.Id && a.ScheduledDate.Date == today)
                .OrderBy(a => a.ScheduledDate)
                .ToListAsync();

            return View(appointments);
        }

        private async Task<DoctorModel?> GetCurrentDoctorAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;

            return await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == user.Id);
        }
    }
}