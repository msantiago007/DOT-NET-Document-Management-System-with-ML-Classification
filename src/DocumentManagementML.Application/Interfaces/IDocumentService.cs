// IDocumentService.cs
namespace DocumentManagementML.Application.Interfaces
{
    public interface IDocumentService
    {
        Task<DocumentDto> GetDocumentByIdAsync(Guid id);
        Task<IEnumerable<DocumentDto>> GetDocumentsAsync(int skip = 0, int limit = 100);
        Task<DocumentDto> CreateDocumentAsync(DocumentCreateDto documentDto, Stream fileStream, string fileName);
        Task<DocumentDto> UpdateDocumentAsync(Guid id, DocumentUpdateDto documentDto);
        Task DeleteDocumentAsync(Guid id);
        Task<IEnumerable<DocumentDto>> GetDocumentsByTypeAsync(Guid typeId, int skip = 0, int limit = 100);
    }
}