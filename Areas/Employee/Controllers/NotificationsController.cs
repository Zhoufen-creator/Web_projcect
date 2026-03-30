using DoAnWeb.Areas.Employee.ViewModels;
using DoAnWeb.Data;
using DoAnWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DoAnWeb.Areas.Employee.Controllers
{
    [Area("Employee")]
    [Authorize(Roles = "Employee,Admin")]
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Danh sách thông báo
        public async Task<IActionResult> Index()
        {
            var notifications = await _context.Notifications
                .Include(n => n.User)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(notifications);
        }

        // GET: Create
        public async Task<IActionResult> Create()
        {
            await LoadPatients();
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SendNotificationVM vm)
        {
            if (!ModelState.IsValid)
            {
                await LoadPatients(vm.UserId);
                return View(vm);
            }

            var notification = new Notification
            {
                UserId = vm.UserId,
                Title = vm.Title,
                Message = vm.Message,
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            TempData["success"] = "Gửi thông báo thành công!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadPatients(string? selectedUserId = null)
        {
            var patients = await _context.Patients
                .Include(p => p.User)
                .Select(p => new
                {
                    p.UserId,
                    Name = p.User.Name + " - " + p.User.Email
                })
                .ToListAsync();

            ViewBag.UserId = new SelectList(patients, "UserId", "Name", selectedUserId);
        }
    }
}