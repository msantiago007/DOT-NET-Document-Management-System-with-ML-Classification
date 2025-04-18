// IDocumentTypeRepository.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentManagementML.Domain.Entities;

namespace DocumentManagementML.Domain.Repositories
{
    /// <summary>
    /// Repository interface for DocumentType entity
    /// </summary>
    public interface IDocumentTypeRepository
    {
        /// <summary>
        /// Gets all document types
        /// </summary>
        /// <returns>Collection of document types</returns>
        Task<IEnumerable<DocumentType>> GetAllAsync();
        
        /// <summary>
        /// Gets a document type by its identifier
        /// </summary>
        /// <param name="id">Document type identifier</param>
        /// <returns>Document type</returns>
        Task<DocumentType?> GetByIdAsync(Guid id);
        
        /// <summary>
        /// Adds a new document type
        /// </summary>
        /// <param name="documentType">Document type to add</param>
        /// <returns>Added document type</returns>
        Task<DocumentType> AddAsync(DocumentType documentType);
        
        /// <summary>
        /// Updates an existing document type
        /// </summary>
        /// <param name="documentType">Document type to update</param>
        /// <returns>Updated document type</returns>
        Task<DocumentType> UpdateAsync(DocumentType documentType);
        
        /// <summary>
        /// Deletes a document type
        /// </summary>
        /// <param name="id">Document type identifier</param>
        /// <returns>True if the document type was deleted, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);
    }
}