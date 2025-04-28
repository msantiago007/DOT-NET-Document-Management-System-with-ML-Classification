// SimpleFileStorageService.cs
using DocumentManagementML.Domain.Services;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DocumentManagementML.Infrastructure.Storage
{
    /// <summary>
    /// A simple implementation of IFileStorageService for phase 1
    /// </summary>
    public class SimpleFileStorageService : IFileStorageService
    {
        private readonly ILogger<SimpleFileStorageService> _logger;
        private readonly string _storageDirectory;

        /// <summary>
        /// Initializes a new instance of the SimpleFileStorageService class
        /// </summary>
        /// <param name="logger">Logger</param>
        public SimpleFileStorageService(ILogger<SimpleFileStorageService> logger)
        {
            _logger = logger;
            
            // Create a storage directory in the temp folder
            _storageDirectory = Path.Combine(Path.GetTempPath(), "DocumentManagementStorage");
            
            // Ensure the directory exists
            if (!Directory.Exists(_storageDirectory))
            {
                Directory.CreateDirectory(_storageDirectory);
            }
            
            _logger.LogInformation("SimpleFileStorageService initialized with storage directory: {StorageDirectory}", _storageDirectory);
        }

        /// <summary>
        /// Saves a file to storage
        /// </summary>
        /// <param name="fileStream">File content stream</param>
        /// <param name="fileName">Original file name</param>
        /// <returns>File path where the file was stored</returns>
        public async Task<string> SaveFileAsync(Stream fileStream, string fileName)
        {
            try
            {
                // Generate a unique file name
                var fileId = Guid.NewGuid().ToString();
                var safeFileName = Path.GetFileName(fileName); // Ensure we only use the filename part
                var storageFileName = $"{fileId}_{safeFileName}";
                var filePath = Path.Combine(_storageDirectory, storageFileName);
                
                // Create a new file and copy the stream contents
                using (var fileStream2 = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    await fileStream.CopyToAsync(fileStream2);
                }
                
                _logger.LogInformation("File saved successfully: {FilePath}", filePath);
                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving file: {FileName}", fileName);
                throw;
            }
        }

        /// <summary>
        /// Retrieves a file from storage
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <returns>File content stream</returns>
        public async Task<Stream> GetFileAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("File not found: {FilePath}", filePath);
                    throw new FileNotFoundException("File not found", filePath);
                }
                
                // Create a memory stream to return
                var memoryStream = new MemoryStream();
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    await fileStream.CopyToAsync(memoryStream);
                }
                
                // Reset the position to the beginning
                memoryStream.Position = 0;
                return memoryStream;
            }
            catch (Exception ex) when (!(ex is FileNotFoundException))
            {
                _logger.LogError(ex, "Error retrieving file: {FilePath}", filePath);
                throw;
            }
        }

        /// <summary>
        /// Deletes a file from storage
        /// </summary>
        /// <param name="filePath">File path</param>
        public async Task DeleteFileAsync(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("File deleted successfully: {FilePath}", filePath);
                }
                else
                {
                    _logger.LogWarning("File not found for deletion: {FilePath}", filePath);
                }
                
                await Task.CompletedTask; // For async compatibility
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
                throw;
            }
        }
    }
}