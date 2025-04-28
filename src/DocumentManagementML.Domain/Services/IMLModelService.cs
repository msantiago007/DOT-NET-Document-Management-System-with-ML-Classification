using System.Threading.Tasks;
using DocumentManagementML.Domain.Entities;

namespace DocumentManagementML.Domain.Services
{
    public interface IMLModelService
    {
        Task<ClassificationResult> ClassifyDocumentAsync(string filePath, string fileType);
    }
} 