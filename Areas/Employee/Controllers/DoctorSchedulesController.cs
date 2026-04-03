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
            await LoadSpecialties();
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
                await LoadSpecialties(vm.SpecialtyId);
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
            var schedule = await _context.DoctorSchedules
                .Include(ds => ds.Doctor)
                .FirstOrDefaultAsync(ds => ds.Id == id);
            if (schedule == null || schedule.Doctor == null) return NotFound();

            var vm = new DoctorScheduleVM
            {
                Id = schedule.Id,
                DoctorId = schedule.DoctorId,
                SpecialtyId = schedule.Doctor.SpecialtyId,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                MaxPatient = schedule.MaxPatient
            };

            await LoadSpecialties(vm.SpecialtyId);
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
                await LoadSpecialties(vm.SpecialtyId);
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

        // API: Tìm bác sĩ theo chuyên khoa và tên
        [HttpGet]
        public async Task<IActionResult> SearchDoctors(int specialtyId, string searchName = "")
        {
            var doctors = await _context.Doctors
                .Where(d => d.SpecialtyId == specialtyId)
                .Include(d => d.User)
                .AsNoTracking()
                .ToListAsync();

            // Lọc theo tên nếu có
            if (!string.IsNullOrWhiteSpace(searchName))
            {
                doctors = doctors.Where(d => d.User != null && 
                                     d.User.Name.Contains(searchName, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Trả về JSON
            var result = doctors.Select(d => new
            {
                id = d.Id,
                name = d.User?.Name ??  "Bác sĩ ẩn danh",
            }).ToList();

            return Json(result);
        }

        private async Task LoadSpecialties(int? selectedSpecialtyId = null)
        {
            var specialties = await _context.Specialties
                .OrderBy(s => s.Name)
                .Select(s => new { s.Id, s.Name })
                .ToListAsync();

            ViewBag.SpecialtyId = new SelectList(specialties, "Id", "Name", selectedSpecialtyId);
        }

        private async Task LoadDoctors(int? selectedDoctorId = null)
        {
            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialty)
                .Select(d => new
                {
                    d.Id,
                    Name = (d.User != null ? d.User.Name : "Bác sĩ ẩn danh") + " - " + 
                            (d.Specialty != null ? d.Specialty.Name : "Chưa rõ khoa")
                })
                .ToListAsync();

            ViewBag.DoctorId = new SelectList(doctors, "Id", "Name", selectedDoctorId);
        }
    }
}