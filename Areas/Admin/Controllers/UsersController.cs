using DoAnWeb.Areas.Admin.ViewModels;
using DoAnWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DoAnWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
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

            if (result.Succeeded)
            {
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

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
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

            if (string.IsNullOrWhiteSpace(model.Gender))
            {
                model.Gender = "Nam";
            }

            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id!);
            if (user == null)
            {
                return NotFound();
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
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            if (!string.IsNullOrWhiteSpace(model.Role) && await _roleManager.RoleExistsAsync(model.Role))
            {
                await _userManager.AddToRoleAsync(user, model.Role);
            }

            TempData["SuccessMessage"] = "Cập nhật tài khoản thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
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
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            await _userManager.DeleteAsync(user);
            TempData["SuccessMessage"] = "Xóa tài khoản thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}