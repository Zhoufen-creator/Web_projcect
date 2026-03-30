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
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Danh sách lịch hẹn
        public async Task<IActionResult> Index()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .OrderByDescending(a => a.ScheduledDate)
                .ToListAsync();

            return View(appointments);
        }

        // GET: Edit
        public async Task<IActionResult> Edit(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound();

            var vm = new AppointmentAssignVM
            {
                Id = appointment.Id,
                PatientName = appointment.Patient.User.Name,
                DoctorId = appointment.DoctorId,
                ScheduledDate = appointment.ScheduledDate,
                ReasonForVisit = appointment.ReasonForVisit,
                Status = appointment.Status
            };

            await LoadDoctors(vm.DoctorId);
            return View(vm);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AppointmentAssignVM vm)
        {
            if (id != vm.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadDoctors(vm.DoctorId);
                return View(vm);
            }

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.DoctorId = vm.DoctorId;
            appointment.ScheduledDate = vm.ScheduledDate;
            appointment.Status = vm.Status;

            _context.Update(appointment);
            await _context.SaveChangesAsync();

            TempData["success"] = "Cập nhật lịch hẹn thành công!";
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