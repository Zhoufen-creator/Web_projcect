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
    public class PatientRecordController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PatientRecordController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> History(int patientId)
        {
            DoctorModel doctor = await GetCurrentDoctorAsync();
            if (doctor == null) return RedirectToAction("Login", "Account", new { area = "" });

            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == patientId);

            if (patient == null) return NotFound();

            var records = await _context.MedicalExaminations
                .Include(m => m.Appointment)
                .Include(m => m.Prescriptions)
                    .ThenInclude(p => p.Medicine)
                .Where(m => m.PatientId == patientId && m.DoctorId == doctor.Id)
                .OrderByDescending(m => m.StartTime)
                .ToListAsync();

            ViewBag.Patient = patient;
            return View(records);
        }

        private async Task<DoctorModel?> GetCurrentDoctorAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;

            return await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
        }
    }
}