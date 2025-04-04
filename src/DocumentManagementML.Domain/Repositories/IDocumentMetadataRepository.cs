// IDocumentMetadataRepository.cs
using DocumentManagementML.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocumentManagementML.Domain.Repositories
{
    /// <summary>
    /// Repository interface for DocumentMetadata entity
    /// </summary>
    public interface IDocumentMetadataRepository : IRepository<DocumentMetadata>
    {
        /// <summary>
        /// Gets metadata for a document
        /// </summary>
        /// <param name="documentId">Document identifier</param>
        /// <returns>Collection of metadata entries</returns>
        Task<IEnumerable<DocumentMetadata>> GetByDocumentIdAsync(int documentId);
        
        /// <summary>
        /// Gets a metadata entry by document and key
        /// </summary>
        /// <param name="documentId">Document identifier</param>
        /// <param name="key">Metadata key</param>
        /// <returns>Metadata entry if found, null otherwise</returns>
        Task<DocumentMetadata?> GetByKeyAsync(int documentId, string key);
    }
}