// -----------------------------------------------------------------------------
// <copyright file="Document.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            February 22, 2025
// Last Modified:      April 29, 2025
// Version:            0.9.0
// Description:        Represents a document in the document management system.
//                     Core entity that supports versions, metadata, and relationships.
// -----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

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
            MetadataItems = new List<DocumentMetadata>();
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
        public Guid DocumentId { get; set; }

        /// <summary>
        /// Gets or sets the name or title of the document.
        /// </summary>
        public string DocumentName { get; set; } = string.Empty;

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
        public Guid CreatedById { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the document was last modified.
        /// </summary>
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the ID of the user who last modified the document.
        /// </summary>
        public Guid LastModifiedById { get; set; }

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
        public Guid? DocumentTypeId { get; set; }

        /// <summary>
        /// Gets or sets the file path of the document.
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file size of the document.
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Gets or sets the upload date of the document.
        /// </summary>
        public DateTime UploadDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the classification confidence of the document.
        /// </summary>
        public double? ClassificationConfidence { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who uploaded the document.
        /// </summary>
        public Guid? UploadedById { get; set; }

        /// <summary>
        /// Gets or sets the user who uploaded the document.
        /// </summary>
        public User? UploadedBy { get; set; }

        /// <summary>
        /// Gets or sets the metadata dictionary for the document.
        /// </summary>
        public ICollection<DocumentMetadata> MetadataItems { get; set; } = new List<DocumentMetadata>();
        
        /// <summary>
        /// Gets a dictionary representation of the metadata items.
        /// </summary>
        public Dictionary<string, string> MetadataDictionary 
        { 
            get 
            {
                var dict = new Dictionary<string, string>();
                foreach (var item in MetadataItems)
                {
                    dict[item.MetadataKey] = item.MetadataValue;
                }
                return dict;
            }
        }

        /// <summary>
        /// Gets or sets the description of the document.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the content type of the document.
        /// </summary>
        public string ContentType { get; set; } = string.Empty;

        // Navigation properties

        /// <summary>
        /// Gets or sets the user who created this document.
        /// </summary>
        [NotMapped]
        public User? CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the user who last modified this document.
        /// </summary>
        [NotMapped]
        public User? ModifiedBy { get; set; }

        /// <summary>
        /// Gets or sets the type of this document.
        /// </summary>
        public DocumentType? DocumentType { get; set; }

        /// <summary>
        /// Gets or sets the collection of versions for this document.
        /// </summary>
        public ICollection<DocumentVersion> Versions { get; set; } = new List<DocumentVersion>();

        /// <summary>
        /// Gets or sets the collection of relationships where this document is the source.
        /// This property is not mapped to the database directly but managed through
        /// explicit relationship configuration in DbContext.
        /// </summary>
        [NotMapped]
        public ICollection<DocumentRelationship> SourceRelationships { get; set; } = new List<DocumentRelationship>();

        /// <summary>
        /// Gets or sets the collection of relationships where this document is the target.
        /// This property is not mapped to the database directly but managed through
        /// explicit relationship configuration in DbContext.
        /// </summary>
        [NotMapped]
        public ICollection<DocumentRelationship> TargetRelationships { get; set; } = new List<DocumentRelationship>();
    }
}