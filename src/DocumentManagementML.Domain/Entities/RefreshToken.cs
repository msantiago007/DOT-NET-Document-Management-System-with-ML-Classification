// -----------------------------------------------------------------------------
// <copyright file="RefreshToken.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 30, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        Entity for storing refresh tokens
// -----------------------------------------------------------------------------
using System;

namespace DocumentManagementML.Domain.Entities
{
    /// <summary>
    /// Entity for storing refresh tokens
    /// </summary>
    public class RefreshToken
    {
        /// <summary>
        /// Gets or sets the refresh token ID
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Gets or sets the user ID
        /// </summary>
        public Guid UserId { get; set; }
        
        /// <summary>
        /// Gets or sets the token
        /// </summary>
        public string Token { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the JTI (JWT token ID) claim
        /// </summary>
        public string JwtId { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets a value indicating whether the token is used
        /// </summary>
        public bool IsUsed { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the token is revoked
        /// </summary>
        public bool IsRevoked { get; set; }
        
        /// <summary>
        /// Gets or sets the creation date
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Gets or sets the expiration date
        /// </summary>
        public DateTime ExpiresAt { get; set; }
        
        /// <summary>
        /// Gets or sets the user
        /// </summary>
        public virtual User? User { get; set; }
    }
}