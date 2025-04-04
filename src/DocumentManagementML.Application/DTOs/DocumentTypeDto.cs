// DocumentTypeDto.cs
namespace DocumentManagementML.Application.DTOs
{
    public class DocumentTypeDto
    {
        public int DocumentTypeId { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? SchemaDefinition { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
    }

    public class DocumentTypeCreateDto
    {
        [Required]
        [StringLength(100)]
        public string TypeName { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public string? SchemaDefinition { get; set; }
    }

    public class DocumentTypeUpdateDto
    {
        [StringLength(100)]
        public string? TypeName { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public string? SchemaDefinition { get; set; }
        
        public bool? IsActive { get; set; }
    }
}