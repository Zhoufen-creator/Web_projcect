using DoAnWeb.Areas.Doctor.ViewModels;
using DoAnWeb.Data;
using DoAnWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DoctorModel = DoAnWeb.Models.Doctor; 

namespace DoAnWeb.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    [Authorize(Roles = "Doctor")]
    public class MedicalExaminationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MedicalExaminationController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            DoctorModel doctor = await GetCurrentDoctorAsync();
            if (doctor == null) return RedirectToAction("Login", "Account", new { area = "" });

            var exams = await _context.MedicalExaminations
                .Include(m => m.Patient)
                    .ThenInclude(p => p.User)
                .Include(m => m.Appointment)
                .Where(m => m.DoctorId == doctor.Id)
                .OrderByDescending(m => m.StartTime)
                .ToListAsync();

            return View(exams);
        }

        public async Task<IActionResult> CreateOrEdit(int appointmentId)
        {
            DoctorModel doctor = await GetCurrentDoctorAsync();
            if (doctor == null) return RedirectToAction("Login", "Account", new { area = "" });

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(a => a.Id == appointmentId && a.DoctorId == doctor.Id);

            if (appointment == null) return NotFound();

            var exam = await _context.MedicalExaminations
                .FirstOrDefaultAsync(m => m.AppointmentId == appointmentId);

            var vm = new MedicalExaminationEditViewModel
            {
                Id = exam?.Id,
                AppointmentId = appointment.Id,
                PatientId = appointment.PatientId,
                DoctorId = doctor.Id,
                Symptoms = exam?.Symptoms,
                Diagnosis = exam?.Diagnosis,
                DoctorAvoid = exam?.DoctorAvoid,
                Status = exam?.Status ?? ExaminationStatus.InProgress,
                Medicines = await _context.Medicines
                    .Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Name })
                    .ToListAsync(),
                Services = await _context.MedicalServices
                    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                    .ToListAsync()
            };

            ViewBag.PatientName = appointment.Patient.User.Name;
            ViewBag.AppointmentTime = appointment.ScheduledDate;

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(MedicalExaminationEditViewModel vm)
        {
            DoctorModel doctor = await GetCurrentDoctorAsync();
            if (doctor == null) return RedirectToAction("Login", "Account", new { area = "" });

            vm.Medicines = await _context.Medicines
                .Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Name })
                .ToListAsync();

            vm.Services = await _context.MedicalServices
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToListAsync();

            if (!ModelState.IsValid)
                return View(vm);

            MedicalExamination exam;

            if (vm.Id.HasValue)
            {
                exam = await _context.MedicalExaminations.FindAsync(vm.Id.Value);
                if (exam == null) return NotFound();

                exam.Symptoms = vm.Symptoms;
                exam.Diagnosis = vm.Diagnosis;
                exam.DoctorAvoid = vm.DoctorAvoid;
                exam.Status = vm.Status;
                exam.EndTime = DateTime.Now;
            }
            else
            {
                exam = new MedicalExamination
                {
                    AppointmentId = vm.AppointmentId,
                    PatientId = vm.PatientId,
                    DoctorId = vm.DoctorId,
                    Symptoms = vm.Symptoms,
                    Diagnosis = vm.Diagnosis,
                    DoctorAvoid = vm.DoctorAvoid,
                    Status = vm.Status,
                    StartTime = DateTime.Now,
                    EndTime = DateTime.Now
                };

                _context.MedicalExaminations.Add(exam);
                await _context.SaveChangesAsync();
            }

            await _context.SaveChangesAsync();

            // Kê thuốc
            if (vm.SelectedMedicineId.HasValue)
            {
                var prescription = new Prescription
                {
                    MedicineId = vm.SelectedMedicineId.Value,
                    MedicalExaminationId = exam.Id,
                    Quantity = vm.MedicineQuantity,
                    Dosage = vm.MedicineDosage
                };

                _context.Prescriptions.Add(prescription);
            }

            // Ghi dịch vụ
            if (vm.SelectedMedicalServiceId.HasValue)
            {
                var service = new ExaminationService
                {
                    MedicalServiceId = vm.SelectedMedicalServiceId.Value,
                    AppointmentId = vm.AppointmentId,
                    Quantity = vm.ServiceQuantity,
                    Result = vm.ServiceResult,
                    CompletedAt = DateTime.Now
                };

                _context.ExaminationServices.Add(service);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã lưu phiếu khám.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<DoctorModel?> GetCurrentDoctorAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;

            return await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
        }
    }
}