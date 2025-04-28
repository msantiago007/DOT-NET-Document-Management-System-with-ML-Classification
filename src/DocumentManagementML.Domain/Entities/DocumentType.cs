// src/DocumentManagementML.Domain/Entities/DocumentType.cs
using System;
using System.Collections.Generic;

namespace DocumentManagementML.Domain.Entities
{
    /// <summary>
    /// Represents a type or category of document within the system.
    /// Document types define the schema and expected metadata for documents.
    /// </summary>
    /// <remarks>
    /// Author: Marco Alejandro Santiago
    /// Created: February 22, 2025
    /// </remarks>
    public class DocumentType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentType"/> class.
        /// </summary>
        public DocumentType()
        {
            // Initialize collections
            Documents = new List<Document>();
            
            // Set default values
            Name = string.Empty;
            Description = string.Empty;
            SchemaDefinition = string.Empty;
            CreatedDate = DateTime.UtcNow;
            LastModifiedDate = DateTime.UtcNow;
            IsActive = true;
        }
        
        /// <summary>
        /// Gets or sets the unique identifier for the document type.
        /// </summary>
        public Guid DocumentTypeId { get; set; }

        /// <summary>
        /// Gets or sets the name of the document type.
        /// Must be unique within the system.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the document type.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the JSON schema definition for the document type.
        /// This defines the expected structure and metadata for documents of this type.
        /// </summary>
        public string SchemaDefinition { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the document type is active.
        /// Inactive document types cannot be assigned to new documents.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the date and time when the document type was created.
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the date and time when the document type was last modified.
        /// </summary>
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties

        /// <summary>
        /// Gets or sets the collection of documents of this type.
        /// </summary>
        public ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}