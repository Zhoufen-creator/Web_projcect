using DoAnWeb.Areas.Doctor.ViewModels;
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
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var doctor = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialty)
                .FirstOrDefaultAsync(d => d.UserId == user.Id);

            if (doctor == null)
            {
                return NotFound();
            }

            if (doctor.User == null || doctor.Specialty == null)
            {
                TempData["Error"] = "Hồ sơ bác sĩ chưa đủ dữ liệu để hiển thị.";
                return RedirectToAction("Index", "Dashboard", new { area = "Doctor" });
            }

            var vm = new DoctorProfileViewModel
            {
                DoctorId = doctor.Id,
                Name = doctor.User.Name,
                DateOfBirth = doctor.User.DateOfBirth,
                Gender = doctor.User.Gender,
                Address = doctor.User.Address,
                PhoneNumber = doctor.User.PhoneNumber ?? string.Empty,
                Specialty = doctor.Specialty.Name,
                LicenseNumber = doctor.LicenseNumber,
                Qualifications = doctor.Qualifications
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(DoctorProfileViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var doctor = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialty)
                .FirstOrDefaultAsync(d => d.UserId == user.Id);

            if (doctor == null)
            {
                ModelState.AddModelError(string.Empty, "Không tìm thấy hồ sơ bác sĩ.");
                return View(vm);
            }

            if (doctor.User == null)
            {
                ModelState.AddModelError(string.Empty, "Hồ sơ người dùng của bác sĩ chưa sẵn sàng.");
                return View(vm);
            }

            doctor.User.Name = vm.Name;
            doctor.User.DateOfBirth = vm.DateOfBirth;
            doctor.User.Gender = vm.Gender ?? string.Empty;
            doctor.User.Address = vm.Address;
            doctor.User.PhoneNumber = vm.PhoneNumber;

            var userUpdateResult = await _userManager.UpdateAsync(doctor.User);
            if (!userUpdateResult.Succeeded)
            {
                foreach (var error in userUpdateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(vm);
            }

            doctor.LicenseNumber = vm.LicenseNumber;
            doctor.Qualifications = vm.Qualifications;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật thông tin thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}
