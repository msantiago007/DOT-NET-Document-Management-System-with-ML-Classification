using System;

namespace DocumentManagementML.Application.DTOs
{
    public class DocumentMetadataDto
    {
        public Guid Id { get; set; }
        
        public Guid DocumentId { get; set; }
        
        public string Key { get; set; } = string.Empty;
        
        public string Value { get; set; } = string.Empty;
    }
} 