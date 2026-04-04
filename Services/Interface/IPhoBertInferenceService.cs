namespace DoAnWeb.Services.Interface
{
    public interface IPhoBertInferenceService
    {
        PhoBertApiHealthResult CheckHealth();
        SpecialtyPredictionResult? TryPredictSpecialty(string? reasonForVisit);
    }
}
