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
} 