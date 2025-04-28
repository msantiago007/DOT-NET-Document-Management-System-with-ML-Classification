// IDocumentService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DocumentManagementML.Application.DTOs;

namespace DocumentManagementML.Application.Interfaces
{
    /// <summary>
    /// Service interface for document operations
    /// </summary>
    public interface IDocumentService
    {
        /// <summary>
        /// Gets a document by its identifier
        /// </summary>
        /// <param name="id">Document identifier</param>
        /// <returns>Document DTO if found, null otherwise</returns>
        Task<DocumentDto?> GetDocumentByIdAsync(Guid id);
        
        /// <summary>
        /// Gets all documents with pagination
        /// </summary>
        /// <param name="skip">Number of documents to skip</param>
        /// <param name="limit">Maximum number of documents to return</param>
        /// <returns>Paged collection of document DTOs</returns>
        Task<IEnumerable<DocumentDto>> GetDocumentsAsync(int skip = 0, int limit = 100);
        
        /// <summary>
        /// Creates a new document with file upload
        /// </summary>
        /// <param name="documentDto">Document creation DTO</param>
        /// <param name="fileStream">Document file stream</param>
        /// <param name="fileName">Original file name</param>
        /// <returns>Created document DTO</returns>
        Task<DocumentDto> CreateDocumentAsync(DocumentCreateDto documentDto, Stream fileStream, string fileName);
        
        /// <summary>
        /// Updates an existing document
        /// </summary>
        /// <param name="id">Document identifier</param>
        /// <param name="documentDto">Document update DTO</param>
        /// <returns>Updated document DTO if found, null otherwise</returns>
        Task<DocumentDto?> UpdateDocumentAsync(Guid id, DocumentUpdateDto documentDto);
        
        /// <summary>
        /// Soft deletes a document
        /// </summary>
        /// <param name="id">Document identifier</param>
        /// <returns>True if the document was deleted, false otherwise</returns>
        Task<bool> DeleteDocumentAsync(Guid id);
        
        /// <summary>
        /// Gets documents by document type with pagination
        /// </summary>
        /// <param name="typeId">Document type identifier</param>
        /// <param name="skip">Number of documents to skip</param>
        /// <param name="limit">Maximum number of documents to return</param>
        /// <returns>Paged collection of document DTOs</returns>
        Task<IEnumerable<DocumentDto>> GetDocumentsByTypeAsync(Guid typeId, int skip = 0, int limit = 100);
        
        /// <summary>
        /// Searches documents by term and optionally by document type with pagination
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <param name="documentTypeId">Optional document type identifier</param>
        /// <param name="skip">Number of documents to skip</param>
        /// <param name="limit">Maximum number of documents to return</param>
        /// <returns>Paged collection of document DTOs</returns>
        Task<IEnumerable<DocumentDto>> SearchDocumentsAsync(string searchTerm, Guid? documentTypeId = null, int skip = 0, int limit = 100);
        
        /// <summary>
        /// Gets the total count of documents
        /// </summary>
        /// <param name="documentTypeId">Optional document type identifier for filtering</param>
        /// <returns>Total count of documents</returns>
        Task<int> GetDocumentCountAsync(Guid? documentTypeId = null);
        
        /// <summary>
        /// Gets the total count of search results
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <param name="documentTypeId">Optional document type identifier for filtering</param>
        /// <returns>Total count of search results</returns>
        Task<int> GetSearchResultCountAsync(string searchTerm, Guid? documentTypeId = null);
        
        /// <summary>
        /// Gets recently added documents
        /// </summary>
        /// <param name="count">Maximum number of documents to return</param>
        /// <returns>Collection of document DTOs</returns>
        Task<IEnumerable<DocumentDto>> GetRecentDocumentsAsync(int count);
    }
}