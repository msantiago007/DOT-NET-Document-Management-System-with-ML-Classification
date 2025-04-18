// IDocumentTypeService.cs
namespace DocumentManagementML.Application.Interfaces
{
    public interface IDocumentTypeService
    {
        Task<DocumentTypeDto> GetDocumentTypeByIdAsync(Guid id);
        Task<IEnumerable<DocumentTypeDto>> GetDocumentTypesAsync(int skip = 0, int limit = 100);
        Task<DocumentTypeDto> CreateDocumentTypeAsync(DocumentTypeCreateDto documentTypeDto);
        Task<DocumentTypeDto> UpdateDocumentTypeAsync(Guid id, DocumentTypeUpdateDto documentTypeDto);
        Task DeactivateDocumentTypeAsync(Guid id);
    }
}