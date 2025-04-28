using System;
using System.Collections.Generic;

namespace DocumentManagementML.Domain.Entities
{
    public class ClassificationResult
    {
        public Guid DocumentId { get; set; }
        public string DocumentName { get; set; } = string.Empty;
        
        public Guid? PredictedDocumentTypeId { get; set; }
        public DocumentType? PredictedDocumentType { get; set; }
        
        public double Confidence { get; set; }
        
        public List<DocumentTypeScore>? AllPredictions { get; set; }
        
        public bool IsSuccessful { get; set; }
        public string? ErrorMessage { get; set; }
    }
    
    public class DocumentTypeScore
    {
        public Guid DocumentTypeId { get; set; }
        public string DocumentTypeName { get; set; } = string.Empty;
        public double Score { get; set; }
    }
} 