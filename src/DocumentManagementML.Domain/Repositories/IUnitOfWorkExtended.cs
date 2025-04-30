// -----------------------------------------------------------------------------
// <copyright file="IUnitOfWorkExtended.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 29, 2025
// Last Modified:      April 29, 2025
// Version:            0.9.0
// Description:        Extended Unit of Work interface that provides access to all repositories
// -----------------------------------------------------------------------------
using System.Threading.Tasks;

namespace DocumentManagementML.Domain.Repositories
{
    /// <summary>
    /// Extended Unit of Work interface that provides access to all repositories
    /// </summary>
    public interface IUnitOfWorkExtended : IUnitOfWork
    {
        /// <summary>
        /// Gets the document repository
        /// </summary>
        IDocumentRepository DocumentRepository { get; }
        
        /// <summary>
        /// Gets the document type repository
        /// </summary>
        IDocumentTypeRepository DocumentTypeRepository { get; }
        
        /// <summary>
        /// Gets the document metadata repository
        /// </summary>
        IDocumentMetadataRepository DocumentMetadataRepository { get; }
        
        /// <summary>
        /// Gets the user repository
        /// </summary>
        IUserRepository UserRepository { get; }
        
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
    }
}