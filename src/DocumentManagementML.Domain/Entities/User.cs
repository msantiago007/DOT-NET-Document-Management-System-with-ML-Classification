// -----------------------------------------------------------------------------
// <copyright file="User.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            February 22, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        Entity representing a user in the system
// -----------------------------------------------------------------------------
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
        public Guid Id { get; set; }
        
        /// <summary>
        /// Gets or sets the unique identifier for the user (legacy property).
        /// </summary>
        public Guid UserId { 
            get => Id; 
            set => Id = value; 
        }

        /// <summary>
        /// Gets or sets the username used for authentication.
        /// Must be unique within the system.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's email address.
        /// Must be unique within the system.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the hashed password for the user.
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's first name.
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// Gets or sets the user's last name.
        /// </summary>
        public string? LastName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user account is active.
        /// Inactive users cannot access the system.
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Gets or sets a value indicating whether the user is an administrator.
        /// </summary>
        public bool IsAdmin { get; set; } = false;
        
        /// <summary>
        /// Gets or sets the refresh tokens associated with this user.
        /// </summary>
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

        /// <summary>
        /// Gets or sets the date and time when the user account was created.
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the date and time of the user's last login.
        /// Null if the user has never logged in.
        /// </summary>
        public DateTime? LastLoginDate { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the user account was last modified.
        /// </summary>
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

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

        /// <summary>
        /// Gets or sets the collection of documents uploaded by this user.
        /// </summary>
        public ICollection<Document>? UploadedDocuments { get; set; }
    }
}