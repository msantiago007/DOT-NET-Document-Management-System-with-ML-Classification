// IDocumentTypeService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentManagementML.Application.DTOs;

namespace DocumentManagementML.Application.Interfaces
{
    /// <summary>
    /// Service interface for document type operations
    /// </summary>
    public interface IDocumentTypeService
    {
        /// <summary>
        /// Gets a document type by its identifier
        /// </summary>
        /// <param name="id">Document type identifier</param>
        /// <returns>Document type DTO if found, null otherwise</returns>
        Task<DocumentTypeDto?> GetDocumentTypeByIdAsync(Guid id);
        
        /// <summary>
        /// Gets all document types with pagination
        /// </summary>
        /// <param name="skip">Number of document types to skip</param>
        /// <param name="limit">Maximum number of document types to return</param>
        /// <returns>Paged collection of document type DTOs</returns>
        Task<IEnumerable<DocumentTypeDto>> GetDocumentTypesAsync(int skip = 0, int limit = 100);
        
        /// <summary>
        /// Gets all active document types with pagination
        /// </summary>
        /// <param name="skip">Number of document types to skip</param>
        /// <param name="limit">Maximum number of document types to return</param>
        /// <returns>Paged collection of active document type DTOs</returns>
        Task<IEnumerable<DocumentTypeDto>> GetActiveDocumentTypesAsync(int skip = 0, int limit = 100);
        
        /// <summary>
        /// Gets all document types (without pagination)
        /// </summary>
        /// <returns>Collection of document type DTOs</returns>
        Task<IEnumerable<DocumentTypeDto>> GetAllDocumentTypesAsync();
        
        /// <summary>
        /// Creates a new document type
        /// </summary>
        /// <param name="documentTypeDto">Document type creation DTO</param>
        /// <returns>Created document type DTO</returns>
        Task<DocumentTypeDto> CreateDocumentTypeAsync(DocumentTypeCreateDto documentTypeDto);
        
        /// <summary>
        /// Updates an existing document type
        /// </summary>
        /// <param name="id">Document type identifier</param>
        /// <param name="documentTypeDto">Document type update DTO</param>
        /// <returns>Updated document type DTO if found, null otherwise</returns>
        Task<DocumentTypeDto?> UpdateDocumentTypeAsync(Guid id, DocumentTypeUpdateDto documentTypeDto);
        
        /// <summary>
        /// Deactivates a document type (soft delete)
        /// </summary>
        /// <param name="id">Document type identifier</param>
        Task DeactivateDocumentTypeAsync(Guid id);
        
        /// <summary>
        /// Deletes a document type permanently
        /// </summary>
        /// <param name="id">Document type identifier</param>
        /// <returns>True if the document type was deleted, false otherwise</returns>
        Task<bool> DeleteDocumentTypeAsync(Guid id);
        
        /// <summary>
        /// Gets the total count of document types
        /// </summary>
        /// <param name="activeOnly">If true, only count active document types</param>
        /// <returns>Total count of document types</returns>
        Task<int> GetDocumentTypeCountAsync(bool activeOnly = false);
        
        /// <summary>
        /// Gets a document type by name
        /// </summary>
        /// <param name="name">Document type name</param>
        /// <returns>Document type DTO if found, null otherwise</returns>
        Task<DocumentTypeDto?> GetDocumentTypeByNameAsync(string name);
    }
}