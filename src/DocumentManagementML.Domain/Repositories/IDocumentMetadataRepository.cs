// IDocumentMetadataRepository.cs
using DocumentManagementML.Domain.Entities;
using System;
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
        Task<IEnumerable<DocumentMetadata>> GetByDocumentIdAsync(Guid documentId);
        
        /// <summary>
        /// Gets metadata entries by key
        /// </summary>
        /// <param name="key">Metadata key</param>
        /// <returns>Collection of metadata entries</returns>
        Task<IEnumerable<DocumentMetadata>> GetByKeyAsync(string key);
        
        /// <summary>
        /// Updates an existing metadata entry or creates a new one if it doesn't exist
        /// </summary>
        /// <param name="metadata">Metadata entry</param>
        /// <returns>Updated or created metadata entry</returns>
        Task<DocumentMetadata> UpsertAsync(DocumentMetadata metadata);
        
        /// <summary>
        /// Searches metadata by value
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <returns>Collection of metadata entries</returns>
        Task<IEnumerable<DocumentMetadata>> SearchByValueAsync(string searchTerm);
        
        /// <summary>
        /// Deletes all metadata for a document
        /// </summary>
        /// <param name="documentId">Document identifier</param>
        Task DeleteByDocumentIdAsync(Guid documentId);
        
        /// <summary>
        /// Gets metadata entries with pagination
        /// </summary>
        /// <param name="skip">Number of entries to skip</param>
        /// <param name="take">Number of entries to take</param>
        /// <returns>Paged collection of metadata entries</returns>
        Task<IEnumerable<DocumentMetadata>> GetPagedMetadataAsync(int skip, int take);
        
        /// <summary>
        /// Gets the count of metadata entries
        /// </summary>
        /// <returns>Total count of metadata entries</returns>
        Task<int> GetMetadataCountAsync();
        
        /// <summary>
        /// Batch adds multiple metadata entries
        /// </summary>
        /// <param name="metadataItems">Collection of metadata entries to add</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task AddRangeAsync(IEnumerable<DocumentMetadata> metadataItems);
    }
}