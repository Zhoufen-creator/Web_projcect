namespace DoAnWeb.Services.Interface
{
    public interface ISpecialtyPredictionService
    {
        SpecialtyPredictionResult PredictSpecialty(string? reasonForVisit);
    }
}