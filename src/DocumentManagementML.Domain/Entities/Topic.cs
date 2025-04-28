// src/DocumentManagementML.Domain/Entities/Topic.cs
using System;
using System.Collections.Generic;

namespace DocumentManagementML.Domain.Entities
{
    /// <summary>
    /// Represents a topic or category for organizing documents.
    /// Topics can have hierarchical relationships (parent-child).
    /// </summary>
    /// <remarks>
    /// Author: Marco Alejandro Santiago
    /// Created: February 22, 2025
    /// </remarks>
    public class Topic
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Topic"/> class.
        /// </summary>
        public Topic()
        {
            // Initialize collections
            ChildTopics = new List<Topic>();
            Documents = new List<Document>();
            
            // Set default values
            TopicName = string.Empty;
            Description = string.Empty;
            CreatedDate = DateTime.UtcNow;
            IsActive = true;
        }
        
        /// <summary>
        /// Gets or sets the unique identifier for the topic.
        /// </summary>
        public int TopicId { get; set; }

        /// <summary>
        /// Gets or sets the name of the topic.
        /// Must be unique within the system.
        /// </summary>
        public string TopicName { get; set; }

        /// <summary>
        /// Gets or sets the description of the topic.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the topic is active.
        /// Inactive topics cannot be assigned to new documents.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the topic was created.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the ID of the parent topic, if this is a child topic.
        /// Null if this is a top-level topic.
        /// </summary>
        public int? ParentTopicId { get; set; }

        // Navigation properties

        /// <summary>
        /// Gets or sets the parent topic, if this is a child topic.
        /// </summary>
        public Topic? ParentTopic { get; set; }

        /// <summary>
        /// Gets or sets the collection of child topics under this topic.
        /// </summary>
        public ICollection<Topic> ChildTopics { get; set; }

        /// <summary>
        /// Gets or sets the collection of documents associated with this topic.
        /// </summary>
        public ICollection<Document> Documents { get; set; }
    }
}