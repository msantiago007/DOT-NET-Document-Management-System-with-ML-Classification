// -----------------------------------------------------------------------------
// <copyright file="EnhancedDocumentService.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 29, 2025
// Last Modified:      April 29, 2025
// Version:            0.9.0
// Description:        Enhanced document service that leverages the Unit of Work pattern
//                     for better transaction management and consistent error handling.
// -----------------------------------------------------------------------------
using AutoMapper;
using DocumentManagementML.Application.DTOs;
using DocumentManagementML.Application.Exceptions;
using DocumentManagementML.Application.Interfaces;
using DocumentManagementML.Domain.Entities;
using DocumentManagementML.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace DocumentManagementML.Application.Services
{
    /// <summary>
    /// Enhanced document service that leverages the Unit of Work pattern
    /// </summary>
    public class EnhancedDocumentService : BaseApplicationService, IDocumentService
    {
        private readonly IDocumentClassificationService _classificationService;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<EnhancedDocumentService> _logger;

        /// <summary>
        /// Initializes a new instance of the EnhancedDocumentService class
        /// </summary>
        /// <param name="unitOfWork">Unit of work for transaction management</param>
        /// <param name="mapper">AutoMapper instance</param>
        /// <param name="classificationService">Document classification service</param>
        /// <param name="fileStorageService">File storage service</param>
        /// <param name="logger">Logger</param>
        public EnhancedDocumentService(
            IUnitOfWorkExtended unitOfWork,
            IMapper mapper,
            IDocumentClassificationService classificationService,
            IFileStorageService fileStorageService,
            ILogger<EnhancedDocumentService> logger)
            : base(unitOfWork, mapper, logger)
        {
            _classificationService = classificationService ?? throw new ArgumentNullException(nameof(classificationService));
            _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
            _logger = logger;
        }

        /// <summary>
        /// Gets a document by its identifier
        /// </summary>
        /// <param name="id">Document identifier</param>
        /// <returns>Document DTO if found, null otherwise</returns>
        public async Task<DocumentDto?> GetDocumentByIdAsync(Guid id)
        {
            try
            {
                // Get document with its metadata
                var document = await UnitOfWork.DocumentRepository.GetWithMetadataAsync(id);
                return document != null ? Mapper.Map<DocumentDto>(document) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document with ID {DocumentId}", id);
                throw;
            }
        }

        /// <summary>
        /// Gets all documents with pagination
        /// </summary>
        /// <param name="skip">Number of documents to skip</param>
        /// <param name="limit">Maximum number of documents to return</param>
        /// <returns>Paged collection of document DTOs</returns>
        public async Task<IEnumerable<DocumentDto>> GetDocumentsAsync(int skip = 0, int limit = 100)
        {
            try
            {
                var documents = await UnitOfWork.DocumentRepository.GetActiveDocumentsAsync(skip, limit);
                return Mapper.Map<IEnumerable<DocumentDto>>(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving documents with pagination (skip: {Skip}, limit: {Limit})", skip, limit);
                throw;
            }
        }

        /// <summary>
        /// Creates a new document with file upload
        /// </summary>
        /// <param name="documentDto">Document creation DTO</param>
        /// <param name="fileStream">Document file stream</param>
        /// <param name="fileName">Original file name</param>
        /// <returns>Created document DTO</returns>
        public async Task<DocumentDto> CreateDocumentAsync(DocumentCreateDto documentDto, Stream fileStream, string fileName)
        {
            return await ExecuteInTransactionAsync(async transaction =>
            {
                // Copy file stream to memory for processing
                using var ms = new MemoryStream();
                await fileStream.CopyToAsync(ms);
                
                // Calculate file hash for integrity check
                var fileHash = await GenerateFileHashAsync(ms);
                
                // Map to domain entity
                var document = Mapper.Map<Document>(documentDto);
                
                // Set document properties
                var storagePath = await _fileStorageService.StoreFileAsync(ms, fileName, Path.GetExtension(fileName).TrimStart('.').ToLower());
                document.FileLocation = storagePath;
                document.FileType = Path.GetExtension(fileName).TrimStart('.').ToLower();
                document.FileSizeBytes = ms.Length;
                document.CreatedDate = DateTime.UtcNow;
                document.LastModifiedDate = DateTime.UtcNow;
                document.IsDeleted = false;
                document.ContentHash = fileHash;
                
                // Validate document type if provided
                if (documentDto.DocumentTypeId.HasValue)
                {
                    var documentType = await UnitOfWork.DocumentTypeRepository.GetByIdAsync(documentDto.DocumentTypeId.Value);
                    if (documentType == null)
                    {
                        throw new ValidationException("DocumentTypeId", $"Document type with ID {documentDto.DocumentTypeId.Value} not found");
                    }
                }
                
                // Save document
                var createdDocument = await UnitOfWork.DocumentRepository.AddAsync(document);
                
                // Process metadata if provided
                if (documentDto.Metadata != null && documentDto.Metadata.Count > 0)
                {
                    foreach (var metadataItem in documentDto.Metadata)
                    {
                        var metadata = new DocumentMetadata
                        {
                            DocumentId = createdDocument.DocumentId,
                            MetadataKey = metadataItem.Key,
                            MetadataValue = metadataItem.Value,
                            DataType = DetermineDataType(metadataItem.Value),
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedDate = DateTime.UtcNow
                        };
                        
                        await UnitOfWork.DocumentMetadataRepository.AddAsync(metadata);
                    }
                }
                
                // Optional: Attempt to classify document
                try
                {
                    ms.Position = 0;
                    var classificationResult = await _classificationService.ClassifyDocumentAsync(ms, fileName);
                    
                    if (classificationResult.IsSuccessful && classificationResult.PredictedDocumentTypeId.HasValue)
                    {
                        // Update document with classification result if not already set by user
                        if (!document.DocumentTypeId.HasValue)
                        {
                            document.DocumentTypeId = classificationResult.PredictedDocumentTypeId;
                            await UnitOfWork.DocumentRepository.UpdateAsync(document);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log but continue - classification is a non-critical operation
                    _logger.LogWarning(ex, "Document classification failed for document {DocumentId}, but document creation will proceed", document.DocumentId);
                }
                
                return Mapper.Map<DocumentDto>(createdDocument);
            }, $"Error creating document {documentDto.Name}");
        }

        /// <summary>
        /// Updates an existing document
        /// </summary>
        /// <param name="id">Document identifier</param>
        /// <param name="documentDto">Document update DTO</param>
        /// <returns>Updated document DTO if found, null otherwise</returns>
        public async Task<DocumentDto?> UpdateDocumentAsync(Guid id, DocumentUpdateDto documentDto)
        {
            return await ExecuteInTransactionAsync(async transaction =>
            {
                // Get document with metadata
                var existingDocument = await UnitOfWork.DocumentRepository.GetWithMetadataAsync(id);
                if (existingDocument == null)
                {
                    return null;
                }

                // Update document properties
                existingDocument.DocumentName = documentDto.Name;
                existingDocument.Description = documentDto.Description;
                existingDocument.DocumentTypeId = documentDto.DocumentTypeId;
                existingDocument.LastModifiedDate = DateTime.UtcNow;

                // Validate document type if provided
                if (documentDto.DocumentTypeId.HasValue)
                {
                    var documentType = await UnitOfWork.DocumentTypeRepository.GetByIdAsync(documentDto.DocumentTypeId.Value);
                    if (documentType == null)
                    {
                        throw new ValidationException("DocumentTypeId", $"Document type with ID {documentDto.DocumentTypeId.Value} not found");
                    }
                }
                
                // Update document
                var updatedDocument = await UnitOfWork.DocumentRepository.UpdateAsync(existingDocument);
                
                // Update metadata if provided
                if (documentDto.Metadata != null)
                {
                    // First delete existing metadata
                    await UnitOfWork.DocumentMetadataRepository.DeleteByDocumentIdAsync(id);
                    
                    // Then add new metadata
                    foreach (var metadataItem in documentDto.Metadata)
                    {
                        var metadata = new DocumentMetadata
                        {
                            DocumentId = updatedDocument.DocumentId,
                            MetadataKey = metadataItem.Key,
                            MetadataValue = metadataItem.Value,
                            DataType = DetermineDataType(metadataItem.Value),
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedDate = DateTime.UtcNow
                        };
                        
                        await UnitOfWork.DocumentMetadataRepository.AddAsync(metadata);
                    }
                }
                
                return Mapper.Map<DocumentDto>(updatedDocument);
            }, $"Error updating document with ID {id}");
        }

        /// <summary>
        /// Soft deletes a document
        /// </summary>
        /// <param name="id">Document identifier</param>
        /// <returns>True if the document was deleted, false otherwise</returns>
        public async Task<bool> DeleteDocumentAsync(Guid id)
        {
            return await ExecuteInTransactionAsync(async transaction =>
            {
                var document = await UnitOfWork.DocumentRepository.GetByIdAsync(id);
                if (document == null)
                {
                    return false;
                }

                // Soft delete the document
                await UnitOfWork.DocumentRepository.SoftDeleteAsync(id);
                return true;
            }, $"Error deleting document with ID {id}");
        }

        /// <summary>
        /// Searches documents by term and optionally by document type with pagination
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <param name="documentTypeId">Optional document type identifier</param>
        /// <param name="skip">Number of documents to skip</param>
        /// <param name="limit">Maximum number of documents to return</param>
        /// <returns>Paged collection of document DTOs</returns>
        public async Task<IEnumerable<DocumentDto>> SearchDocumentsAsync(string searchTerm, Guid? documentTypeId = null, int skip = 0, int limit = 100)
        {
            try
            {
                var documents = await UnitOfWork.DocumentRepository.SearchAsync(searchTerm, documentTypeId, skip, limit);
                return Mapper.Map<IEnumerable<DocumentDto>>(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching documents with term {SearchTerm}", searchTerm);
                throw;
            }
        }

        /// <summary>
        /// Gets documents by document type with pagination
        /// </summary>
        /// <param name="typeId">Document type identifier</param>
        /// <param name="skip">Number of documents to skip</param>
        /// <param name="limit">Maximum number of documents to return</param>
        /// <returns>Paged collection of document DTOs</returns>
        public async Task<IEnumerable<DocumentDto>> GetDocumentsByTypeAsync(Guid typeId, int skip = 0, int limit = 100)
        {
            try
            {
                var documents = await UnitOfWork.DocumentRepository.GetByDocumentTypeAsync(typeId);
                return Mapper.Map<IEnumerable<DocumentDto>>(documents)
                    .Skip(skip)
                    .Take(limit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving documents by type ID {TypeId}", typeId);
                throw;
            }
        }

        /// <summary>
        /// Gets the total count of documents
        /// </summary>
        /// <param name="documentTypeId">Optional document type identifier for filtering</param>
        /// <returns>Total count of documents</returns>
        public async Task<int> GetDocumentCountAsync(Guid? documentTypeId = null)
        {
            try
            {
                return await UnitOfWork.DocumentRepository.GetDocumentCountAsync(documentTypeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document count with documentTypeId: {DocumentTypeId}", documentTypeId);
                return 0;
            }
        }

        /// <summary>
        /// Gets the total count of search results
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <param name="documentTypeId">Optional document type identifier for filtering</param>
        /// <returns>Total count of search results</returns>
        public async Task<int> GetSearchResultCountAsync(string searchTerm, Guid? documentTypeId = null)
        {
            try
            {
                return await UnitOfWork.DocumentRepository.GetSearchResultCountAsync(searchTerm, documentTypeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving search result count with term {SearchTerm} and documentTypeId: {DocumentTypeId}", searchTerm, documentTypeId);
                return 0;
            }
        }

        /// <summary>
        /// Gets recently added documents
        /// </summary>
        /// <param name="count">Maximum number of documents to return</param>
        /// <returns>Collection of document DTOs</returns>
        public async Task<IEnumerable<DocumentDto>> GetRecentDocumentsAsync(int count)
        {
            try
            {
                var documents = await UnitOfWork.DocumentRepository.GetRecentDocumentsAsync(count);
                return Mapper.Map<IEnumerable<DocumentDto>>(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent documents with count: {Count}", count);
                return Enumerable.Empty<DocumentDto>();
            }
        }

        private async Task<string> GenerateFileHashAsync(Stream fileStream)
        {
            using (var sha256 = SHA256.Create())
            {
                fileStream.Position = 0;
                var hashBytes = await sha256.ComputeHashAsync(fileStream);
                fileStream.Position = 0; // Reset position for further processing
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }
        
        private string DetermineDataType(string value)
        {
            // Simple data type detection
            if (DateTime.TryParse(value, out _))
                return "date";
            
            if (decimal.TryParse(value, out _))
                return "number";
            
            if (bool.TryParse(value, out _))
                return "boolean";
            
            if (value.StartsWith("{") && value.EndsWith("}") || value.StartsWith("[") && value.EndsWith("]"))
                return "json";
            
            return "string";
        }
    }
}