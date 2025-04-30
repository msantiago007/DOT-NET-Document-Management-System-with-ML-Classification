// -----------------------------------------------------------------------------
// <copyright file="DbContextTransaction.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            February 22, 2025
// Last Modified:      April 29, 2025
// Version:            0.9.0
// Description:        Implementation of ITransaction that wraps an Entity Framework
//                     IDbContextTransaction to provide abstracted transaction handling.
// -----------------------------------------------------------------------------
using DocumentManagementML.Domain.Repositories;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Threading.Tasks;

namespace DocumentManagementML.Infrastructure.Repositories
{
    /// <summary>
    /// Implementation of ITransaction that wraps an Entity Framework IDbContextTransaction
    /// </summary>
    public class DbContextTransaction : ITransaction, IDisposable, IAsyncDisposable
    {
        private readonly IDbContextTransaction _transaction;
        private bool _isDisposed;
        
        /// <summary>
        /// Initializes a new instance of the DbContextTransaction class
        /// </summary>
        /// <param name="transaction">The Entity Framework transaction to wrap</param>
        /// <exception cref="ArgumentNullException">Thrown when transaction is null</exception>
        public DbContextTransaction(IDbContextTransaction transaction)
        {
            _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
            _isDisposed = false;
        }
        
        /// <summary>
        /// Commits the transaction
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown when the transaction has been disposed</exception>
        public async Task CommitAsync()
        {
            ThrowIfDisposed();
            await _transaction.CommitAsync();
        }
        
        /// <summary>
        /// Rolls back the transaction
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown when the transaction has been disposed</exception>
        public async Task RollbackAsync()
        {
            ThrowIfDisposed();
            await _transaction.RollbackAsync();
        }
        
        /// <summary>
        /// Disposes the transaction
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Asynchronously disposes the transaction
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of Dispose pattern
        /// </summary>
        /// <param name="disposing">True to dispose managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _transaction.Dispose();
                }

                _isDisposed = true;
            }
        }

        /// <summary>
        /// Protected implementation of async Dispose pattern
        /// </summary>
        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (!_isDisposed)
            {
                await _transaction.DisposeAsync();
            }
        }
        
        /// <summary>
        /// Gets the underlying Entity Framework transaction
        /// </summary>
        /// <returns>The underlying EF Core transaction</returns>
        /// <exception cref="ObjectDisposedException">Thrown when the transaction has been disposed</exception>
        public IDbContextTransaction GetDbContextTransaction()
        {
            ThrowIfDisposed();
            return _transaction;
        }
        
        /// <summary>
        /// Throws an ObjectDisposedException if this transaction has been disposed
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown when the transaction has been disposed</exception>
        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(DbContextTransaction), "The transaction has been disposed");
            }
        }
    }
}