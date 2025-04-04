// IRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocumentManagementML.Domain.Repositories
{
    /// <summary>
    /// Generic repository interface defining common operations for all entities
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Gets an entity by its identifier
        /// </summary>
        /// <param name="id">Entity identifier</param>
        /// <returns>Entity if found, null otherwise</returns>
        Task<T?> GetByIdAsync(int id);
        
        /// <summary>
        /// Gets all entities
        /// </summary>
        /// <returns>Collection of all entities</returns>
        Task<IEnumerable<T>> GetAllAsync();
        
        /// <summary>
        /// Gets a paged collection of entities
        /// </summary>
        /// <param name="skip">Number of entities to skip</param>
        /// <param name="take">Number of entities to take</param>
        /// <returns>Paged collection of entities</returns>
        Task<IEnumerable<T>> GetPagedAsync(int skip, int take);
        
        /// <summary>
        /// Adds a new entity
        /// </summary>
        /// <param name="entity">Entity to add</param>
        /// <returns>Added entity</returns>
        Task<T> AddAsync(T entity);
        
        /// <summary>
        /// Updates an existing entity
        /// </summary>
        /// <param name="entity">Entity to update</param>
        Task UpdateAsync(T entity);
        
        /// <summary>
        /// Deletes an entity
        /// </summary>
        /// <param name="entity">Entity to delete</param>
        Task DeleteAsync(T entity);
        
        /// <summary>
        /// Saves changes to the database
        /// </summary>
        /// <returns>Number of affected entities</returns>
        Task<int> SaveChangesAsync();
    }
}