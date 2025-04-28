// IRepository.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocumentManagementML.Domain.Repositories
{
    /// <summary>
    /// Generic repository interface for common entity operations
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Gets an entity by its identifier
        /// </summary>
        /// <param name="id">Entity identifier</param>
        /// <returns>Entity if found, null otherwise</returns>
        Task<T?> GetByIdAsync(Guid id);
        
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
        /// <returns>Updated entity</returns>
        Task<T> UpdateAsync(T entity);
        
        /// <summary>
        /// Deletes an entity
        /// </summary>
        /// <param name="id">Entity identifier</param>
        /// <returns>True if entity was deleted, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);
        
        /// <summary>
        /// Begins a new transaction
        /// </summary>
        /// <returns>The transaction</returns>
        Task<ITransaction> BeginTransactionAsync();
        
        /// <summary>
        /// Commits the transaction
        /// </summary>
        /// <param name="transaction">The transaction to commit</param>
        Task CommitTransactionAsync(ITransaction transaction);
        
        /// <summary>
        /// Rolls back the transaction
        /// </summary>
        /// <param name="transaction">The transaction to roll back</param>
        Task RollbackTransactionAsync(ITransaction transaction);
        
        /// <summary>
        /// Saves changes to the database
        /// </summary>
        /// <returns>Number of affected entities</returns>
        Task<int> SaveChangesAsync();
    }
}