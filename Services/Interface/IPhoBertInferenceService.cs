namespace DoAnWeb.Services.Interface
{
    public interface IPhoBertInferenceService
    {
        /// <summary>
        /// Gọi PhoBERT model để dự đoán chuyên khoa từ triệu chứng
        /// </summary>
        Task<SpecialtyPredictionResult> PredictSpecialtyAsync(string? reasonForVisit);
    }
}
