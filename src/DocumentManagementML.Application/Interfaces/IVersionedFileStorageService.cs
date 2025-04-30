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
// Description:        Application layer interface for versioned file storage
// -----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DocumentManagementML.Application.Interfaces
{
    /// <summary>
    /// Interface for application-level versioned file storage operations
    /// </summary>
    public interface IVersionedFileStorageService : IFileStorageService
    {
        /// <summary>
        /// Saves a file with versioning
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="fileStream">File content</param>
        /// <param name="fileName">Original file name</param>
        /// <param name="userId">User ID who is saving the file</param>
        /// <returns>Version information tuple (FilePath, VersionNumber)</returns>
        Task<(string FilePath, int VersionNumber)> SaveVersionAsync(Guid documentId, Stream fileStream, string fileName, Guid userId);
        
        /// <summary>
        /// Gets a specific version of a file
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="versionNumber">Version number (0 for latest)</param>
        /// <returns>File content and file path</returns>
        Task<(Stream Content, string FilePath)> GetVersionAsync(Guid documentId, int versionNumber = 0);
        
        /// <summary>
        /// Gets version history for a document
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <returns>List of version information</returns>
        Task<IEnumerable<FileVersionInfo>> GetVersionHistoryAsync(Guid documentId);
        
        /// <summary>
        /// Deletes a specific version
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="versionNumber">Version number</param>
        /// <returns>True if successful</returns>
        Task<bool> DeleteVersionAsync(Guid documentId, int versionNumber);
    }
    
    /// <summary>
    /// File version information for application layer
    /// </summary>
    public class FileVersionInfo
    {
        /// <summary>
        /// Version number
        /// </summary>
        public int VersionNumber { get; set; }
        
        /// <summary>
        /// File name
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
        /// User who created this version
        /// </summary>
        public Guid CreatedByUserId { get; set; }
        
        /// <summary>
        /// Date when this version was created
        /// </summary>
        public DateTime CreatedDate { get; set; }
    }
}