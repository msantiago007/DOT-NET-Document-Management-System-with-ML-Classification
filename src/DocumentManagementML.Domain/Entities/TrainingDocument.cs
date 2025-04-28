using System;

namespace DocumentManagementML.Domain.Entities
{
    public class TrainingDocument
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
} 