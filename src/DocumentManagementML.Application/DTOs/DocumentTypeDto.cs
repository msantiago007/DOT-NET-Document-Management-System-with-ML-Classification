// DocumentTypeDto.cs

using System;
using System.ComponentModel.DataAnnotations;

namespace DocumentManagementML.Application.DTOs
{
    public class DocumentTypeDto
    {
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Added for compatibility with tests
        public string TypeName { get; set; } = string.Empty;
    }
    
    public class DocumentTypeCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Added for compatibility with tests
        public string? TypeName { get; set; }
    }
    
    public class DocumentTypeUpdateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}