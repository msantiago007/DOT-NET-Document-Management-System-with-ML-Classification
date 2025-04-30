// -----------------------------------------------------------------------------
// <copyright file="DocumentType.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            February 22, 2025
// Last Modified:      April 29, 2025
// Version:            0.9.0
// Description:        Defines document type entities that categorize documents
// -----------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace DocumentManagementML.Domain.Entities
{
    /// <summary>
    /// Represents a type or category of document within the system.
    /// Document types define the schema and expected metadata for documents.
    /// </summary>
    /// <remarks>
    /// Author: Marco Santiago
    /// Created: February 22, 2025
    /// Modified: April 29, 2025 - Added TypeName property for system identification
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
            TypeName = string.Empty;
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
        /// Gets or sets the type name (identifier) of the document type.
        /// This is a system identifier used for classification and automation.
        /// </summary>
        public string TypeName { get; set; } = string.Empty;

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