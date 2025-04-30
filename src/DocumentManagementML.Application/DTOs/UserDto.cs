// -----------------------------------------------------------------------------
// <copyright file="UserDto.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            February 22, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        Data transfer objects for user operations
// -----------------------------------------------------------------------------
using System;
using System.ComponentModel.DataAnnotations;

namespace DocumentManagementML.Application.DTOs
{
    /// <summary>
    /// Data transfer object for user information
    /// </summary>
    public class UserDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Gets or sets the username
        /// </summary>
        public string Username { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the user's email
        /// </summary>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the user's first name
        /// </summary>
        public string? FirstName { get; set; }
        
        /// <summary>
        /// Gets or sets the user's last name
        /// </summary>
        public string? LastName { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the user is active
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the user is an administrator
        /// </summary>
        public bool IsAdmin { get; set; }
        
        /// <summary>
        /// Gets or sets the date the user was created
        /// </summary>
        public DateTime CreatedDate { get; set; }
        
        /// <summary>
        /// Gets or sets the date the user last logged in
        /// </summary>
        public DateTime? LastLoginDate { get; set; }
    }
    
    /// <summary>
    /// Data transfer object for creating users
    /// </summary>
    public class UserCreateDto
    {
        /// <summary>
        /// Gets or sets the username
        /// </summary>
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the email
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the password
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$", 
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character")]
        public string Password { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the first name
        /// </summary>
        [StringLength(50)]
        public string? FirstName { get; set; }
        
        /// <summary>
        /// Gets or sets the last name
        /// </summary>
        [StringLength(50)]
        public string? LastName { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the user is an administrator
        /// </summary>
        public bool IsAdmin { get; set; } = false;
    }
    
    /// <summary>
    /// Data transfer object for updating users
    /// </summary>
    public class UserUpdateDto
    {
        /// <summary>
        /// Gets or sets the username
        /// </summary>
        [StringLength(50, MinimumLength = 3)]
        public string? Username { get; set; }
        
        /// <summary>
        /// Gets or sets the email
        /// </summary>
        [EmailAddress]
        public string? Email { get; set; }
        
        /// <summary>
        /// Gets or sets the password
        /// </summary>
        [StringLength(100, MinimumLength = 6)]
        public string? Password { get; set; }
        
        /// <summary>
        /// Gets or sets the first name
        /// </summary>
        [StringLength(50)]
        public string? FirstName { get; set; }
        
        /// <summary>
        /// Gets or sets the last name
        /// </summary>
        [StringLength(50)]
        public string? LastName { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the user is active
        /// </summary>
        public bool? IsActive { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the user is an administrator
        /// </summary>
        public bool? IsAdmin { get; set; }
        
        /// <summary>
        /// Gets or sets the date the user last logged in
        /// </summary>
        public DateTime? LastLoginDate { get; set; }
    }
} 