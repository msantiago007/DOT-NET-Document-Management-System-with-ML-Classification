// IFileStorageService.cs
namespace DocumentManagementML.Application.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> StoreFileAsync(Stream fileStream, string fileName, string contentType);
        Task<Stream> RetrieveFileAsync(string filePath);
        Task DeleteFileAsync(string filePath);
        Task<string> GetContentTypeAsync(string filePath);
        Task<long> GetFileSizeAsync(string filePath);
    }
}