using DoAnWeb.Areas.Patient.ViewModels;
using DoAnWeb.Data;
using DoAnWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnWeb.Areas.Patient.Controllers
{
    [Area("Patient")]
    [Authorize(Roles = "Patient")]
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Patient/Appointment
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

        // GET: Patient/Appointment/Create
        public async Task<IActionResult> Create()
        {
            var doctors = await _context.Doctors
                .Include(d => d.User)
                .ToListAsync();

            var model = new AppointmentCreateViewModel
            {
                ScheduledDate = DateTime.Now.AddDays(1),
                Doctors = doctors.Select(d => new DoctorSelectViewModel
                {
                    Id = d.Id,
                    Name = d.User.Name,
                    Specialty = d.Specialty
                }).ToList()
            };

            return View(model);
        }

        // POST: Patient/Appointment/Create
        [HttpPost]
        public async Task<IActionResult> Create(AppointmentCreateViewModel model)
        {
            var doctors = await _context.Doctors
                .Include(d => d.User)
                .ToListAsync();

            model.Doctors = doctors.Select(d => new DoctorSelectViewModel
            {
                Id = d.Id,
                Name = d.User.Name,
                Specialty = d.Specialty
            }).ToList();

            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (patient == null) return NotFound();

            var appointment = new DoAnWeb.Models.Appointment
            {
                PatientId = patient.Id,
                DoctorId = model.DoctorId,
                ScheduledDate = model.ScheduledDate,
                ReasonForVisit = model.ReasonForVisit,
                Status = AppointmentStatus.Pending
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}