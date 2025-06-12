// -----------------------------------------------------------------------------
// <copyright file="RefreshTokenRepository.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 30, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        Repository implementation for refresh tokens
// -----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentManagementML.Domain.Entities;
using DocumentManagementML.Domain.Repositories;
using DocumentManagementML.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DocumentManagementML.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for refresh tokens
    /// </summary>
    public class RefreshTokenRepository : BaseRepository<RefreshToken>, IRefreshTokenRepository
    {
        /// <summary>
        /// Initializes a new instance of the RefreshTokenRepository class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        public RefreshTokenRepository(DocumentManagementDbContext dbContext)
            : base(dbContext)
        {
        }
        
        /// <summary>
        /// Gets a refresh token by its token string
        /// </summary>
        /// <param name="refreshToken">The refresh token string</param>
        /// <returns>The refresh token entity or null</returns>
        public async Task<RefreshToken?> GetByTokenAsync(string refreshToken)
        {
            return await _dbContext.Set<RefreshToken>()
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);
        }
        
        /// <summary>
        /// Gets all refresh tokens for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>Collection of refresh tokens</returns>
        public async Task<IEnumerable<RefreshToken>> GetAllForUserAsync(Guid userId)
        {
            return await _dbContext.Set<RefreshToken>()
                .Where(rt => rt.UserId == userId)
                .ToListAsync();
        }
        
        /// <summary>
        /// Gets active refresh tokens for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>Collection of active refresh tokens</returns>
        public async Task<IEnumerable<RefreshToken>> GetActiveTokensForUserAsync(Guid userId)
        {
            return await _dbContext.Set<RefreshToken>()
                .Where(rt => rt.UserId == userId && 
                             !rt.IsRevoked && 
                             !rt.IsUsed && 
                             rt.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
        }
        
        /// <summary>
        /// Marks a refresh token as used
        /// </summary>
        /// <param name="refreshTokenId">The refresh token ID</param>
        /// <returns>True if successful</returns>
        public async Task<bool> MarkAsUsedAsync(Guid refreshTokenId)
        {
            var token = await GetByIdAsync(refreshTokenId);
            if (token == null)
            {
                return false;
            }
            
            token.IsUsed = true;
            await UpdateAsync(token);
            return true;
        }
        
        /// <summary>
        /// Revokes a refresh token
        /// </summary>
        /// <param name="refreshTokenId">The refresh token ID</param>
        /// <returns>True if successful</returns>
        public async Task<bool> RevokeTokenAsync(Guid refreshTokenId)
        {
            var token = await GetByIdAsync(refreshTokenId);
            if (token == null)
            {
                return false;
            }
            
            token.IsRevoked = true;
            await UpdateAsync(token);
            return true;
        }
        
        /// <summary>
        /// Revokes all refresh tokens for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>The number of tokens revoked</returns>
        public async Task<int> RevokeAllUserTokensAsync(Guid userId)
        {
            var tokens = await _dbContext.Set<RefreshToken>()
                .Where(rt => rt.UserId == userId && 
                             !rt.IsRevoked && 
                             rt.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
                
            foreach (var token in tokens)
            {
                token.IsRevoked = true;
            }
            
            await _dbContext.SaveChangesAsync();
            return tokens.Count();
        }
    }
}