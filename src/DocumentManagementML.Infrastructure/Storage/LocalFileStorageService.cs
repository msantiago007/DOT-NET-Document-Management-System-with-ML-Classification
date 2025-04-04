// LocalFileStorageService.cs
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

            // Ensure storage directory exists
            Directory.CreateDirectory(_storageSettings.Value.LocalStoragePath);
        }

        public async Task<string> StoreFileAsync(Stream fileStream, string fileName, string contentType)
        {
            try
            {
                // Generate unique filename
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var extension = Path.GetExtension(fileName);
                var storageFileName = $"{timestamp}_{Path.GetFileNameWithoutExtension(fileName)}{extension}";
                
                // Create year/month folder structure
                var yearMonth = DateTime.Now.ToString("yyyy/MM");
                var relativePath = Path.Combine(yearMonth, storageFileName);
                var fullPath = Path.Combine(_storageSettings.Value.LocalStoragePath, relativePath);
                
                // Ensure directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                
                // Save file
                fileStream.Position = 0;
                using (var fileStream2 = new FileStream(fullPath, FileMode.Create))
                {
                    await fileStream.CopyToAsync(fileStream2);
                }
                
                _logger.LogInformation($"File saved: {fullPath}");
                return relativePath; // Return relative path for storage in database
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error storing file: {ex.Message}");
                throw new StorageException("Error storing file", ex);
            }
        }

        public async Task<Stream> RetrieveFileAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_storageSettings.Value.LocalStoragePath, filePath);
                
                if (!File.Exists(fullPath))
                {
                    _logger.LogWarning($"File not found: {fullPath}");
                    throw new FileNotFoundException($"File not found: {filePath}");
                }
                
                // Open file stream and return it
                // Note: Caller is responsible for disposing the stream
                return new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            }
            catch (Exception ex) when (!(ex is FileNotFoundException))
            {
                _logger.LogError(ex, $"Error retrieving file: {ex.Message}");
                throw new StorageException("Error retrieving file", ex);
            }
        }

        public Task DeleteFileAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_storageSettings.Value.LocalStoragePath, filePath);
                
                if (!File.Exists(fullPath))
                {
                    _logger.LogWarning($"File not found for deletion: {fullPath}");
                    return Task.CompletedTask;
                }
                
                File.Delete(fullPath);
                _logger.LogInformation($"File deleted: {fullPath}");
                
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting file: {ex.Message}");
                throw new StorageException("Error deleting file", ex);
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
                var fullPath = Path.Combine(_storageSettings.Value.LocalStoragePath, filePath);
                
                if (!File.Exists(fullPath))
                {
                    _logger.LogWarning($"File not found for size check: {fullPath}");
                    throw new FileNotFoundException($"File not found: {filePath}");
                }
                
                var fileInfo = new FileInfo(fullPath);
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