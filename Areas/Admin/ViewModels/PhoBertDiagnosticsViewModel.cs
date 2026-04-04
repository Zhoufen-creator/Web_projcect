using DoAnWeb.Services;

namespace DoAnWeb.Areas.Admin.ViewModels
{
    public class PhoBertDiagnosticsViewModel
    {
        public string InputText { get; set; } = string.Empty;
        public PhoBertApiHealthResult? Health { get; set; }
        public SpecialtyPredictionResult? Prediction { get; set; }
    }
}
