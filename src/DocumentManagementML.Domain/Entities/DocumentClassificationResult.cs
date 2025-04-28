using System;
using System.Collections.Generic;

namespace DocumentManagementML.Domain.Entities
{
    public class DocumentClassificationResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public float Confidence { get; set; }
        public Dictionary<string, float> AllPredictions { get; set; } = new Dictionary<string, float>();
        public Guid? DocumentId { get; set; }
        public string DocumentName { get; set; } = string.Empty;
    }
} 