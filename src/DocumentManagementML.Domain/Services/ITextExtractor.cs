using System.IO;
using System.Threading.Tasks;

namespace DocumentManagementML.Domain.Services
{
    public interface ITextExtractor
    {
        Task<string> ExtractTextAsync(Stream documentStream, string fileExtension);
    }
} 