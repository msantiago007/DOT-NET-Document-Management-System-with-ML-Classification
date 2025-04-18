// IDocumentClassificationService.cs
using System.Threading.Tasks;
using DocumentManagementML.Application.DTOs;

namespace DocumentManagementML.Application.Interfaces
{
    public interface IDocumentClassificationService
    {
        Task<DocumentClassificationResultDto> ClassifyDocumentAsync(ClassificationRequestDto request);
        Task<bool> ApplyClassificationResultAsync(DocumentClassificationResultDto result);
        Task<ModelMetricsDto> GetModelMetricsAsync();
    }
}