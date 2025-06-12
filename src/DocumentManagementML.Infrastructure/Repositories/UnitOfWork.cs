// -----------------------------------------------------------------------------
// <copyright file="UnitOfWork.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 29, 2025
// Last Modified:      April 29, 2025
// Version:            0.9.0
// Description:        Implementation of the Unit of Work pattern for coordinating
//                     multiple repositories and transactions.
// -----------------------------------------------------------------------------
using DocumentManagementML.Domain.Repositories;
using DocumentManagementML.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Threading.Tasks;

namespace DocumentManagementML.Infrastructure.Repositories
{
    /// <summary>
    /// Implementation of the Unit of Work pattern for coordinating multiple repositories and transactions
    /// </summary>
    public class UnitOfWork : IUnitOfWorkExtended, IDisposable, IAsyncDisposable
    {
        private readonly DocumentManagementDbContext _dbContext;
        private bool _disposed;
        private IDocumentRepository? _documentRepository;
        private IDocumentTypeRepository? _documentTypeRepository;
        private IDocumentMetadataRepository? _documentMetadataRepository;
        private IUserRepository? _userRepository;

        /// <summary>
        /// Initializes a new instance of the UnitOfWork class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <exception cref="ArgumentNullException">Thrown when dbContext is null</exception>
        public UnitOfWork(DocumentManagementDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _disposed = false;
        }

        /// <summary>
        /// Gets the document repository
        /// </summary>
        public IDocumentRepository DocumentRepository 
        {
            get
            {
                _documentRepository ??= new DocumentRepository(_dbContext);
                return _documentRepository;
            }
        }

        /// <summary>
        /// Gets the document type repository
        /// </summary>
        public IDocumentTypeRepository DocumentTypeRepository
        {
            get
            {
                _documentTypeRepository ??= new DocumentTypeRepository(_dbContext);
                return _documentTypeRepository;
            }
        }

        /// <summary>
        /// Gets the document metadata repository
        /// </summary>
        public IDocumentMetadataRepository DocumentMetadataRepository
        {
            get
            {
                _documentMetadataRepository ??= new DocumentMetadataRepository(_dbContext);
                return _documentMetadataRepository;
            }
        }

        /// <summary>
        /// Gets the user repository
        /// </summary>
        public IUserRepository UserRepository
        {
            get
            {
                _userRepository ??= new UserRepository(_dbContext);
                return _userRepository;
            }
        }

        /// <summary>
        /// Begins a new transaction
        /// </summary>
        /// <returns>The transaction</returns>
        /// <exception cref="InvalidOperationException">Thrown when a transaction could not be started</exception>
        public async Task<ITransaction> BeginTransactionAsync()
        {
            try
            {
                // Check if using in-memory database (which doesn't support transactions)
                if (_dbContext.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
                {
                    // Return a dummy transaction for in-memory database
                    return new DummyTransaction();
                }
                
                var efTransaction = await _dbContext.Database.BeginTransactionAsync();
                return new DbContextTransaction(efTransaction);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to begin a new transaction", ex);
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
        
        /// <summary>
        /// Commits the current transaction
        /// </summary>
        /// <returns>Completed task</returns>
        public async Task CommitAsync()
        {
            try
            {
                // Check if using in-memory database (which doesn't support transactions)
                if (_dbContext.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
                {
                    await _dbContext.Database.CommitTransactionAsync();
                }
                // Just save changes for in-memory database
                else
                {
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to commit transaction", ex);
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
                System.Diagnostics.Debug.WriteLine($"Warning: Failed to roll back transaction: {ex.Message}");
            }
        }

        /// <summary>
        /// Disposes the UnitOfWork and the underlying database context
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Asynchronously disposes the UnitOfWork and the underlying database context
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
            if (!_disposed)
            {
                if (disposing)
                {
                    _dbContext.Dispose();
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Protected implementation of async Dispose pattern
        /// </summary>
        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (!_disposed)
            {
                await _dbContext.DisposeAsync();
            }
        }
    }
    
    /// <summary>
    /// Dummy transaction for in-memory database that doesn't support transactions
    /// </summary>
    internal class DummyTransaction : ITransaction
    {
        /// <summary>
        /// Does nothing as in-memory database doesn't support transactions
        /// </summary>
        public Task CommitAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Does nothing as in-memory database doesn't support transactions
        /// </summary>
        public Task RollbackAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Does nothing as in-memory database doesn't support transactions
        /// </summary>
        public void Dispose()
        {
            // Nothing to dispose
        }
    }
}