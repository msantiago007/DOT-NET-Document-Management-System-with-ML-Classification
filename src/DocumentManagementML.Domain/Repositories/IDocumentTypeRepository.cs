// IDocumentTypeRepository.cs
using DocumentManagementML.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocumentManagementML.Domain.Repositories
{
    /// <summary>
    /// Repository interface for DocumentType entity
    /// </summary>
    public interface IDocumentTypeRepository : IRepository<DocumentType>
    {
        /// <summary>
        /// Gets all active document types
        /// </summary>
        /// <returns>Collection of active document types</returns>
        Task<IEnumerable<DocumentType>> GetActiveTypesAsync();
        
        /// <summary>
        /// Deactivates a document type (soft delete)
        /// </summary>
        /// <param name="id">Document type identifier</param>
        Task DeactivateAsync(int id);
    }
}