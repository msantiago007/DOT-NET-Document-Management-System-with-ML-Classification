// BaseRepository.cs
using DocumentManagementML.Domain.Repositories;
using DocumentManagementML.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using DocumentManagementML.Domain.Entities;

namespace DocumentManagementML.Infrastructure.Repositories
{
    /// <summary>
    /// Generic repository implementation for common entity operations
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public abstract class BaseRepository<T> : IRepository<T> where T : class
    {
        protected readonly DbContext _dbContext;
        protected readonly DbSet<T> _dbSet;

        /// <summary>
        /// Initializes a new instance of the BaseRepository class
        /// </summary>
        /// <param name="dbContext">Database context</param>
        protected BaseRepository(DbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<T>();
        }

        /// <summary>
        /// Gets an entity by its identifier
        /// </summary>
        /// <param name="id">Entity identifier</param>
        /// <returns>Entity if found, null otherwise</returns>
        public virtual async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        /// <summary>
        /// Gets all entities
        /// </summary>
        /// <returns>Collection of all entities</returns>
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        /// <summary>
        /// Gets a paged collection of entities
        /// </summary>
        /// <param name="skip">Number of entities to skip</param>
        /// <param name="take">Number of entities to take</param>
        /// <returns>Paged collection of entities</returns>
        public virtual async Task<IEnumerable<T>> GetPagedAsync(int skip, int take)
        {
            return await _dbSet.Skip(skip).Take(take).ToListAsync();
        }

        /// <summary>
        /// Adds a new entity
        /// </summary>
        /// <param name="entity">Entity to add</param>
        /// <returns>Added entity</returns>
        public virtual async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Updates an existing entity
        /// </summary>
        /// <param name="entity">Entity to update</param>
        /// <returns>Updated entity</returns>
        public virtual async Task<T> UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Deletes an entity
        /// </summary>
        /// <param name="id">Entity identifier</param>
        /// <returns>True if entity was deleted, false otherwise</returns>
        public virtual async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
            {
                return false;
            }

            _dbSet.Remove(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Begins a new database transaction
        /// </summary>
        /// <returns>The transaction</returns>
        public async Task<ITransaction> BeginTransactionAsync()
        {
            var efTransaction = await _dbContext.Database.BeginTransactionAsync();
            return new DbContextTransaction(efTransaction);
        }

        /// <summary>
        /// Commits the current transaction
        /// </summary>
        /// <param name="transaction">The transaction to commit</param>
        public async Task CommitTransactionAsync(ITransaction transaction)
        {
            await transaction.CommitAsync();
        }

        /// <summary>
        /// Rolls back the current transaction
        /// </summary>
        /// <param name="transaction">The transaction to roll back</param>
        public async Task RollbackTransactionAsync(ITransaction transaction)
        {
            await transaction.RollbackAsync();
        }

        /// <summary>
        /// Saves changes to the database
        /// </summary>
        /// <returns>Number of affected entities</returns>
        public async Task<int> SaveChangesAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }
    }
}