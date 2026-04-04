using System.Net.Http.Json;
using DoAnWeb.Services.Interface;
using Microsoft.Extensions.Options;

namespace DoAnWeb.Services
{
    public class PhoBertInferenceService : IPhoBertInferenceService
    {
        private readonly HttpClient _httpClient;
        private readonly PhoBertApiOptions _options;
        private readonly ILogger<PhoBertInferenceService> _logger;

        public PhoBertInferenceService(
            HttpClient httpClient,
            IOptions<PhoBertApiOptions> options,
            ILogger<PhoBertInferenceService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;

            if (!string.IsNullOrWhiteSpace(_options.Url))
            {
                _httpClient.BaseAddress = new Uri(_options.Url);
            }

            _httpClient.Timeout = TimeSpan.FromSeconds(Math.Max(1, _options.TimeoutSeconds));
        }

        public SpecialtyPredictionResult? TryPredictSpecialty(string? reasonForVisit)
        {
            if (!_options.Enabled || string.IsNullOrWhiteSpace(reasonForVisit))
            {
                return null;
            }

            try
            {
                var response = _httpClient
                    .PostAsJsonAsync("/predict", new PhoBertPredictRequest
                    {
                        Text = reasonForVisit,
                        TopK = _options.TopK
                    })
                    .GetAwaiter()
                    .GetResult();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("PhoBERT API returned status code {StatusCode}", response.StatusCode);
                    return null;
                }

                var result = response.Content
                    .ReadFromJsonAsync<PhoBertPredictResponse>()
                    .GetAwaiter()
                    .GetResult();

                if (result == null || string.IsNullOrWhiteSpace(result.PredictedLabel))
                {
                    return null;
                }

                return new SpecialtyPredictionResult
                {
                    PredictedSpecialty = result.PredictedLabel,
                    MatchScore = ConvertConfidenceToScore(result.Confidence),
                    MatchedKeywords = result.TopK?
                        .Select(item => $"{item.Label}:{item.Probability:0.000}")
                        .ToList() ?? new List<string>(),
                    Message = $"PhoBERT goi y chuyen khoa: {result.PredictedLabel}."
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "PhoBERT API unavailable. Falling back to rule-based prediction.");
                return null;
            }
        }

        public PhoBertApiHealthResult CheckHealth()
        {
            var result = new PhoBertApiHealthResult
            {
                Enabled = _options.Enabled,
                Url = _options.Url,
                TimeoutSeconds = _options.TimeoutSeconds
            };

            if (!_options.Enabled)
            {
                result.IsHealthy = false;
                result.Message = "PhoBERT API dang tat trong appsettings.";
                return result;
            }

            try
            {
                var response = _httpClient
                    .GetAsync("/health")
                    .GetAwaiter()
                    .GetResult();

                if (!response.IsSuccessStatusCode)
                {
                    result.IsHealthy = false;
                    result.Message = $"PhoBERT API tra ve status code {(int)response.StatusCode}.";
                    return result;
                }

                result.IsHealthy = true;
                result.Message = "PhoBERT API dang san sang.";
                return result;
            }
            catch (Exception ex)
            {
                result.IsHealthy = false;
                result.Message = $"Khong the ket noi PhoBERT API: {ex.Message}";
                return result;
            }
        }

        private static int ConvertConfidenceToScore(double confidence)
        {
            return (int)Math.Round(Math.Clamp(confidence, 0d, 1d) * 100d);
        }

        private sealed class PhoBertPredictRequest
        {
            public string Text { get; set; } = string.Empty;
            public int TopK { get; set; } = 3;
        }

        private sealed class PhoBertPredictResponse
        {
            public string InputText { get; set; } = string.Empty;
            public string PredictedLabel { get; set; } = string.Empty;
            public int PredictedLabelId { get; set; }
            public double Confidence { get; set; }
            public List<PhoBertTopPrediction>? TopK { get; set; }
        }

        private sealed class PhoBertTopPrediction
        {
            public int LabelId { get; set; }
            public string Label { get; set; } = string.Empty;
            public double Probability { get; set; }
        }
    }
}
