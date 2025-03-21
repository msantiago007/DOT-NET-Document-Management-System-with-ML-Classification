// src/DocumentManagementML.Domain/Entities/User.cs
using System;
using System.Collections.Generic;

namespace DocumentManagementML.Domain.Entities
{
    /// <summary>
    /// Represents a user within the document management system.
    /// Users can create, modify, and access documents based on their permissions.
    /// </summary>
    /// <remarks>
    /// Author: Marco Alejandro Santiago
    /// Created: February 22, 2025
    /// </remarks>
    public class User
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        public User()
        {
            // Initialize collections
            CreatedDocuments = new List<Document>();
            ModifiedDocuments = new List<Document>();
            CreatedVersions = new List<DocumentVersion>();
            CreatedRelationships = new List<DocumentRelationship>();
            
            // Set default values
            Username = string.Empty;
            Email = string.Empty;
            PasswordHash = string.Empty;
            CreatedDate = DateTime.UtcNow;
            IsActive = true;
        }

        /// <summary>
        /// Gets or sets the unique identifier for the user.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the username used for authentication.
        /// Must be unique within the system.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the user's email address.
        /// Must be unique within the system.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the hashed password for the user.
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user account is active.
        /// Inactive users cannot access the system.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the user account was created.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the date and time of the user's last login.
        /// Null if the user has never logged in.
        /// </summary>
        public DateTime? LastLoginDate { get; set; }

        // Navigation properties

        /// <summary>
        /// Gets or sets the collection of documents created by this user.
        /// </summary>
        public ICollection<Document> CreatedDocuments { get; set; }

        /// <summary>
        /// Gets or sets the collection of documents last modified by this user.
        /// </summary>
        public ICollection<Document> ModifiedDocuments { get; set; }

        /// <summary>
        /// Gets or sets the collection of document versions created by this user.
        /// </summary>
        public ICollection<DocumentVersion> CreatedVersions { get; set; }

        /// <summary>
        /// Gets or sets the collection of document relationships created by this user.
        /// </summary>
        public ICollection<DocumentRelationship> CreatedRelationships { get; set; }
    }
}