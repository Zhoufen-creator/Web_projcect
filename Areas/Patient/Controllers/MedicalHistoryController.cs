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
    public class MedicalHistoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MedicalHistoryController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Patient/MedicalHistory
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (patient == null) return NotFound();

            var exams = await _context.MedicalExaminations
                .Include(m => m.Doctor).ThenInclude(d => d.User)
                .Include(m => m.Appointment)
                .Include(m => m.Prescriptions).ThenInclude(p => p.Medicine)
                .Where(m => m.PatientId == patient.Id)
                .OrderByDescending(m => m.StartTime)
                .ToListAsync();

            var appointmentIds = exams.Select(e => e.AppointmentId).ToList();

            var services = await _context.ExaminationServices
                .Include(es => es.MedicalService)
                .Where(es => appointmentIds.Contains(es.AppointmentId))
                .ToListAsync();

            var result = exams.Select(exam => new MedicalHistoryViewModel
            {
                ExaminationId = exam.Id,
                ExaminationDate = exam.StartTime,
                DoctorName = exam.Doctor.User.Name,
                Specialty = exam.Doctor.Specialty,
                Symptoms = exam.Symptoms,
                Diagnosis = exam.Diagnosis,
                DoctorAvoid = exam.DoctorAvoid,
                Status = exam.Status.ToString(),

                Prescriptions = exam.Prescriptions.Select(p => new PrescriptionItemViewModel
                {
                    MedicineName = p.Medicine.Name,
                    Dosage = p.Dosage,
                    Quantity = p.Quantity
                }).ToList(),

                Services = services
                    .Where(s => s.AppointmentId == exam.AppointmentId)
                    .Select(s => new ServiceItemViewModel
                    {
                        ServiceName = s.MedicalService.Name,
                        Quantity = s.Quantity,
                        Result = s.Result,
                        CompletedAt = s.CompletedAt
                    }).ToList()
            }).ToList();

            return View(result);
        }
    }
}