// -----------------------------------------------------------------------------
// <copyright file="VersionedFileStorageService.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 30, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        File storage service implementation with versioning support
// -----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using DocumentManagementML.Domain.Services;
using DocumentManagementML.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DocumentManagementML.Infrastructure.Storage
{
    /// <summary>
    /// Enhanced file storage service with versioning support
    /// </summary>
    public class VersionedFileStorageService : IVersionedFileStorageService
    {
        private readonly IOptions<StorageSettings> _storageSettings;
        private readonly ILogger<VersionedFileStorageService> _logger;
        private readonly string _basePath;
        private readonly string _metadataPath;
        
        /// <summary>
        /// Initializes a new instance of the VersionedFileStorageService class
        /// </summary>
        /// <param name="storageSettings">Storage settings</param>
        /// <param name="logger">Logger</param>
        public VersionedFileStorageService(
            IOptions<StorageSettings> storageSettings,
            ILogger<VersionedFileStorageService> logger)
        {
            _storageSettings = storageSettings;
            _logger = logger;
            
            // Set up base path
            _basePath = _storageSettings.Value.BasePath;
            if (string.IsNullOrEmpty(_basePath))
            {
                _basePath = Path.Combine(Path.GetTempPath(), "DocumentManagementStorage");
            }
            
            // Set up metadata path
            _metadataPath = Path.Combine(_basePath, "_metadata");
            
            // Ensure directories exist
            EnsureDirectoriesExist();
            
            _logger.LogInformation("VersionedFileStorageService initialized with base path: {BasePath}", _basePath);
        }
        
        /// <summary>
        /// Ensures all necessary directories exist
        /// </summary>
        private void EnsureDirectoriesExist()
        {
            try
            {
                if (!Directory.Exists(_basePath))
                {
                    Directory.CreateDirectory(_basePath);
                }
                
                if (!Directory.Exists(_metadataPath))
                {
                    Directory.CreateDirectory(_metadataPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating storage directories");
                throw;
            }
        }
        
        /// <summary>
        /// Saves a file to storage (non-versioned, for backward compatibility)
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
                var filePath = Path.Combine(_basePath, storageFileName);
                
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
        /// Retrieves a file from storage (non-versioned, for backward compatibility)
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
        /// Deletes a file from storage (non-versioned, for backward compatibility)
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
        
        /// <summary>
        /// Saves a file as a new version
        /// </summary>
        /// <param name="documentId">Document ID for the file</param>
        /// <param name="fileStream">File content stream</param>
        /// <param name="fileName">Original file name</param>
        /// <param name="userId">ID of the user creating the version</param>
        /// <returns>Path to the saved version and the version number</returns>
        public async Task<(string FilePath, int VersionNumber)> SaveFileVersionAsync(Guid documentId, Stream fileStream, string fileName, Guid userId)
        {
            try
            {
                // Get the document directory
                var documentDir = GetDocumentDirectory(documentId);
                
                // Get the next version number
                var nextVersion = await GetNextVersionNumberAsync(documentId);
                
                // Generate a unique file name for this version
                var safeFileName = Path.GetFileName(fileName); // Ensure we only use the filename part
                var extension = Path.GetExtension(safeFileName);
                var versionFileName = $"v{nextVersion:D5}{extension}";
                var filePath = Path.Combine(documentDir, versionFileName);
                
                // Create a separate stream for hash calculation
                var memoryStream = new MemoryStream();
                await fileStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                
                // Calculate file hash for integrity check
                var fileHash = await GenerateFileHashAsync(memoryStream);
                memoryStream.Position = 0;
                
                // Create a new file and copy the stream contents
                using (var fileStream2 = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    await memoryStream.CopyToAsync(fileStream2);
                }
                
                // Create and save version metadata
                var fileInfo = new FileInfo(filePath);
                var metadata = new FileVersionMetadata
                {
                    DocumentId = documentId,
                    VersionNumber = nextVersion,
                    FilePath = filePath,
                    FileName = safeFileName,
                    FileSizeBytes = fileInfo.Length,
                    ContentType = GetContentTypeFromExtension(extension),
                    CreatedByUserId = userId,
                    CreatedDate = DateTime.UtcNow,
                    ContentHash = fileHash
                };
                
                await SaveVersionMetadataAsync(metadata);
                
                _logger.LogInformation("File version {Version} saved successfully for document {DocumentId}: {FilePath}", 
                    nextVersion, documentId, filePath);
                
                return (filePath, nextVersion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving file version for document {DocumentId}: {FileName}", documentId, fileName);
                throw;
            }
        }
        
        /// <summary>
        /// Gets a specific version of a file
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="versionNumber">Version number (use 0 for latest)</param>
        /// <returns>File content stream and the complete file path</returns>
        public async Task<(Stream FileStream, string FilePath)> GetFileVersionAsync(Guid documentId, int versionNumber = 0)
        {
            try
            {
                // Get version history
                var versions = await GetVersionHistoryAsync(documentId);
                
                // Determine which version to retrieve
                FileVersionMetadata versionMetadata;
                if (versionNumber <= 0)
                {
                    // Get latest version
                    versionMetadata = versions.OrderByDescending(v => v.VersionNumber).FirstOrDefault();
                }
                else
                {
                    // Get specific version
                    versionMetadata = versions.FirstOrDefault(v => v.VersionNumber == versionNumber);
                }
                
                if (versionMetadata == null)
                {
                    throw new FileNotFoundException($"No versions found for document {documentId}");
                }
                
                // Get the file
                var filePath = versionMetadata.FilePath;
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("File version found in metadata but file not found on disk: {FilePath}", filePath);
                    throw new FileNotFoundException($"File not found for document {documentId}, version {versionNumber}", filePath);
                }
                
                // Read the file into a memory stream
                var memoryStream = new MemoryStream();
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    await fileStream.CopyToAsync(memoryStream);
                }
                
                // Reset position and return the stream
                memoryStream.Position = 0;
                
                _logger.LogInformation("Retrieved file version {Version} for document {DocumentId}: {FilePath}", 
                    versionMetadata.VersionNumber, documentId, filePath);
                
                return (memoryStream, filePath);
            }
            catch (Exception ex) when (!(ex is FileNotFoundException))
            {
                _logger.LogError(ex, "Error retrieving file version {Version} for document {DocumentId}", 
                    versionNumber, documentId);
                throw;
            }
        }
        
        /// <summary>
        /// Gets metadata for all versions of a document
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <returns>List of file version metadata</returns>
        public async Task<IEnumerable<FileVersionMetadata>> GetVersionHistoryAsync(Guid documentId)
        {
            try
            {
                var metadataFilePath = GetMetadataFilePath(documentId);
                if (!File.Exists(metadataFilePath))
                {
                    return Enumerable.Empty<FileVersionMetadata>();
                }
                
                using var fileStream = new FileStream(metadataFilePath, FileMode.Open, FileAccess.Read);
                var versionHistory = await JsonSerializer.DeserializeAsync<List<FileVersionMetadata>>(fileStream);
                
                return versionHistory ?? Enumerable.Empty<FileVersionMetadata>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving version history for document {DocumentId}", documentId);
                throw;
            }
        }
        
        /// <summary>
        /// Deletes a specific version of a file (if allowed by retention policy)
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="versionNumber">Version number to delete</param>
        /// <returns>True if successful, false if not found or not allowed to delete</returns>
        public async Task<bool> DeleteFileVersionAsync(Guid documentId, int versionNumber)
        {
            try
            {
                // Get version history
                var versions = (await GetVersionHistoryAsync(documentId)).ToList();
                
                // Find the specific version
                var versionToDelete = versions.FirstOrDefault(v => v.VersionNumber == versionNumber);
                if (versionToDelete == null)
                {
                    _logger.LogWarning("Version {Version} not found for document {DocumentId}", versionNumber, documentId);
                    return false;
                }
                
                // Check if this is the only version (don't allow deletion of only version)
                if (versions.Count == 1)
                {
                    _logger.LogWarning("Cannot delete the only version of document {DocumentId}", documentId);
                    return false;
                }
                
                // Check if this is the latest version (don't allow deletion of latest version)
                var latestVersion = versions.Max(v => v.VersionNumber);
                if (versionNumber == latestVersion)
                {
                    _logger.LogWarning("Cannot delete the latest version of document {DocumentId}", documentId);
                    return false;
                }
                
                // Delete the file
                var filePath = versionToDelete.FilePath;
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                
                // Update metadata by removing the deleted version
                versions.Remove(versionToDelete);
                await SaveVersionMetadataAsync(versions);
                
                _logger.LogInformation("Deleted file version {Version} for document {DocumentId}", versionNumber, documentId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file version {Version} for document {DocumentId}", 
                    versionNumber, documentId);
                throw;
            }
        }
        
        /// <summary>
        /// Gets the document-specific directory
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <returns>Directory path</returns>
        private string GetDocumentDirectory(Guid documentId)
        {
            var documentDir = Path.Combine(_basePath, documentId.ToString());
            
            if (!Directory.Exists(documentDir))
            {
                Directory.CreateDirectory(documentDir);
            }
            
            return documentDir;
        }
        
        /// <summary>
        /// Gets the next available version number for a document
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <returns>Next version number</returns>
        private async Task<int> GetNextVersionNumberAsync(Guid documentId)
        {
            var versions = await GetVersionHistoryAsync(documentId);
            if (!versions.Any())
            {
                return 1;
            }
            
            return versions.Max(v => v.VersionNumber) + 1;
        }
        
        /// <summary>
        /// Saves metadata for a new version
        /// </summary>
        /// <param name="metadata">Version metadata</param>
        private async Task SaveVersionMetadataAsync(FileVersionMetadata metadata)
        {
            // Get existing versions
            var versions = (await GetVersionHistoryAsync(metadata.DocumentId)).ToList();
            
            // Add or update the version
            var existingVersion = versions.FirstOrDefault(v => v.VersionNumber == metadata.VersionNumber);
            if (existingVersion != null)
            {
                versions.Remove(existingVersion);
            }
            
            versions.Add(metadata);
            
            // Save the updated version history
            await SaveVersionMetadataAsync(versions);
        }
        
        /// <summary>
        /// Saves the complete version history for a document
        /// </summary>
        /// <param name="versions">List of version metadata</param>
        private async Task SaveVersionMetadataAsync(IEnumerable<FileVersionMetadata> versions)
        {
            if (!versions.Any())
            {
                return;
            }
            
            var documentId = versions.First().DocumentId;
            var metadataFilePath = GetMetadataFilePath(documentId);
            
            using var fileStream = new FileStream(metadataFilePath, FileMode.Create, FileAccess.Write);
            await JsonSerializer.SerializeAsync(fileStream, versions, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
        
        /// <summary>
        /// Gets the metadata file path for a document
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <returns>Metadata file path</returns>
        private string GetMetadataFilePath(Guid documentId)
        {
            return Path.Combine(_metadataPath, $"{documentId}.json");
        }
        
        /// <summary>
        /// Generates a hash for a file
        /// </summary>
        /// <param name="fileStream">File stream</param>
        /// <returns>Hash string</returns>
        private async Task<string> GenerateFileHashAsync(Stream fileStream)
        {
            using var sha256 = SHA256.Create();
            fileStream.Position = 0;
            var hashBytes = await sha256.ComputeHashAsync(fileStream);
            fileStream.Position = 0; // Reset position for further processing
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
        
        /// <summary>
        /// Gets content type based on file extension
        /// </summary>
        /// <param name="extension">File extension</param>
        /// <returns>MIME content type</returns>
        private string GetContentTypeFromExtension(string extension)
        {
            if (string.IsNullOrEmpty(extension))
            {
                return "application/octet-stream";
            }
            
            // Ensure extension starts with a dot
            if (!extension.StartsWith("."))
            {
                extension = "." + extension;
            }
            
            return extension.ToLowerInvariant() switch
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
        }
    }
}