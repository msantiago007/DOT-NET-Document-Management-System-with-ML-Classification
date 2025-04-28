namespace DocumentManagementML.Infrastructure.Settings
{
    public class MLSettings
    {
        public string ModelPath { get; set; } = string.Empty;
        public string ModelDirectory { get; set; } = string.Empty;
        public string TrainingDataPath { get; set; } = string.Empty;
        public float MinimumConfidenceThreshold { get; set; } = 0.7f;
    }
} 