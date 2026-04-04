using DoAnWeb.Areas.Admin.ViewModels;
using DoAnWeb.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAnWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PhoBertDiagnosticsController : Controller
    {
        private readonly IPhoBertInferenceService _phoBertInferenceService;
        private readonly ISpecialtyPredictionService _specialtyPredictionService;

        public PhoBertDiagnosticsController(
            IPhoBertInferenceService phoBertInferenceService,
            ISpecialtyPredictionService specialtyPredictionService)
        {
            _phoBertInferenceService = phoBertInferenceService;
            _specialtyPredictionService = specialtyPredictionService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = new PhoBertDiagnosticsViewModel
            {
                Health = _phoBertInferenceService.CheckHealth()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(PhoBertDiagnosticsViewModel model)
        {
            model.Health = _phoBertInferenceService.CheckHealth();

            if (!string.IsNullOrWhiteSpace(model.InputText))
            {
                model.Prediction = _specialtyPredictionService.PredictSpecialty(model.InputText);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Status()
        {
            var result = _phoBertInferenceService.CheckHealth();
            return Json(result);
        }

        [HttpPost]
        public IActionResult Predict(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return BadRequest(new { error = "Text không được để trống." });
            }

            var result = _specialtyPredictionService.PredictSpecialty(text);
            return Json(new
            {
                input = text,
                prediction = result
            });
        }
    }
}
