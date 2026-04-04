namespace DoAnWeb.Services
{
    public class SeasonalAnomalyDetectionOptions
    {
        public bool Enabled { get; set; } = true;
        public string PythonExecutablePath { get; set; } = @".\.env3.11\Scripts\python.exe";
        public string ScriptPath { get; set; } = @"ml\seasonal_isolation_forest.py";
        public int LookbackWeeks { get; set; } = 12;
        public int MinimumHistoryWeeks { get; set; } = 6;
        public int TreeCount { get; set; } = 100;
        public int SampleSize { get; set; } = 16;
        public double Contamination { get; set; } = 0.2;
        public double SurgeMultiplier { get; set; } = 1.2;
        public int MinimumCurrentWeekCases { get; set; } = 4;
    }
}
