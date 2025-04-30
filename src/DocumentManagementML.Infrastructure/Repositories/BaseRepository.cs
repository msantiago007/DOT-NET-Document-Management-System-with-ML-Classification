// -----------------------------------------------------------------------------
// <copyright file="BaseRepository.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            February 22, 2025
// Last Modified:      April 29, 2025
// Version:            0.9.0
// Description:        Generic repository implementation for common entity operations,
//                     providing basic CRUD operations and transaction handling.
// -----------------------------------------------------------------------------
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
        /// <exception cref="InvalidOperationException">Thrown when a transaction could not be started</exception>
        public async Task<ITransaction> BeginTransactionAsync()
        {
            try
            {
                var efTransaction = await _dbContext.Database.BeginTransactionAsync();
                return new DbContextTransaction(efTransaction);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to begin a new transaction", ex);
            }
        }

        /// <summary>
        /// Commits the current transaction
        /// </summary>
        /// <param name="transaction">The transaction to commit</param>
        /// <exception cref="ArgumentNullException">Thrown when transaction is null</exception>
        /// <exception cref="InvalidOperationException">Thrown when the transaction cannot be committed</exception>
        public async Task CommitTransactionAsync(ITransaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            try
            {
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to commit transaction", ex);
            }
        }

        /// <summary>
        /// Rolls back the current transaction
        /// </summary>
        /// <param name="transaction">The transaction to roll back</param>
        /// <exception cref="ArgumentNullException">Thrown when transaction is null</exception>
        public async Task RollbackTransactionAsync(ITransaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            try
            {
                await transaction.RollbackAsync();
            }
            catch (Exception ex)
            {
                // Log but don't rethrow as rollback is typically called in exception handling
                // and we don't want to mask the original exception
                System.Diagnostics.Debug.WriteLine($"Warning: Failed to roll back transaction: {ex.Message}");
            }
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