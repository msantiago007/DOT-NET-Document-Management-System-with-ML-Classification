// LocalFileStorageService.cs
using System;
using System.IO;
using System.Threading.Tasks;
using DocumentManagementML.Domain.Services;
using DocumentManagementML.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DocumentManagementML.Infrastructure.Storage
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IOptions<StorageSettings> _storageSettings;
        private readonly ILogger<LocalFileStorageService> _logger;

        public LocalFileStorageService(
            IOptions<StorageSettings> storageSettings,
            ILogger<LocalFileStorageService> logger)
        {
            _storageSettings = storageSettings;
            _logger = logger;
        }

        public async Task<string> SaveFileAsync(Stream fileStream, string fileName)
        {
            try
            {
                var basePath = _storageSettings.Value.BasePath;
                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                }

                var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
                var filePath = Path.Combine(basePath, uniqueFileName);

                using (var fileStream2 = new FileStream(filePath, FileMode.Create))
                {
                    await fileStream.CopyToAsync(fileStream2);
                }

                _logger.LogInformation($"File saved successfully: {filePath}");
                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving file: {ex.Message}");
                throw;
            }
        }

        public async Task<Stream> GetFileAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning($"File not found: {filePath}");
                    throw new FileNotFoundException($"File not found: {filePath}");
                }

                var memoryStream = new MemoryStream();
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    await fileStream.CopyToAsync(memoryStream);
                }

                memoryStream.Position = 0;
                return memoryStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving file: {ex.Message}");
                throw;
            }
        }

        public Task DeleteFileAsync(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation($"File deleted successfully: {filePath}");
                }
                else
                {
                    _logger.LogWarning($"File not found for deletion: {filePath}");
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting file: {ex.Message}");
                throw;
            }
        }

        public Task<string> GetContentTypeAsync(string filePath)
        {
            try
            {
                var extension = Path.GetExtension(filePath).ToLowerInvariant();
                var contentType = extension switch
                {
                    ".pdf" => "application/pdf",
                    ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    ".doc" => "application/msword",
                    ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    ".xls" => "application/vnd.ms-excel",
                    ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                    ".ppt" => "application/vnd.ms-powerpoint",
                    ".txt" => "text/plain",
                    ".html" => "text/html",
                    ".csv" => "text/csv",
                    ".jpg" => "image/jpeg",
                    ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    _ => "application/octet-stream"
                };
                
                return Task.FromResult(contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting content type: {ex.Message}");
                throw new StorageException("Error determining content type", ex);
            }
        }

        public Task<long> GetFileSizeAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning($"File not found for size check: {filePath}");
                    throw new FileNotFoundException($"File not found: {filePath}");
                }
                
                var fileInfo = new FileInfo(filePath);
                return Task.FromResult(fileInfo.Length);
            }
            catch (Exception ex) when (!(ex is FileNotFoundException))
            {
                _logger.LogError(ex, $"Error getting file size: {ex.Message}");
                throw new StorageException("Error getting file size", ex);
            }
        }
    }

    public class StorageException : Exception
    {
        public StorageException(string message) : base(message) { }
        public StorageException(string message, Exception innerException) : base(message, innerException) { }
    }
}