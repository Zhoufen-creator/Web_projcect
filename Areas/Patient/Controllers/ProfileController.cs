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
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Patient/Profile
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (patient == null)
            {
                patient = new Models.Patient
                {
                    UserId = user.Id
                };
                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();
            }

            var model = new PatientProfileViewModel
            {
                PatientId = patient.Id,
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                Address = user.Address,

                BloodType = patient.BloodType,
                Height = patient.Height,
                Weight = patient.Weight,
                HealthInsuranceNumber = patient.HealthInsuranceNumber,
                MedicalHistory = patient.MedicalHistory,
                Allergies = patient.Allergies
            };

            return View(model);
        }

        // POST: Patient/Profile
        [HttpPost]
        public async Task<IActionResult> Index(PatientProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (patient == null) return NotFound();

            // Update User
            user.Name = model.Name;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.DateOfBirth = model.DateOfBirth;
            user.Gender = model.Gender;
            user.Address = model.Address;

            // Update Patient
            patient.BloodType = model.BloodType;
            patient.Height = model.Height;
            patient.Weight = model.Weight;
            patient.HealthInsuranceNumber = model.HealthInsuranceNumber;
            patient.MedicalHistory = model.MedicalHistory;
            patient.Allergies = model.Allergies;

            await _userManager.UpdateAsync(user);
            await _context.SaveChangesAsync();

            ViewBag.Success = "Cập nhật thông tin thành công!";
            return View(model);
        }
    }
}