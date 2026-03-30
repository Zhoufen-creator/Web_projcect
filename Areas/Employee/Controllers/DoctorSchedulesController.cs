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
    public class DoctorSchedulesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DoctorSchedulesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Danh sách ca làm
        public async Task<IActionResult> Index()
        {
            var schedules = await _context.DoctorSchedules
                .Include(ds => ds.Doctor)
                .ThenInclude(d => d.User)
                .OrderBy(ds => ds.StartTime)
                .ToListAsync();

            return View(schedules);
        }

        // GET: Create
        public async Task<IActionResult> Create()
        {
            await LoadDoctors();
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DoctorScheduleVM vm)
        {
            if (vm.EndTime <= vm.StartTime)
            {
                ModelState.AddModelError("", "Thời gian kết thúc phải lớn hơn thời gian bắt đầu.");
            }

            if (!ModelState.IsValid)
            {
                await LoadDoctors(vm.DoctorId);
                return View(vm);
            }

            var schedule = new DoctorSchedule
            {
                DoctorId = vm.DoctorId,
                StartTime = vm.StartTime,
                EndTime = vm.EndTime,
                MaxPatient = vm.MaxPatient
            };

            _context.DoctorSchedules.Add(schedule);
            await _context.SaveChangesAsync();

            TempData["success"] = "Tạo ca làm thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Edit
        public async Task<IActionResult> Edit(int id)
        {
            var schedule = await _context.DoctorSchedules.FindAsync(id);
            if (schedule == null) return NotFound();

            var vm = new DoctorScheduleVM
            {
                Id = schedule.Id,
                DoctorId = schedule.DoctorId,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                MaxPatient = schedule.MaxPatient
            };

            await LoadDoctors(vm.DoctorId);
            return View(vm);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DoctorScheduleVM vm)
        {
            if (id != vm.Id) return NotFound();

            if (vm.EndTime <= vm.StartTime)
            {
                ModelState.AddModelError("", "Thời gian kết thúc phải lớn hơn thời gian bắt đầu.");
            }

            if (!ModelState.IsValid)
            {
                await LoadDoctors(vm.DoctorId);
                return View(vm);
            }

            var schedule = await _context.DoctorSchedules.FindAsync(id);
            if (schedule == null) return NotFound();

            schedule.DoctorId = vm.DoctorId;
            schedule.StartTime = vm.StartTime;
            schedule.EndTime = vm.EndTime;
            schedule.MaxPatient = vm.MaxPatient;

            _context.Update(schedule);
            await _context.SaveChangesAsync();

            TempData["success"] = "Cập nhật ca làm thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Delete
        public async Task<IActionResult> Delete(int id)
        {
            var schedule = await _context.DoctorSchedules
                .Include(ds => ds.Doctor)
                .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(ds => ds.Id == id);

            if (schedule == null) return NotFound();

            return View(schedule);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var schedule = await _context.DoctorSchedules.FindAsync(id);
            if (schedule == null) return NotFound();

            _context.DoctorSchedules.Remove(schedule);
            await _context.SaveChangesAsync();

            TempData["success"] = "Xóa ca làm thành công!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDoctors(int? selectedDoctorId = null)
        {
            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Select(d => new
                {
                    d.Id,
                    Name = d.User.Name + " - " + d.Specialty
                })
                .ToListAsync();

            ViewBag.DoctorId = new SelectList(doctors, "Id", "Name", selectedDoctorId);
        }
    }
}