// -----------------------------------------------------------------------------
// <copyright file="IVersionedFileStorageService.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 30, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        Interface for a file storage service that supports versioning
// -----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DocumentManagementML.Domain.Services
{
    /// <summary>
    /// Extended file storage service interface with versioning support
    /// </summary>
    public interface IVersionedFileStorageService : IFileStorageService
    {
        /// <summary>
        /// Saves a file as a new version
        /// </summary>
        /// <param name="documentId">Document ID for the file</param>
        /// <param name="fileStream">File content stream</param>
        /// <param name="fileName">Original file name</param>
        /// <param name="userId">ID of the user creating the version</param>
        /// <returns>Path to the saved version and the version number</returns>
        Task<(string FilePath, int VersionNumber)> SaveFileVersionAsync(Guid documentId, Stream fileStream, string fileName, Guid userId);
        
        /// <summary>
        /// Gets a specific version of a file
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="versionNumber">Version number (use 0 for latest)</param>
        /// <returns>File content stream and the complete file path</returns>
        Task<(Stream FileStream, string FilePath)> GetFileVersionAsync(Guid documentId, int versionNumber = 0);
        
        /// <summary>
        /// Gets metadata for all versions of a document
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <returns>List of file version metadata</returns>
        Task<IEnumerable<FileVersionMetadata>> GetVersionHistoryAsync(Guid documentId);
        
        /// <summary>
        /// Deletes a specific version of a file (if allowed by retention policy)
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="versionNumber">Version number to delete</param>
        /// <returns>True if successful, false if not found or not allowed to delete</returns>
        Task<bool> DeleteFileVersionAsync(Guid documentId, int versionNumber);
    }
    
    /// <summary>
    /// Metadata for a file version
    /// </summary>
    public class FileVersionMetadata
    {
        /// <summary>
        /// Document ID
        /// </summary>
        public Guid DocumentId { get; set; }
        
        /// <summary>
        /// Version number
        /// </summary>
        public int VersionNumber { get; set; }
        
        /// <summary>
        /// File path
        /// </summary>
        public string FilePath { get; set; }
        
        /// <summary>
        /// Original file name
        /// </summary>
        public string FileName { get; set; }
        
        /// <summary>
        /// File size in bytes
        /// </summary>
        public long FileSizeBytes { get; set; }
        
        /// <summary>
        /// Content type
        /// </summary>
        public string ContentType { get; set; }
        
        /// <summary>
        /// User ID who created this version
        /// </summary>
        public Guid CreatedByUserId { get; set; }
        
        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime CreatedDate { get; set; }
        
        /// <summary>
        /// Content hash for integrity verification
        /// </summary>
        public string ContentHash { get; set; }
    }
}