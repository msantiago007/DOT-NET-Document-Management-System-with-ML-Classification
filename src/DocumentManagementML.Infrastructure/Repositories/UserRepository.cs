// UserRepository.cs
using DocumentManagementML.Domain.Entities;
using DocumentManagementML.Domain.Repositories;
using DocumentManagementML.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DocumentManagementML.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for User entity
    /// </summary>
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        /// <summary>
        /// Initializes a new instance of the UserRepository class
        /// </summary>
        /// <param name="context">Database context</param>
        public UserRepository(DocumentManagementDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Gets a user by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>User if found, null otherwise</returns>
        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .Where(u => u.Username == username && u.IsActive)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets a user by email
        /// </summary>
        /// <param name="email">Email address</param>
        /// <returns>User if found, null otherwise</returns>
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Where(u => u.Email == email && u.IsActive)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets active users with pagination
        /// </summary>
        /// <param name="skip">Number of users to skip</param>
        /// <param name="take">Number of users to take</param>
        /// <returns>Paged collection of active users</returns>
        public async Task<IEnumerable<User>> GetActiveUsersAsync(int skip, int take)
        {
            return await _context.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.Username)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        /// <summary>
        /// Deactivates a user (soft delete)
        /// </summary>
        /// <param name="id">User identifier</param>
        public async Task DeactivateAsync(int id)
        {
            var user = await _dbSet.FindAsync(id);
            if (user != null)
            {
                user.IsActive = false;
                // Optionally set last modified date if your entity has it
                // user.LastModifiedDate = DateTime.UtcNow;
            }
        }
    }
}