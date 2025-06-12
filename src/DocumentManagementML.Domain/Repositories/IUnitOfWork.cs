// -----------------------------------------------------------------------------
// <copyright file="IUnitOfWork.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            February 22, 2025
// Last Modified:      April 29, 2025
// Version:            0.9.0
// Description:        Interfaces for database transaction abstraction and management.
// -----------------------------------------------------------------------------
using System.Threading.Tasks;

namespace DocumentManagementML.Domain.Repositories
{
    /// <summary>
    /// Interface for database transaction abstraction
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Begins a new transaction
        /// </summary>
        /// <returns>The transaction</returns>
        Task<ITransaction> BeginTransactionAsync();
        
        /// <summary>
        /// Saves changes to the database
        /// </summary>
        /// <returns>Number of affected entities</returns>
        Task<int> SaveChangesAsync();
        
        /// <summary>
        /// Commits the current transaction
        /// </summary>
        /// <returns>Completed task</returns>
        Task CommitAsync();
    }
    
    /// <summary>
    /// Interface for transaction abstraction
    /// </summary>
    public interface ITransaction
    {
        /// <summary>
        /// Commits the transaction
        /// </summary>
        Task CommitAsync();
        
        /// <summary>
        /// Rolls back the transaction
        /// </summary>
        Task RollbackAsync();
    }
}