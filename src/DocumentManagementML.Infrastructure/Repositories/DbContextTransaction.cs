// DbContextTransaction.cs
using DocumentManagementML.Domain.Repositories;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Threading.Tasks;

namespace DocumentManagementML.Infrastructure.Repositories
{
    /// <summary>
    /// Implementation of ITransaction that wraps an Entity Framework IDbContextTransaction
    /// </summary>
    public class DbContextTransaction : ITransaction, IDisposable
    {
        private readonly IDbContextTransaction _transaction;
        
        /// <summary>
        /// Initializes a new instance of the DbContextTransaction class
        /// </summary>
        /// <param name="transaction">The Entity Framework transaction to wrap</param>
        public DbContextTransaction(IDbContextTransaction transaction)
        {
            _transaction = transaction;
        }
        
        /// <summary>
        /// Commits the transaction
        /// </summary>
        public async Task CommitAsync()
        {
            await _transaction.CommitAsync();
        }
        
        /// <summary>
        /// Rolls back the transaction
        /// </summary>
        public async Task RollbackAsync()
        {
            await _transaction.RollbackAsync();
        }
        
        /// <summary>
        /// Disposes the transaction
        /// </summary>
        public void Dispose()
        {
            _transaction.Dispose();
        }
        
        /// <summary>
        /// Gets the underlying Entity Framework transaction
        /// </summary>
        public IDbContextTransaction GetDbContextTransaction()
        {
            return _transaction;
        }
    }
}