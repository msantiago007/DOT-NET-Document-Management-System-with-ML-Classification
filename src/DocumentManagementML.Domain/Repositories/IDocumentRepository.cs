// IDocumentRepository.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentManagementML.Domain.Entities;

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
        Task<IEnumerable<Document>> GetByTypeIdAsync(Guid typeId);
        
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
        Task<Document?> GetWithMetadataAsync(Guid id);
        
        /// <summary>
        /// Gets a document with its version history
        /// </summary>
        /// <param name="id">Document identifier</param>
        /// <returns>Document with versions if found, null otherwise</returns>
        Task<Document?> GetWithVersionsAsync(Guid id);
        
        /// <summary>
        /// Soft deletes a document
        /// </summary>
        /// <param name="id">Document identifier</param>
        Task SoftDeleteAsync(Guid id);
        
        /// <summary>
        /// Gets documents by document type
        /// </summary>
        /// <param name="documentTypeId">Document type identifier</param>
        /// <returns>Collection of documents</returns>
        Task<IEnumerable<Document>> GetByDocumentTypeAsync(Guid documentTypeId);
        
        /// <summary>
        /// Gets documents uploaded by a user
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <returns>Collection of documents</returns>
        Task<IEnumerable<Document>> GetByUploadedByAsync(Guid userId);
        
        /// <summary>
        /// Searches documents by term and optionally filters by document type
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <param name="documentTypeId">Optional document type identifier</param>
        /// <returns>Collection of documents</returns>
        Task<IEnumerable<Document>> SearchAsync(string searchTerm, Guid? documentTypeId = null);
        
        /// <summary>
        /// Gets the most recent documents
        /// </summary>
        /// <param name="count">Number of documents to return</param>
        /// <returns>Collection of documents</returns>
        Task<IEnumerable<Document>> GetRecentDocumentsAsync(int count);
    }
}