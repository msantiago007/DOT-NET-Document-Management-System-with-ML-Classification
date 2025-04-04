// IDocumentService.cs
namespace DocumentManagementML.Application.Interfaces
{
    public interface IDocumentService
    {
        Task<DocumentDto> GetDocumentByIdAsync(int id);
        Task<IEnumerable<DocumentDto>> GetDocumentsAsync(int skip = 0, int limit = 100);
        Task<DocumentDto> CreateDocumentAsync(DocumentCreateDto documentDto, Stream fileStream, string fileName);
        Task<DocumentDto> UpdateDocumentAsync(int id, DocumentUpdateDto documentDto);
        Task DeleteDocumentAsync(int id);
        Task<IEnumerable<DocumentDto>> GetDocumentsByTypeAsync(int typeId, int skip = 0, int limit = 100);
    }
}

