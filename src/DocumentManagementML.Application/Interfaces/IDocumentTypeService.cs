// IDocumentTypeService.cs
namespace DocumentManagementML.Application.Interfaces
{
    public interface IDocumentTypeService
    {
        Task<DocumentTypeDto> GetDocumentTypeByIdAsync(int id);
        Task<IEnumerable<DocumentTypeDto>> GetDocumentTypesAsync(int skip = 0, int limit = 100);
        Task<DocumentTypeDto> CreateDocumentTypeAsync(DocumentTypeCreateDto documentTypeDto);
        Task<DocumentTypeDto> UpdateDocumentTypeAsync(int id, DocumentTypeUpdateDto documentTypeDto);
        Task DeactivateDocumentTypeAsync(int id);
    }
}