// IDocumentTypeService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentManagementML.Application.DTOs;

namespace DocumentManagementML.Application.Interfaces
{
    public interface IDocumentTypeService
    {
        Task<IEnumerable<DocumentTypeDto>> GetAllDocumentTypesAsync();
        Task<DocumentTypeDto?> GetDocumentTypeByIdAsync(Guid id);
        Task<DocumentTypeDto> CreateDocumentTypeAsync(DocumentTypeCreateDto documentTypeDto);
        Task<DocumentTypeDto?> UpdateDocumentTypeAsync(Guid id, DocumentTypeUpdateDto documentTypeDto);
        Task<bool> DeleteDocumentTypeAsync(Guid id);
        
        // These methods need to be updated to use Guid instead of int
        Task<IEnumerable<DocumentTypeDto>> GetDocumentTypesAsync(int skip = 0, int limit = 100);
        Task DeactivateDocumentTypeAsync(int id);
    }
}