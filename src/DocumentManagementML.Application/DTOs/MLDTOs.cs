// MLDTOs.cs

using System;
using System.Collections.Generic;

namespace DocumentManagementML.Application.DTOs
{
    public class DocumentClassificationResultDto
    {
        public Guid DocumentId { get; set; }
        
        public string DocumentName { get; set; } = string.Empty;
        
        public Guid? PredictedDocumentTypeId { get; set; }
        
        public string? PredictedDocumentTypeName { get; set; }
        
        public double Confidence { get; set; }
        
        public List<DocumentTypeScoreDto>? AllPredictions { get; set; }
        
        public bool IsSuccessful { get; set; }
        
        public string? ErrorMessage { get; set; }
    }
    
    public class DocumentTypeScoreDto
    {
        public Guid DocumentTypeId { get; set; }
        
        public string DocumentTypeName { get; set; } = string.Empty;
        
        public double Score { get; set; }
    }
    
    public class ClassificationRequestDto
    {
        public Guid DocumentId { get; set; }
        
        public string FilePath { get; set; } = string.Empty;
        
        public string FileType { get; set; } = string.Empty;
    }

    public class ModelMetricsDto
    {
        public Guid Id { get; set; }
        
        public string ModelName { get; set; } = string.Empty;
        
        public string ModelVersion { get; set; } = string.Empty;
        
        public double Accuracy { get; set; }
        
        public double Precision { get; set; }
        
        public double Recall { get; set; }
        
        public double F1Score { get; set; }
        
        public int TotalSamples { get; set; }
        
        public DateTime TrainingDate { get; set; }
        
        public Dictionary<string, double> ClassMetrics { get; set; } = new Dictionary<string, double>();
        
        // Keep these properties for backward compatibility
        public bool Success { get; set; }
        public float MicroAccuracy { get; set; }
        public float MacroAccuracy { get; set; }
        public float LogLoss { get; set; }
        public float LogLossReduction { get; set; }
        public string? ErrorMessage { get; set; }
    }
}