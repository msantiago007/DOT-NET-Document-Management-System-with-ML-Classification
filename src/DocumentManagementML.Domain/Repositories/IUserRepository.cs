// IUserRepository.cs
using DocumentManagementML.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocumentManagementML.Domain.Repositories
{
    /// <summary>
    /// Repository interface for User entity
    /// </summary>
    public interface IUserRepository : IRepository<User>
    {
        /// <summary>
        /// Gets a user by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>User if found, null otherwise</returns>
        Task<User?> GetByUsernameAsync(string username);
        
        /// <summary>
        /// Gets a user by email
        /// </summary>
        /// <param name="email">Email address</param>
        /// <returns>User if found, null otherwise</returns>
        Task<User?> GetByEmailAsync(string email);
        
        /// <summary>
        /// Gets active users with pagination
        /// </summary>
        /// <param name="skip">Number of users to skip</param>
        /// <param name="take">Number of users to take</param>
        /// <returns>Paged collection of active users</returns>
        Task<IEnumerable<User>> GetActiveUsersAsync(int skip, int take);
        
        /// <summary>
        /// Deactivates a user (soft delete)
        /// </summary>
        /// <param name="id">User identifier</param>
        Task DeactivateAsync(int id);
    }
}