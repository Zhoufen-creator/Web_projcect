namespace DoAnWeb.Services
{
    public interface ISpecialtyPredictionService
    {
        SpecialtyPredictionResult PredictSpecialty(string? reasonForVisit);
    }
}