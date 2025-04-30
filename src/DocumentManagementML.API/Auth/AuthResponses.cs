// -----------------------------------------------------------------------------
// <copyright file="AuthResponses.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 30, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        Response models for authentication
// -----------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace DocumentManagementML.API.Auth
{
    /// <summary>
    /// Authentication response model
    /// </summary>
    public class AuthResponse
    {
        /// <summary>
        /// Gets or sets the user ID
        /// </summary>
        public Guid UserId { get; set; }
        
        /// <summary>
        /// Gets or sets the username
        /// </summary>
        public string Username { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the email
        /// </summary>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the first name
        /// </summary>
        public string FirstName { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the last name
        /// </summary>
        public string LastName { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the JWT access token
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the refresh token
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the token expiration time
        /// </summary>
        public DateTime TokenExpiration { get; set; }
        
        /// <summary>
        /// Gets or sets the user roles
        /// </summary>
        public List<string> Roles { get; set; } = new List<string>();
    }
    
    /// <summary>
    /// Token response model
    /// </summary>
    public class TokenResponse
    {
        /// <summary>
        /// Gets or sets the JWT access token
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the refresh token
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the token expiration time
        /// </summary>
        public DateTime TokenExpiration { get; set; }
    }
}