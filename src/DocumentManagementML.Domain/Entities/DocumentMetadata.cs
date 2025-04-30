// -----------------------------------------------------------------------------
// <copyright file="DocumentMetadata.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            February 22, 2025
// Last Modified:      April 29, 2025
// Version:            0.9.0
// Description:        Represents metadata associated with documents as key-value
//                     pairs for storing additional document information.
// -----------------------------------------------------------------------------
using System;

namespace DocumentManagementML.Domain.Entities
{
    /// <summary>
    /// Represents metadata associated with a document.
    /// Metadata consists of key-value pairs that provide additional information about a document.
    /// </summary>
    /// <remarks>
    /// Author: Marco Alejandro Santiago
    /// Created: February 22, 2025
    /// </remarks>
    public class DocumentMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentMetadata"/> class.
        /// </summary>
        public DocumentMetadata()
        {
            // Set default values
            MetadataKey = string.Empty;
            MetadataValue = string.Empty;
            DataType = "string";
            CreatedDate = DateTime.UtcNow;
            LastModifiedDate = DateTime.UtcNow;
        }
        
        /// <summary>
        /// Gets or sets the unique identifier for the document metadata.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the document this metadata belongs to.
        /// </summary>
        public Guid DocumentId { get; set; }

        /// <summary>
        /// Gets or sets the key or name of the metadata field.
        /// </summary>
        public string MetadataKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the value of the metadata field.
        /// </summary>
        public string MetadataValue { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the data type of the metadata value (e.g., string, number, date, boolean, json).
        /// </summary>
        public string? DataType { get; set; }

        /// <summary>
        /// Gets or sets the date and time when this metadata was created.
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the date and time when this metadata was last modified.
        /// </summary>
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the key for simpler access in Document.MetadataDictionary.
        /// </summary>
        public string Key => MetadataKey;

        /// <summary>
        /// Gets or sets the value for simpler access in Document.MetadataDictionary.
        /// </summary>
        public string Value => MetadataValue;

        // Navigation properties

        /// <summary>
        /// Gets or sets the document this metadata belongs to.
        /// </summary>
        public Document? Document { get; set; }
    }
}