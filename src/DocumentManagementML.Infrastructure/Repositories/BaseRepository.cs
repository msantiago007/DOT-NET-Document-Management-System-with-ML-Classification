// BaseRepository.cs
using DocumentManagementML.Domain.Repositories;
using DocumentManagementML.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocumentManagementML.Infrastructure.Repositories
{
    /// <summary>
    /// Generic repository implementation for common entity operations
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public class BaseRepository<T> : IRepository<T> where T : class
    {
        protected readonly DocumentManagementDbContext _context;
        protected readonly DbSet<T> _dbSet;

        /// <summary>
        /// Initializes a new instance of the BaseRepository class
        /// </summary>
        /// <param name="context">Database context</param>
        public BaseRepository(DocumentManagementDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        /// <summary>
        /// Gets an entity by its identifier
        /// </summary>
        /// <param name="id">Entity identifier</param>
        /// <returns>Entity if found, null otherwise</returns>
        public virtual async Task<T?> GetByIdAsync(int id)
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
            return entity;
        }

        /// <summary>
        /// Updates an existing entity
        /// </summary>
        /// <param name="entity">Entity to update</param>
        public virtual Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Deletes an entity
        /// </summary>
        /// <param name="entity">Entity to delete</param>
        public virtual Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Saves changes to the database
        /// </summary>
        /// <returns>Number of affected entities</returns>
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}