// src/DocumentManagementML.Domain/Entities/Document.cs
using System;
using System.Collections.Generic;

namespace DocumentManagementML.Domain.Entities
{
    /// <summary>
    /// Represents a document in the document management system.
    /// Documents are the core entity and can have multiple versions, metadata, and relationships.
    /// </summary>
    /// <remarks>
    /// Author: Marco Alejandro Santiago
    /// Created: February 22, 2025
    /// </remarks>
    public class Document
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Document"/> class.
        /// </summary>
        public Document()
        {
            // Initialize collections
            Versions = new List<DocumentVersion>();
            Metadata = new List<DocumentMetadata>();
            SourceRelationships = new List<DocumentRelationship>();
            TargetRelationships = new List<DocumentRelationship>();
            
            // Set default values
            DocumentName = string.Empty;
            FileLocation = string.Empty;
            FileType = string.Empty;
            ContentHash = string.Empty;
            CreatedDate = DateTime.UtcNow;
            LastModifiedDate = DateTime.UtcNow;
            IsDeleted = false;
        }
        
        /// <summary>
        /// Gets or sets the unique identifier for the document.
        /// </summary>
        public int DocumentId { get; set; }

        /// <summary>
        /// Gets or sets the name or title of the document.
        /// </summary>
        public string DocumentName { get; set; }

        /// <summary>
        /// Gets or sets the file location path where the document is stored.
        /// </summary>
        public string FileLocation { get; set; }

        /// <summary>
        /// Gets or sets the file type or extension of the document.
        /// </summary>
        public string FileType { get; set; }

        /// <summary>
        /// Gets or sets the file size in bytes.
        /// </summary>
        public long FileSizeBytes { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the document was created.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who created the document.
        /// </summary>
        public int CreatedById { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the document was last modified.
        /// </summary>
        public DateTime LastModifiedDate { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who last modified the document.
        /// </summary>
        public int LastModifiedById { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the document has been deleted (soft delete).
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets the content hash for integrity verification.
        /// </summary>
        public string ContentHash { get; set; }

        /// <summary>
        /// Gets or sets the ID of the document type this document belongs to.
        /// </summary>
        public int? DocumentTypeId { get; set; }

        // Navigation properties

        /// <summary>
        /// Gets or sets the user who created this document.
        /// </summary>
        public User? CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the user who last modified this document.
        /// </summary>
        public User? ModifiedBy { get; set; }

        /// <summary>
        /// Gets or sets the type of this document.
        /// </summary>
        public DocumentType? DocumentType { get; set; }

        /// <summary>
        /// Gets or sets the collection of versions for this document.
        /// </summary>
        public ICollection<DocumentVersion> Versions { get; set; }

        /// <summary>
        /// Gets or sets the collection of metadata associated with this document.
        /// </summary>
        public ICollection<DocumentMetadata> Metadata { get; set; }

        /// <summary>
        /// Gets or sets the collection of relationships where this document is the source.
        /// </summary>
        public ICollection<DocumentRelationship> SourceRelationships { get; set; }

        /// <summary>
        /// Gets or sets the collection of relationships where this document is the target.
        /// </summary>
        public ICollection<DocumentRelationship> TargetRelationships { get; set; }
    }
}