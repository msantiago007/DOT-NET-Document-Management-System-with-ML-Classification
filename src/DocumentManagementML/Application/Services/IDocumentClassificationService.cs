using System.Threading.Tasks;
using DocumentManagementML.Application.DTOs;

namespace DocumentManagementML.Application.Services
{
    public interface IDocumentClassificationService
    {
        Task<DocumentClassificationResultDto> ClassifyDocumentAsync(ClassificationRequestDto request);
        Task<bool> ApplyClassificationResultAsync(DocumentClassificationResultDto result);
    }
} 