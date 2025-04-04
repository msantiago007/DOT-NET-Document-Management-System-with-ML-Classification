// IDocumentClassificationService.cs
namespace DocumentManagementML.Application.Interfaces
{
    public interface IDocumentClassificationService
    {
        Task<DocumentClassificationResultDto> ClassifyDocumentAsync(int documentId);
        Task<DocumentClassificationResultDto> ClassifyDocumentAsync(Stream documentStream, string fileName);
        Task TrainModelAsync();
        Task<ModelMetricsDto> EvaluateModelAsync();
    }
}