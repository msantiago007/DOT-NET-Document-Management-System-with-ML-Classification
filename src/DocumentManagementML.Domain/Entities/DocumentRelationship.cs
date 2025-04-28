// src/DocumentManagementML.Domain/Entities/DocumentRelationship.cs
using System;

namespace DocumentManagementML.Domain.Entities
{
    /// <summary>
    /// Represents a relationship between two documents.
    /// Relationships define how documents are connected (e.g., references, relates to, depends on).
    /// </summary>
    /// <remarks>
    /// Author: Marco Alejandro Santiago
    /// Created: February 22, 2025
    /// </remarks>
    public class DocumentRelationship
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentRelationship"/> class.
        /// </summary>
        public DocumentRelationship()
        {
            // Set default values
            RelationshipType = string.Empty;
            RelationshipMetadata = string.Empty;
            CreatedDate = DateTime.UtcNow;
            IsActive = true;
        }
        
        /// <summary>
        /// Gets or sets the unique identifier for the document relationship.
        /// </summary>
        public int RelationshipId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the source document in the relationship.
        /// </summary>
        public int SourceDocumentId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the target document in the relationship.
        /// </summary>
        public int TargetDocumentId { get; set; }

        /// <summary>
        /// Gets or sets the type of relationship (e.g., references, relates to, depends on).
        /// </summary>
        public string RelationshipType { get; set; }

        /// <summary>
        /// Gets or sets additional metadata about the relationship in JSON format.
        /// </summary>
        public string RelationshipMetadata { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the relationship was created.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who created this relationship.
        /// </summary>
        public int CreatedById { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the relationship is active.
        /// </summary>
        public bool IsActive { get; set; }

        // Navigation properties

        /// <summary>
        /// Gets or sets the source document in the relationship.
        /// </summary>
        public Document? SourceDocument { get; set; }

        /// <summary>
        /// Gets or sets the target document in the relationship.
        /// </summary>
        public Document? TargetDocument { get; set; }

        /// <summary>
        /// Gets or sets the user who created this relationship.
        /// </summary>
        public User? CreatedBy { get; set; }
    }
}