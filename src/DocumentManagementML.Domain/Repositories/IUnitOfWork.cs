// IUnitOfWork.cs
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