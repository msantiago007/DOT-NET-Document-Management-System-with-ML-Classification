// MLDTOs.cs
namespace DocumentManagementML.Application.DTOs
{
    public class DocumentClassificationResultDto
    {
        public bool Success { get; set; }
        public string? DocumentType { get; set; }
        public float Confidence { get; set; }
        public Dictionary<string, float>? AllPredictions { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class ModelMetricsDto
    {
        public bool Success { get; set; }
        public float MicroAccuracy { get; set; }
        public float MacroAccuracy { get; set; }
        public float LogLoss { get; set; }
        public float LogLossReduction { get; set; }
        public string? ErrorMessage { get; set; }
    }
}