// -----------------------------------------------------------------------------
// <copyright file="VersionedApplicationFileStorageService.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 30, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        Application layer adapter for the versioned file storage service
// -----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DocumentManagementML.Application.Interfaces;
using DocumentManagementML.Domain.Services;
using Microsoft.Extensions.Logging;

namespace DocumentManagementML.Infrastructure.Storage
{
    /// <summary>
    /// Application layer adapter for the versioned file storage service
    /// </summary>
    public class VersionedApplicationFileStorageService : 
        ApplicationFileStorageService, 
        DocumentManagementML.Application.Interfaces.IVersionedFileStorageService
    {
        private readonly DocumentManagementML.Domain.Services.IVersionedFileStorageService _versionedFileStorageService;
        private readonly ILogger<VersionedApplicationFileStorageService> _logger;
        
        /// <summary>
        /// Initializes a new instance of the VersionedApplicationFileStorageService class
        /// </summary>
        /// <param name="fileStorageService">Domain file storage service</param>
        /// <param name="versionedFileStorageService">Domain versioned file storage service</param>
        /// <param name="logger">Logger</param>
        public VersionedApplicationFileStorageService(
            DocumentManagementML.Domain.Services.IFileStorageService fileStorageService,
            DocumentManagementML.Domain.Services.IVersionedFileStorageService versionedFileStorageService,
            ILogger<VersionedApplicationFileStorageService> logger)
            : base(fileStorageService, logger)
        {
            _versionedFileStorageService = versionedFileStorageService;
            _logger = logger;
        }
        
        /// <summary>
        /// Saves a file with versioning
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="fileStream">File content</param>
        /// <param name="fileName">Original file name</param>
        /// <param name="userId">User ID who is saving the file</param>
        /// <returns>Version information tuple (FilePath, VersionNumber)</returns>
        public async Task<(string FilePath, int VersionNumber)> SaveVersionAsync(
            Guid documentId, Stream fileStream, string fileName, Guid userId)
        {
            try
            {
                _logger.LogInformation("Saving version for document {DocumentId} by user {UserId}", documentId, userId);
                return await _versionedFileStorageService.SaveFileVersionAsync(documentId, fileStream, fileName, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving version for document {DocumentId}", documentId);
                throw;
            }
        }
        
        /// <summary>
        /// Gets a specific version of a file
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="versionNumber">Version number (0 for latest)</param>
        /// <returns>File content and file path</returns>
        public async Task<(Stream Content, string FilePath)> GetVersionAsync(Guid documentId, int versionNumber = 0)
        {
            try
            {
                _logger.LogInformation("Getting version {Version} for document {DocumentId}", 
                    versionNumber > 0 ? versionNumber.ToString() : "latest", documentId);
                
                var result = await _versionedFileStorageService.GetFileVersionAsync(documentId, versionNumber);
                return (result.FileStream, result.FilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting version {Version} for document {DocumentId}", 
                    versionNumber, documentId);
                throw;
            }
        }
        
        /// <summary>
        /// Gets version history for a document
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <returns>List of version information</returns>
        public async Task<IEnumerable<FileVersionInfo>> GetVersionHistoryAsync(Guid documentId)
        {
            try
            {
                _logger.LogInformation("Getting version history for document {DocumentId}", documentId);
                
                var domainVersions = await _versionedFileStorageService.GetVersionHistoryAsync(documentId);
                
                // Map to application DTO
                return domainVersions.Select(v => new FileVersionInfo
                {
                    VersionNumber = v.VersionNumber,
                    FileName = v.FileName,
                    FileSizeBytes = v.FileSizeBytes,
                    ContentType = v.ContentType,
                    CreatedByUserId = v.CreatedByUserId,
                    CreatedDate = v.CreatedDate
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting version history for document {DocumentId}", documentId);
                throw;
            }
        }
        
        /// <summary>
        /// Deletes a specific version
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="versionNumber">Version number</param>
        /// <returns>True if successful</returns>
        public async Task<bool> DeleteVersionAsync(Guid documentId, int versionNumber)
        {
            try
            {
                _logger.LogInformation("Deleting version {Version} for document {DocumentId}", 
                    versionNumber, documentId);
                
                return await _versionedFileStorageService.DeleteFileVersionAsync(documentId, versionNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting version {Version} for document {DocumentId}", 
                    versionNumber, documentId);
                throw;
            }
        }
    }
}