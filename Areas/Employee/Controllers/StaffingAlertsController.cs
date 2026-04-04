using DoAnWeb.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAnWeb.Areas.Employee.Controllers
{
    [Area("Employee")]
    [Authorize(Roles = "Employee,Admin")]
    public class StaffingAlertsController : Controller
    {
        private readonly ISeasonalStaffingDetectionService _seasonalStaffingDetectionService;

        public StaffingAlertsController(ISeasonalStaffingDetectionService seasonalStaffingDetectionService)
        {
            _seasonalStaffingDetectionService = seasonalStaffingDetectionService;
        }

        public async Task<IActionResult> Index()
        {
            var alerts = await _seasonalStaffingDetectionService.DetectAsync();
            await _seasonalStaffingDetectionService.DetectAndNotifyEmployeesAsync();

            return View(alerts);
        }
    }
}
