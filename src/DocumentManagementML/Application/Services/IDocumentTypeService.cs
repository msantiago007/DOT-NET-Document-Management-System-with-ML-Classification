using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentManagementML.Application.DTOs;

namespace DocumentManagementML.Application.Services
{
    public interface IDocumentTypeService
    {
        Task<IEnumerable<DocumentTypeDto>> GetAllDocumentTypesAsync();
        Task<DocumentTypeDto?> GetDocumentTypeByIdAsync(Guid id);
        Task<DocumentTypeDto> CreateDocumentTypeAsync(DocumentTypeCreateDto documentTypeDto);
        Task<DocumentTypeDto?> UpdateDocumentTypeAsync(Guid id, DocumentTypeUpdateDto documentTypeDto);
        Task<bool> DeleteDocumentTypeAsync(Guid id);
    }
} 