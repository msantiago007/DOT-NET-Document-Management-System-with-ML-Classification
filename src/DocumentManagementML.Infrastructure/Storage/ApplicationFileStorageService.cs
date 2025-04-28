// ApplicationFileStorageService.cs
using DocumentManagementML.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DomainFileService = DocumentManagementML.Domain.Services.IFileStorageService;

namespace DocumentManagementML.Infrastructure.Storage
{
    /// <summary>
    /// Implementation of the Application layer's IFileStorageService that delegates to the Domain layer's implementation
    /// </summary>
    public class ApplicationFileStorageService : IFileStorageService
    {
        private readonly DomainFileService _domainFileService;
        private readonly ILogger<ApplicationFileStorageService> _logger;
        private readonly Dictionary<string, string> _contentTypeMap = new();

        /// <summary>
        /// Initializes a new instance of the ApplicationFileStorageService class
        /// </summary>
        /// <param name="domainFileService">Domain layer file service</param>
        /// <param name="logger">Logger</param>
        public ApplicationFileStorageService(DomainFileService domainFileService, ILogger<ApplicationFileStorageService> logger)
        {
            _domainFileService = domainFileService;
            _logger = logger;
            
            // Initialize a simple content type map for phase 1
            _contentTypeMap.Add(".txt", "text/plain");
            _contentTypeMap.Add(".pdf", "application/pdf");
            _contentTypeMap.Add(".doc", "application/msword");
            _contentTypeMap.Add(".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
            _contentTypeMap.Add(".xls", "application/vnd.ms-excel");
            _contentTypeMap.Add(".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            _contentTypeMap.Add(".jpg", "image/jpeg");
            _contentTypeMap.Add(".jpeg", "image/jpeg");
            _contentTypeMap.Add(".png", "image/png");
        }

        /// <summary>
        /// Stores a file
        /// </summary>
        /// <param name="fileStream">File content stream</param>
        /// <param name="fileName">Original file name</param>
        /// <param name="contentType">Content type (MIME type)</param>
        /// <returns>File path</returns>
        public async Task<string> StoreFileAsync(Stream fileStream, string fileName, string contentType)
        {
            _logger.LogInformation("Storing file: {FileName}, Content-Type: {ContentType}", fileName, contentType);
            return await _domainFileService.SaveFileAsync(fileStream, fileName);
        }

        /// <summary>
        /// Retrieves a file
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <returns>File content stream</returns>
        public async Task<Stream> RetrieveFileAsync(string filePath)
        {
            _logger.LogInformation("Retrieving file: {FilePath}", filePath);
            return await _domainFileService.GetFileAsync(filePath);
        }

        /// <summary>
        /// Deletes a file
        /// </summary>
        /// <param name="filePath">File path</param>
        public async Task DeleteFileAsync(string filePath)
        {
            _logger.LogInformation("Deleting file: {FilePath}", filePath);
            await _domainFileService.DeleteFileAsync(filePath);
        }

        /// <summary>
        /// Gets the content type of a file
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <returns>Content type (MIME type)</returns>
        public async Task<string> GetContentTypeAsync(string filePath)
        {
            _logger.LogInformation("Getting content type for file: {FilePath}", filePath);
            
            // For phase 1, determine content type based on file extension
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            
            if (_contentTypeMap.TryGetValue(extension, out var contentType))
            {
                return await Task.FromResult(contentType);
            }
            
            // Default content type for unknown extensions
            return await Task.FromResult("application/octet-stream");
        }

        /// <summary>
        /// Gets the size of a file
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <returns>File size in bytes</returns>
        public async Task<long> GetFileSizeAsync(string filePath)
        {
            _logger.LogInformation("Getting file size for: {FilePath}", filePath);
            
            try
            {
                var fileInfo = new FileInfo(filePath);
                if (!fileInfo.Exists)
                {
                    _logger.LogWarning("File not found: {FilePath}", filePath);
                    throw new FileNotFoundException("File not found", filePath);
                }
                
                return await Task.FromResult(fileInfo.Length);
            }
            catch (Exception ex) when (!(ex is FileNotFoundException))
            {
                _logger.LogError(ex, "Error getting file size: {FilePath}", filePath);
                throw;
            }
        }
    }
}