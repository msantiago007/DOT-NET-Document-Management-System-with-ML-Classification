// -----------------------------------------------------------------------------
// <copyright file="IRefreshTokenRepository.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 30, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        Repository interface for refresh tokens
// -----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentManagementML.Domain.Entities;

namespace DocumentManagementML.Domain.Repositories
{
    /// <summary>
    /// Repository interface for refresh tokens
    /// </summary>
    public interface IRefreshTokenRepository : IRepository<RefreshToken>
    {
        /// <summary>
        /// Gets a refresh token by its token string
        /// </summary>
        /// <param name="refreshToken">The refresh token string</param>
        /// <returns>The refresh token entity or null</returns>
        Task<RefreshToken?> GetByTokenAsync(string refreshToken);
        
        /// <summary>
        /// Gets all refresh tokens for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>Collection of refresh tokens</returns>
        Task<IEnumerable<RefreshToken>> GetAllForUserAsync(Guid userId);
        
        /// <summary>
        /// Gets active refresh tokens for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>Collection of active refresh tokens</returns>
        Task<IEnumerable<RefreshToken>> GetActiveTokensForUserAsync(Guid userId);
        
        /// <summary>
        /// Marks a refresh token as used
        /// </summary>
        /// <param name="refreshTokenId">The refresh token ID</param>
        /// <returns>True if successful</returns>
        Task<bool> MarkAsUsedAsync(Guid refreshTokenId);
        
        /// <summary>
        /// Revokes a refresh token
        /// </summary>
        /// <param name="refreshTokenId">The refresh token ID</param>
        /// <returns>True if successful</returns>
        Task<bool> RevokeTokenAsync(Guid refreshTokenId);
        
        /// <summary>
        /// Revokes all refresh tokens for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>The number of tokens revoked</returns>
        Task<int> RevokeAllUserTokensAsync(Guid userId);
    }
}