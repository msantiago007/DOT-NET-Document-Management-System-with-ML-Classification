// -----------------------------------------------------------------------------
// <copyright file="JwtSettings.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 30, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        Settings for JWT authentication
// -----------------------------------------------------------------------------
namespace DocumentManagementML.API.Auth
{
    /// <summary>
    /// JWT configuration settings
    /// </summary>
    public class JwtSettings
    {
        /// <summary>
        /// Gets or sets the JWT secret key
        /// </summary>
        public string SecretKey { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the issuer (iss claim)
        /// </summary>
        public string Issuer { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the audience (aud claim)
        /// </summary>
        public string Audience { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the token expiration in minutes
        /// </summary>
        public int ExpirationMinutes { get; set; } = 60;
        
        /// <summary>
        /// Gets or sets the refresh token expiration in days
        /// </summary>
        public int RefreshTokenExpirationDays { get; set; } = 7;
    }
}