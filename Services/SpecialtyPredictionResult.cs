namespace DoAnWeb.Services
{
    public class SpecialtyPredictionResult
    {
        public string PredictedSpecialty { get; set; } = string.Empty;
        public int MatchScore { get; set; }
        public List<string> MatchedKeywords { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }
}