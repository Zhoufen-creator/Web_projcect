using DoAnWeb.Services.Interface;
using System.Text.Json;

namespace DoAnWeb.Services
{
    /// <summary>
    /// Service để inference PhoBERT model từ Python API
    /// </summary>
    public class PhoBertInferenceService : IPhoBertInferenceService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PhoBertInferenceService> _logger;
        private readonly string _pythonApiUrl;

        public PhoBertInferenceService(HttpClient httpClient, ILogger<PhoBertInferenceService> logger, IConfiguration config)
        {
            _httpClient = httpClient;
            _logger = logger;
            _pythonApiUrl = config.GetValue<string>("PhoBertApi:Url") ?? "http://localhost:5000";
        }

        /// <summary>
        /// Gọi Python API để dự đoán chuyên khoa từ triệu chứng sử dụng PhoBERT
        /// </summary>
        public async Task<SpecialtyPredictionResult> PredictSpecialtyAsync(string? reasonForVisit)
        {
            if (string.IsNullOrWhiteSpace(reasonForVisit))
            {
                return new SpecialtyPredictionResult
                {
                    PredictedSpecialty = string.Empty,
                    MatchScore = 0,
                    Message = "Chưa có mô tả triệu chứng để dự đoán chuyên khoa."
                };
            }

            try
            {
                var request = new { text = reasonForVisit };
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(request),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync($"{_pythonApiUrl}/predict", jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"PhoBERT API returned status {response.StatusCode}. Falling back to default.");
                    return new SpecialtyPredictionResult
                    {
                        PredictedSpecialty = "Nội tổng quát",
                        MatchScore = 0,
                        Message = "Hệ thống không thể dự đoán chuyên khoa. Gợi ý mặc định: Nội tổng quát."
                    };
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, object>>(content);

                return new SpecialtyPredictionResult
                {
                    PredictedSpecialty = result?["specialty"]?.ToString() ?? "Nội tổng quát",
                    MatchScore = int.TryParse(result?["confidence"]?.ToString(), out var score) ? score : 0,
                    Message = result?["message"]?.ToString() ?? $"PhoBERT gợi ý chuyên khoa: {result?["specialty"]}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling PhoBERT API");
                return new SpecialtyPredictionResult
                {
                    PredictedSpecialty = "Nội tổng quát",
                    MatchScore = 0,
                    Message = "Lỗi komunication với mô hình PhoBERT. Gợi ý mặc định: Nội tổng quát."
                };
            }
        }
    }
}
