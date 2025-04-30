// -----------------------------------------------------------------------------
// <copyright file="IJwtTokenService.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 30, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        Interface for JWT token service
// -----------------------------------------------------------------------------
using System;
using System.Security.Claims;
using DocumentManagementML.Domain.Entities;

namespace DocumentManagementML.API.Auth
{
    /// <summary>
    /// Interface for JWT token service
    /// </summary>
    public interface IJwtTokenService
    {
        /// <summary>
        /// Generates a JWT token for a user
        /// </summary>
        /// <param name="user">User entity</param>
        /// <returns>JWT token</returns>
        string GenerateJwtToken(User user);
        
        /// <summary>
        /// Generates a refresh token
        /// </summary>
        /// <returns>Refresh token string</returns>
        string GenerateRefreshToken();
        
        /// <summary>
        /// Gets the expiration date for a refresh token
        /// </summary>
        /// <returns>Refresh token expiration date</returns>
        DateTime GetRefreshTokenExpiryTime();
        
        /// <summary>
        /// Validates a token and returns the principal
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>Principal if valid, null otherwise</returns>
        ClaimsPrincipal? GetPrincipalFromToken(string token);
    }
}