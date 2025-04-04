// IDocumentRepository.cs
using DocumentManagementML.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocumentManagementML.Domain.Repositories
{
    /// <summary>
    /// Repository interface for Document entity
    /// </summary>
    public interface IDocumentRepository : IRepository<Document>
    {
        /// <summary>
        /// Gets documents by document type
        /// </summary>
        /// <param name="typeId">Document type identifier</param>
        /// <returns>Collection of documents</returns>
        Task<IEnumerable<Document>> GetByTypeIdAsync(int typeId);
        
        /// <summary>
        /// Gets active (non-deleted) documents with pagination
        /// </summary>
        /// <param name="skip">Number of documents to skip</param>
        /// <param name="take">Number of documents to take</param>
        /// <returns>Paged collection of active documents</returns>
        Task<IEnumerable<Document>> GetActiveDocumentsAsync(int skip, int take);
        
        /// <summary>
        /// Gets a document with its metadata
        /// </summary>
        /// <param name="id">Document identifier</param>
        /// <returns>Document with metadata if found, null otherwise</returns>
        Task<Document?> GetWithMetadataAsync(int id);
        
        /// <summary>
        /// Gets a document with its version history
        /// </summary>
        /// <param name="id">Document identifier</param>
        /// <returns>Document with versions if found, null otherwise</returns>
        Task<Document?> GetWithVersionsAsync(int id);
        
        /// <summary>
        /// Soft deletes a document
        /// </summary>
        /// <param name="id">Document identifier</param>
        Task SoftDeleteAsync(int id);
    }
}