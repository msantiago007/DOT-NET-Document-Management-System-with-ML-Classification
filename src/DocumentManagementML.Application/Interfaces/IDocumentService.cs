// IDocumentService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentManagementML.Application.DTOs;

namespace DocumentManagementML.Application.Interfaces
{
    public interface IDocumentService
    {
        Task<IEnumerable<DocumentDto>> GetAllDocumentsAsync();
        Task<DocumentDto?> GetDocumentByIdAsync(Guid id);
        Task<DocumentDto> CreateDocumentAsync(DocumentCreateDto documentDto, string filePath, string fileType, long fileSize, Guid? userId);
        Task<DocumentDto?> UpdateDocumentAsync(Guid id, DocumentUpdateDto documentDto);
        Task<bool> DeleteDocumentAsync(Guid id);
        Task<IEnumerable<DocumentDto>> SearchDocumentsAsync(string searchTerm, Guid? documentTypeId = null);
        Task<IEnumerable<DocumentDto>> GetDocumentsAsync(int skip = 0, int limit = 100);
        Task<IEnumerable<DocumentDto>> GetDocumentsByTypeAsync(int typeId, int skip = 0, int limit = 100);
    }
}

