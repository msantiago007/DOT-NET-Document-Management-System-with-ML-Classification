using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentManagementML.Domain.Entities;

namespace DocumentManagementML.Domain.Services
{
    public interface IDocumentClassificationModel
    {
        Task<DocumentClassificationResult> ClassifyAsync(Stream documentStream, string fileExtension);
        Task<bool> TrainModelAsync(IEnumerable<TrainingDocument> trainingData);
        Task<ModelMetrics> EvaluateModelAsync(IEnumerable<TrainingDocument> testData);
    }
} 