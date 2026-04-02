namespace DoAnWeb.Services
{
    public interface ISpecialtyLoadAnalysisService
    {
        Task<List<SpecialtyLoadInsight>> AnalyzeSpecialtyLoadsAsync();
    }
}