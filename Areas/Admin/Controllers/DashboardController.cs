using DoAnWeb.Areas.Admin.ViewModels;
using DoAnWeb.Data;
using DoAnWeb.Models;
using DoAnWeb.Services;
using DoAnWeb.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISpecialtyLoadAnalysisService _specialtyLoadAnalysisService;

        public DashboardController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ISpecialtyLoadAnalysisService specialtyLoadAnalysisService)
        {
            _context = context;
            _userManager = userManager;
            _specialtyLoadAnalysisService = specialtyLoadAnalysisService;
        }

        public async Task<IActionResult> Index()
        {
            var model = new AdminDashboardViewModel
            {
                TotalUsers = await _userManager.Users.CountAsync(),
                TotalDoctors = await _context.Doctors.CountAsync(),
                TotalPatients = await _context.Patients.CountAsync(),
                TotalAppointments = await _context.Appointments.CountAsync(),

                PendingAppointments = await _context.Appointments
                    .CountAsync(a => a.Status == AppointmentStatus.Pending),

                CompletedAppointments = await _context.Appointments
                    .CountAsync(a => a.Status == AppointmentStatus.Completed),

                CancelledAppointments = await _context.Appointments
                    .CountAsync(a => a.Status == AppointmentStatus.Cancelled),

                RecentAppointments = await _context.Appointments
                    .Include(a => a.Patient)
                        .ThenInclude(p => p.User)
                    .Include(a => a.Doctor)
                        .ThenInclude(d => d.User)
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(8)
                    .ToListAsync(),

                // SỬA: load insight
                SpecialtyLoadInsights = await _specialtyLoadAnalysisService.AnalyzeSpecialtyLoadsAsync()
            };

            return View(model);
        }
    }
}