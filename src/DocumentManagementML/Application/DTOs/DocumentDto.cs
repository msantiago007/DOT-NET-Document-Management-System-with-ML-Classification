using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DocumentManagementML.Application.DTOs
{
    public class DocumentDto
    {
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        public string FilePath { get; set; } = string.Empty;
        
        public string FileType { get; set; } = string.Empty;
        
        public long FileSize { get; set; }
        
        public DateTime UploadDate { get; set; }
        
        public DateTime? LastModifiedDate { get; set; }
        
        public Guid? DocumentTypeId { get; set; }
        
        public string? DocumentTypeName { get; set; }
        
        public double? ClassificationConfidence { get; set; }
        
        public Guid? UploadedById { get; set; }
        
        public string? UploadedByName { get; set; }
        
        public Dictionary<string, string>? Metadata { get; set; }
    }
    
    public class DocumentCreateDto
    {
        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        public Guid? DocumentTypeId { get; set; }
        
        public Dictionary<string, string>? Metadata { get; set; }
    }
    
    public class DocumentUpdateDto
    {
        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        public Guid? DocumentTypeId { get; set; }
        
        public Dictionary<string, string>? Metadata { get; set; }
    }
} 