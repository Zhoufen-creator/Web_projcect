namespace DoAnWeb.Services.Interface
{
    public interface ISpecialtyLoadAnalysisService
    {
        Task<List<SpecialtyLoadInsight>> AnalyzeSpecialtyLoadsAsync();
    }
}