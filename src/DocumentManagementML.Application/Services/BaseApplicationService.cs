// -----------------------------------------------------------------------------
// <copyright file="BaseApplicationService.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 29, 2025
// Last Modified:      April 29, 2025
// Version:            0.9.0
// Description:        Base service class providing common functionality for all
//                     application services, including transaction handling and logging.
// -----------------------------------------------------------------------------
using AutoMapper;
using DocumentManagementML.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace DocumentManagementML.Application.Services
{
    /// <summary>
    /// Base application service with common functionality
    /// </summary>
    public abstract class BaseApplicationService
    {
        protected readonly IUnitOfWorkExtended UnitOfWork;
        protected readonly IMapper Mapper;
        protected readonly ILogger Logger;

        /// <summary>
        /// Initializes a new instance of the BaseApplicationService class
        /// </summary>
        /// <param name="unitOfWork">Unit of work for transaction management</param>
        /// <param name="mapper">AutoMapper instance</param>
        /// <param name="logger">Logger</param>
        protected BaseApplicationService(
            IUnitOfWorkExtended unitOfWork,
            IMapper mapper,
            ILogger logger)
        {
            UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes an operation within a transaction
        /// </summary>
        /// <typeparam name="TResult">Result type</typeparam>
        /// <param name="operation">The operation to execute</param>
        /// <param name="errorMessage">Error message for logging</param>
        /// <returns>Operation result</returns>
        protected async Task<TResult> ExecuteInTransactionAsync<TResult>(
            Func<ITransaction, Task<TResult>> operation,
            string errorMessage)
        {
            ITransaction? transaction = null;
            
            try
            {
                // Begin transaction
                transaction = await UnitOfWork.BeginTransactionAsync();
                
                // Execute operation
                var result = await operation(transaction);
                
                // Commit transaction
                await UnitOfWork.CommitTransactionAsync(transaction);
                
                return result;
            }
            catch (Exception ex)
            {
                // Rollback transaction on error
                if (transaction != null)
                {
                    await UnitOfWork.RollbackTransactionAsync(transaction);
                }
                
                Logger.LogError(ex, errorMessage);
                throw;
            }
        }
        
        /// <summary>
        /// Executes an operation within a transaction that returns no result
        /// </summary>
        /// <param name="operation">The operation to execute</param>
        /// <param name="errorMessage">Error message for logging</param>
        protected async Task ExecuteInTransactionAsync(
            Func<ITransaction, Task> operation,
            string errorMessage)
        {
            ITransaction? transaction = null;
            
            try
            {
                // Begin transaction
                transaction = await UnitOfWork.BeginTransactionAsync();
                
                // Execute operation
                await operation(transaction);
                
                // Commit transaction
                await UnitOfWork.CommitTransactionAsync(transaction);
            }
            catch (Exception ex)
            {
                // Rollback transaction on error
                if (transaction != null)
                {
                    await UnitOfWork.RollbackTransactionAsync(transaction);
                }
                
                Logger.LogError(ex, errorMessage);
                throw;
            }
        }
    }
}