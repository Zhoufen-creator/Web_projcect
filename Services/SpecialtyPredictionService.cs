using System.Globalization;
using System.Text;
using DoAnWeb.Services.Interface;

namespace DoAnWeb.Services
{
    public class SpecialtyPredictionService : ISpecialtyPredictionService
    {
        private readonly IPhoBertInferenceService _phoBertInferenceService;

        private readonly Dictionary<string, List<string>> _specialtyKeywords = new()
        {
            { "Noi tong quat", new List<string> { "sot", "ho", "dau hong", "cam", "met moi", "nhuc dau", "kho tho", "viem", "nong", "lanh" } },
            { "Tieu hoa", new List<string> { "dau bung", "tieu chay", "tao bon", "non", "oi", "da day", "kho tieu", "day hoi", "buon non" } },
            { "Tim mach", new List<string> { "dau nguc", "hoi hop", "tim dap nhanh", "kho tho", "cao huyet ap", "tuc nguc", "met tim" } },
            { "Da lieu", new List<string> { "ngua", "noi man", "di ung", "da", "mun", "phat ban", "man do", "viem da", "nam da" } },
            { "Mat", new List<string> { "mo mat", "do mat", "ngua mat", "cay mat", "dau mat", "chay nuoc mat", "nhin mo" } },
            { "Tai mui hong", new List<string> { "dau hong", "so mui", "nghet mui", "ho", "viem hong", "tai", "mui", "hong", "u tai" } },
            { "Xuong khop", new List<string> { "dau lung", "dau goi", "dau khop", "nhuc xuong", "te tay", "te chan", "cot song", "vai gay" } }
        };

        public SpecialtyPredictionService(IPhoBertInferenceService phoBertInferenceService)
        {
            _phoBertInferenceService = phoBertInferenceService;
        }

        public SpecialtyPredictionResult PredictSpecialty(string? reasonForVisit)
        {
            var phoBertResult = _phoBertInferenceService.TryPredictSpecialty(reasonForVisit);
            if (phoBertResult != null)
            {
                return phoBertResult;
            }

            return PredictByRules(reasonForVisit);
        }

        private SpecialtyPredictionResult PredictByRules(string? reasonForVisit)
        {
            if (string.IsNullOrWhiteSpace(reasonForVisit))
            {
                return new SpecialtyPredictionResult
                {
                    PredictedSpecialty = string.Empty,
                    MatchScore = 0,
                    Message = "Chua co mo ta trieu chung de du doan chuyen khoa."
                };
            }

            var normalizedText = NormalizeText(reasonForVisit);

            string bestSpecialty = string.Empty;
            int bestScore = 0;
            List<string> bestMatchedKeywords = new();

            foreach (var item in _specialtyKeywords)
            {
                var matchedKeywords = item.Value
                    .Where(keyword => normalizedText.Contains(keyword))
                    .Distinct()
                    .ToList();

                var score = matchedKeywords.Count;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestSpecialty = item.Key;
                    bestMatchedKeywords = matchedKeywords;
                }
            }

            if (bestScore == 0)
            {
                return new SpecialtyPredictionResult
                {
                    PredictedSpecialty = "Noi tong quat",
                    MatchScore = 0,
                    MatchedKeywords = new List<string>(),
                    Message = "Khong du tu khoa ro rang. He thong tam goi y Noi tong quat."
                };
            }

            return new SpecialtyPredictionResult
            {
                PredictedSpecialty = bestSpecialty,
                MatchScore = bestScore,
                MatchedKeywords = bestMatchedKeywords,
                Message = $"He thong goi y chuyen khoa: {bestSpecialty}."
            };
        }

        private static string NormalizeText(string input)
        {
            input = input.ToLowerInvariant().Trim().Replace('đ', 'd');

            var normalized = input.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(normalized.Length);

            foreach (var c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(c);
                }
            }

            return builder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
