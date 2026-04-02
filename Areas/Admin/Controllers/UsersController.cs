using DoAnWeb.Areas.Admin.ViewModels;
using DoAnWeb.Data;
using DoAnWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var model = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                model.Add(new UserViewModel
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email ?? "",
                    PhoneNumber = user.PhoneNumber ?? "",
                    Gender = string.IsNullOrWhiteSpace(user.Gender) ? "Nam" : user.Gender,
                    DateOfBirth = user.DateOfBirth,
                    Address = user.Address ?? "",
                    IsSpam = user.IsSpam,
                    Role = roles.FirstOrDefault() ?? ""
                });
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();

            var model = new UserViewModel
            {
                Gender = "Nam"
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserViewModel model)
        {
            ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();

            if (string.IsNullOrWhiteSpace(model.Gender))
            {
                model.Gender = "Nam";
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingUserByEmail != null)
            {
                ModelState.AddModelError("Email", "Email này đã tồn tại trong hệ thống.");
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.Role))
            {
                ModelState.AddModelError("Role", "Vui lòng chọn vai trò.");
                return View(model);
            }

            var roleExists = await _roleManager.RoleExistsAsync(model.Role);
            if (!roleExists)
            {
                ModelState.AddModelError("Role", "Vai trò không tồn tại trong hệ thống.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Name = model.Name,
                PhoneNumber = model.PhoneNumber,
                Gender = model.Gender,
                DateOfBirth = model.DateOfBirth,
                Address = model.Address,
                IsSpam = model.IsSpam
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }

            var addRoleResult = await _userManager.AddToRoleAsync(user, model.Role);
            if (!addRoleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);

                foreach (var error in addRoleResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }

            TempData["SuccessMessage"] = "Tạo tài khoản mới thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);

            var model = new UserViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                Gender = string.IsNullOrWhiteSpace(user.Gender) ? "Nam" : user.Gender,
                DateOfBirth = user.DateOfBirth,
                Address = user.Address ?? "",
                IsSpam = user.IsSpam,
                Role = roles.FirstOrDefault() ?? ""
            };

            ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
            return View(model);
        }

        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(UserViewModel model)
{
    ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();

    // Bỏ validate password khi sửa
    ModelState.Remove("Password");
    ModelState.Remove("ConfirmPassword");

    if (string.IsNullOrWhiteSpace(model.Id))
    {
        TempData["ErrorMessage"] = "Không tìm thấy mã tài khoản cần cập nhật.";
        return View(model);
    }

    if (string.IsNullOrWhiteSpace(model.Gender))
    {
        model.Gender = "Nam";
    }

    if (string.IsNullOrWhiteSpace(model.Role))
    {
        ModelState.AddModelError("Role", "Vui lòng chọn vai trò.");
        return View(model);
    }

    var roleExists = await _roleManager.RoleExistsAsync(model.Role);
    if (!roleExists)
    {
        ModelState.AddModelError("Role", "Vai trò không tồn tại trong hệ thống.");
        return View(model);
    }

    if (!ModelState.IsValid)
    {
        return View(model);
    }

    var user = await _userManager.FindByIdAsync(model.Id);
    if (user == null)
    {
        TempData["ErrorMessage"] = "Không tìm thấy tài khoản.";
        return View(model);
    }

    var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
    if (existingUserByEmail != null && existingUserByEmail.Id != model.Id)
    {
        ModelState.AddModelError("Email", "Email này đã tồn tại trong hệ thống.");
        return View(model);
    }

    user.Name = model.Name;
    user.Email = model.Email;
    user.UserName = model.Email;
    user.PhoneNumber = model.PhoneNumber;
    user.Gender = model.Gender;
    user.DateOfBirth = model.DateOfBirth;
    user.Address = model.Address;
    user.IsSpam = model.IsSpam;

    var updateResult = await _userManager.UpdateAsync(user);
    if (!updateResult.Succeeded)
    {
        foreach (var error in updateResult.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        return View(model);
    }

    var currentRoles = await _userManager.GetRolesAsync(user);
    var currentRole = currentRoles.FirstOrDefault();

    // Chỉ đổi role khi thật sự khác
    if (currentRole != model.Role)
    {
        if (currentRoles.Any())
        {
            var removeRoleResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeRoleResult.Succeeded)
            {
                foreach (var error in removeRoleResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }
        }

        var addRoleResult = await _userManager.AddToRoleAsync(user, model.Role);
        if (!addRoleResult.Succeeded)
        {
            foreach (var error in addRoleResult.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }
    }

    TempData["SuccessMessage"] = "Cập nhật tài khoản thành công.";
    return RedirectToAction(nameof(Index));
}

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);

            var model = new UserViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                Gender = string.IsNullOrWhiteSpace(user.Gender) ? "Nam" : user.Gender,
                DateOfBirth = user.DateOfBirth,
                Address = user.Address ?? "",
                IsSpam = user.IsSpam,
                Role = roles.FirstOrDefault() ?? ""
            };

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(user.Email) &&
                user.Email.ToLower() == "admin@gmail.com")
            {
                TempData["ErrorMessage"] = "Không được xóa tài khoản admin gốc.";
                return RedirectToAction(nameof(Index));
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.UserId == id);

                if (patient != null)
                {
                    var patientAppointments = await _context.Appointments
                        .Where(a => a.PatientId == patient.Id)
                        .ToListAsync();

                    foreach (var appointment in patientAppointments)
                    {
                        var examServices = await _context.ExaminationServices
                            .Where(es => es.AppointmentId == appointment.Id)
                            .ToListAsync();

                        if (examServices.Any())
                        {
                            _context.ExaminationServices.RemoveRange(examServices);
                        }

                        var medicalExams = await _context.MedicalExaminations
                            .Where(me => me.AppointmentId == appointment.Id)
                            .ToListAsync();

                        foreach (var exam in medicalExams)
                        {
                            var prescriptions = await _context.Prescriptions
                                .Where(p => p.MedicalExaminationId == exam.Id)
                                .ToListAsync();

                            if (prescriptions.Any())
                            {
                                _context.Prescriptions.RemoveRange(prescriptions);
                            }
                        }

                        if (medicalExams.Any())
                        {
                            _context.MedicalExaminations.RemoveRange(medicalExams);
                        }
                    }

                    if (patientAppointments.Any())
                    {
                        _context.Appointments.RemoveRange(patientAppointments);
                    }

                    var extraPatientExams = await _context.MedicalExaminations
                        .Where(me => me.PatientId == patient.Id)
                        .ToListAsync();

                    foreach (var exam in extraPatientExams)
                    {
                        var prescriptions = await _context.Prescriptions
                            .Where(p => p.MedicalExaminationId == exam.Id)
                            .ToListAsync();

                        if (prescriptions.Any())
                        {
                            _context.Prescriptions.RemoveRange(prescriptions);
                        }
                    }

                    if (extraPatientExams.Any())
                    {
                        _context.MedicalExaminations.RemoveRange(extraPatientExams);
                    }

                    _context.Patients.Remove(patient);
                }

                var doctor = await _context.Doctors
                    .FirstOrDefaultAsync(d => d.UserId == id);

                if (doctor != null)
                {
                    var doctorSchedules = await _context.DoctorSchedules
                        .Where(ds => ds.DoctorId == doctor.Id)
                        .ToListAsync();

                    if (doctorSchedules.Any())
                    {
                        _context.DoctorSchedules.RemoveRange(doctorSchedules);
                    }

                    var doctorAppointments = await _context.Appointments
                        .Where(a => a.DoctorId == doctor.Id)
                        .ToListAsync();

                    foreach (var appointment in doctorAppointments)
                    {
                        var examServices = await _context.ExaminationServices
                            .Where(es => es.AppointmentId == appointment.Id)
                            .ToListAsync();

                        if (examServices.Any())
                        {
                            _context.ExaminationServices.RemoveRange(examServices);
                        }

                        var medicalExams = await _context.MedicalExaminations
                            .Where(me => me.AppointmentId == appointment.Id)
                            .ToListAsync();

                        foreach (var exam in medicalExams)
                        {
                            var prescriptions = await _context.Prescriptions
                                .Where(p => p.MedicalExaminationId == exam.Id)
                                .ToListAsync();

                            if (prescriptions.Any())
                            {
                                _context.Prescriptions.RemoveRange(prescriptions);
                            }
                        }

                        if (medicalExams.Any())
                        {
                            _context.MedicalExaminations.RemoveRange(medicalExams);
                        }
                    }

                    if (doctorAppointments.Any())
                    {
                        _context.Appointments.RemoveRange(doctorAppointments);
                    }

                    var extraDoctorExams = await _context.MedicalExaminations
                        .Where(me => me.DoctorId == doctor.Id)
                        .ToListAsync();

                    foreach (var exam in extraDoctorExams)
                    {
                        var prescriptions = await _context.Prescriptions
                            .Where(p => p.MedicalExaminationId == exam.Id)
                            .ToListAsync();

                        if (prescriptions.Any())
                        {
                            _context.Prescriptions.RemoveRange(prescriptions);
                        }
                    }

                    if (extraDoctorExams.Any())
                    {
                        _context.MedicalExaminations.RemoveRange(extraDoctorExams);
                    }

                    _context.Doctors.Remove(doctor);
                }

                var employee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.UserId == id);

                if (employee != null)
                {
                    _context.Employees.Remove(employee);
                }

                var notifications = await _context.Notifications
                    .Where(n => n.UserId == id)
                    .ToListAsync();

                if (notifications.Any())
                {
                    _context.Notifications.RemoveRange(notifications);
                }

                var emailHistories = await _context.EmailHistories
                    .Where(e => e.UserId == id)
                    .ToListAsync();

                if (emailHistories.Any())
                {
                    _context.EmailHistories.RemoveRange(emailHistories);
                }

                var siteVisits = await _context.SiteVisits
                    .Where(s => s.UserId == id)
                    .ToListAsync();

                if (siteVisits.Any())
                {
                    _context.SiteVisits.RemoveRange(siteVisits);
                }

                await _context.SaveChangesAsync();

                var currentRoles = await _userManager.GetRolesAsync(user);
                if (currentRoles.Any())
                {
                    var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);

                    if (!removeRolesResult.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        TempData["ErrorMessage"] = "Không thể xóa vai trò của tài khoản trước khi xóa.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                var deleteResult = await _userManager.DeleteAsync(user);
                if (!deleteResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    TempData["ErrorMessage"] = "Xóa tài khoản thất bại.";
                    return RedirectToAction(nameof(Index));
                }

                await transaction.CommitAsync();

                TempData["SuccessMessage"] = "Xóa tài khoản thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa tài khoản: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}