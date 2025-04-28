// src/DocumentManagementML.Domain/Entities/Tag.cs
using System.Collections.Generic;

namespace DocumentManagementML.Domain.Entities
{
    /// <summary>
    /// Represents a tag that can be applied to documents for categorization and search.
    /// Tags provide a flexible way to organize documents across different document types.
    /// </summary>
    /// <remarks>
    /// Author: Marco Alejandro Santiago
    /// Created: February 22, 2025
    /// </remarks>
    public class Tag
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Tag"/> class.
        /// </summary>
        public Tag()
        {
            // Initialize collections
            Documents = new List<Document>();
            
            // Set default values
            TagName = string.Empty;
            Description = string.Empty;
            IsActive = true;
        }
        
        /// <summary>
        /// Gets or sets the unique identifier for the tag.
        /// </summary>
        public int TagId { get; set; }

        /// <summary>
        /// Gets or sets the name of the tag.
        /// Must be unique within the system.
        /// </summary>
        public string TagName { get; set; }

        /// <summary>
        /// Gets or sets the description of the tag.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the tag is active.
        /// Inactive tags cannot be assigned to new documents.
        /// </summary>
        public bool IsActive { get; set; }

        // Navigation properties

        /// <summary>
        /// Gets or sets the collection of documents associated with this tag.
        /// </summary>
        public ICollection<Document> Documents { get; set; }
    }
}