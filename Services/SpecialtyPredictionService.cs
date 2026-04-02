using System.Text;

namespace DoAnWeb.Services
{
    public class SpecialtyPredictionService : ISpecialtyPredictionService
    {
        private readonly Dictionary<string, List<string>> _specialtyKeywords = new()
        {
            { "Nội tổng quát", new List<string> { "sot", "ho", "dau hong", "cam", "met moi", "nhuc dau", "kho tho", "viem", "nong", "lanh" } },
            { "Tiêu hóa", new List<string> { "dau bung", "tieu chay", "tao bon", "non", "oi", "da day", "kho tieu", "day hoi", "buon non" } },
            { "Tim mạch", new List<string> { "dau nguc", "hoi hop", "tim dap nhanh", "kho tho", "cao huyet ap", "tuc nguc", "met tim" } },
            { "Da liễu", new List<string> { "ngua", "noi man", "di ung", "da", "mun", "phat ban", "man do", "viem da", "nam da" } },
            { "Mắt", new List<string> { "mo mat", "do mat", "ngua mat", "cay mat", "dau mat", "chay nuoc mat", "nhin mo" } },
            { "Tai mũi họng", new List<string> { "dau hong", "so mui", "nghet mui", "ho", "viem hong", "tai", "mui", "hong", "u tai" } },
            { "Xương khớp", new List<string> { "dau lung", "dau goi", "dau khop", "nhuc xuong", "te tay", "te chan", "cot song", "vai gay" } }
        };

        public SpecialtyPredictionResult PredictSpecialty(string? reasonForVisit)
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
                    PredictedSpecialty = "Nội tổng quát",
                    MatchScore = 0,
                    MatchedKeywords = new List<string>(),
                    Message = "Không đủ từ khóa rõ ràng. Hệ thống tạm gợi ý Nội tổng quát."
                };
            }

            return new SpecialtyPredictionResult
            {
                PredictedSpecialty = bestSpecialty,
                MatchScore = bestScore,
                MatchedKeywords = bestMatchedKeywords,
                Message = $"Hệ thống gợi ý chuyên khoa: {bestSpecialty}."
            };
        }

        private static string NormalizeText(string input)
        {
            input = input.ToLowerInvariant().Trim();

            var replacements = new Dictionary<string, string>
            {
                { "á", "a" }, { "à", "a" }, { "ả", "a" }, { "ã", "a" }, { "ạ", "a" },
                { "ă", "a" }, { "ắ", "a" }, { "ằ", "a" }, { "ẳ", "a" }, { "ẵ", "a" }, { "ặ", "a" },
                { "â", "a" }, { "ấ", "a" }, { "ầ", "a" }, { "ẩ", "a" }, { "ẫ", "a" }, { "ậ", "a" },

                { "é", "e" }, { "è", "e" }, { "ẻ", "e" }, { "ẽ", "e" }, { "ẹ", "e" },
                { "ê", "e" }, { "ế", "e" }, { "ề", "e" }, { "ể", "e" }, { "ễ", "e" }, { "ệ", "e" },

                { "í", "i" }, { "ì", "i" }, { "ỉ", "i" }, { "ĩ", "i" }, { "ị", "i" },

                { "ó", "o" }, { "ò", "o" }, { "ỏ", "o" }, { "õ", "o" }, { "ọ", "o" },
                { "ô", "o" }, { "ố", "o" }, { "ồ", "o" }, { "ổ", "o" }, { "ỗ", "o" }, { "ộ", "o" },
                { "ơ", "o" }, { "ớ", "o" }, { "ờ", "o" }, { "ở", "o" }, { "ỡ", "o" }, { "ợ", "o" },

                { "ú", "u" }, { "ù", "u" }, { "ủ", "u" }, { "ũ", "u" }, { "ụ", "u" },
                { "ư", "u" }, { "ứ", "u" }, { "ừ", "u" }, { "ử", "u" }, { "ữ", "u" }, { "ự", "u" },

                { "ý", "y" }, { "ỳ", "y" }, { "ỷ", "y" }, { "ỹ", "y" }, { "ỵ", "y" },
                { "đ", "d" }
            };

            var builder = new StringBuilder(input);
            foreach (var replacement in replacements)
            {
                builder.Replace(replacement.Key, replacement.Value);
            }

            return builder.ToString();
        }
    }
}