// src/DocumentManagementML.Domain/Entities/DocumentVersion.cs
using System;

namespace DocumentManagementML.Domain.Entities
{
    /// <summary>
    /// Represents a specific version of a document.
    /// Each time a document is updated, a new version is created to maintain history.
    /// </summary>
    /// <remarks>
    /// Author: Marco Alejandro Santiago
    /// Created: February 22, 2025
    /// </remarks>
    public class DocumentVersion
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentVersion"/> class.
        /// </summary>
        public DocumentVersion()
        {
            // Set default values
            FileLocation = string.Empty;
            ContentHash = string.Empty;
            CreatedDate = DateTime.UtcNow;
        }
        
        /// <summary>
        /// Gets or sets the unique identifier for the document version.
        /// </summary>
        public int VersionId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the document this version belongs to.
        /// </summary>
        public int DocumentId { get; set; }

        /// <summary>
        /// Gets or sets the version number, which typically increments with each new version.
        /// </summary>
        public int VersionNumber { get; set; }

        /// <summary>
        /// Gets or sets the file location path where this version of the document is stored.
        /// </summary>
        public string FileLocation { get; set; }

        /// <summary>
        /// Gets or sets the date and time when this version was created.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who created this version.
        /// </summary>
        public int CreatedById { get; set; }

        /// <summary>
        /// Gets or sets the content hash for integrity verification.
        /// </summary>
        public string ContentHash { get; set; }

        // Navigation properties

        /// <summary>
        /// Gets or sets the document this version belongs to.
        /// </summary>
        public Document? Document { get; set; }

        /// <summary>
        /// Gets or sets the user who created this version.
        /// </summary>
        public User? CreatedBy { get; set; }
    }
}