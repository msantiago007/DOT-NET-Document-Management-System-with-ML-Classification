// DocumentService.cs

using AutoMapper;
using DocumentManagementML.Application.DTOs;
using DocumentManagementML.Application.Exceptions;
using DocumentManagementML.Application.Interfaces;
using DocumentManagementML.Domain.Entities;
using DocumentManagementML.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore.Storage;

namespace DocumentManagementML.Application.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IDocumentTypeRepository _documentTypeRepository;
        private readonly IUserService _userService;
        private readonly IDocumentMetadataRepository _documentMetadataRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<DocumentService> _logger;

        public DocumentService(
            IDocumentRepository documentRepository,
            IDocumentTypeRepository documentTypeRepository,
            IUserService userService,
            IDocumentMetadataRepository documentMetadataRepository,
            IMapper mapper,
            ILogger<DocumentService> logger)
        {
            _documentRepository = documentRepository;
            _documentTypeRepository = documentTypeRepository;
            _userService = userService;
            _documentMetadataRepository = documentMetadataRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<DocumentDto>> GetDocumentsAsync(int skip = 0, int limit = 100)
        {
            try
            {
                var documents = await _documentRepository.GetActiveDocumentsAsync(skip, limit);
                var totalCount = await _documentRepository.GetDocumentCountAsync();
                
                var result = _mapper.Map<IEnumerable<DocumentDto>>(documents);
                
                // If needed, you could add pagination metadata to the response here
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving documents with pagination (skip: {Skip}, limit: {Limit})", skip, limit);
                throw;
            }
        }

        public async Task<DocumentDto?> GetDocumentByIdAsync(Guid id)
        {
            try
            {
                // Get document with its metadata
                var document = await _documentRepository.GetWithMetadataAsync(id);
                return document != null ? _mapper.Map<DocumentDto>(document) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document with ID {DocumentId}", id);
                throw;
            }
        }

        public async Task<DocumentDto> CreateDocumentAsync(DocumentCreateDto documentDto, Stream fileStream, string fileName)
        {
            IDbContextTransaction? transaction = null;
            
            try
            {
                // Start transaction
                transaction = await _documentRepository.BeginTransactionAsync();
                
                var document = _mapper.Map<Document>(documentDto);
                
                // Process file
                using (var ms = new MemoryStream())
                {
                    await fileStream.CopyToAsync(ms);
                    
                    // Calculate file hash for integrity check
                    var fileHash = await GenerateFileHashAsync(ms);
                    
                    // Prepare document entity
                    document.FileLocation = $"storage/{Guid.NewGuid()}_{fileName}"; // Placeholder path
                    document.FileType = Path.GetExtension(fileName).TrimStart('.').ToLower();
                    document.FileSizeBytes = ms.Length;
                    document.CreatedDate = DateTime.UtcNow;
                    document.LastModifiedDate = DateTime.UtcNow;
                    document.IsDeleted = false;
                    document.ContentHash = fileHash;
                }
                
                // Validate document type if provided
                if (documentDto.DocumentTypeId.HasValue)
                {
                    var documentType = await _documentTypeRepository.GetByIdAsync(documentDto.DocumentTypeId.Value);
                    if (documentType == null)
                    {
                        throw new ValidationException($"Document type with ID {documentDto.DocumentTypeId.Value} not found");
                    }
                }
                
                // Save document
                var createdDocument = await _documentRepository.AddAsync(document);
                
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
                        
                        await _documentMetadataRepository.AddAsync(metadata);
                    }
                }
                
                // Commit transaction
                await _documentRepository.CommitTransactionAsync(transaction);
                
                // Return mapped DTO
                return _mapper.Map<DocumentDto>(createdDocument);
            }
            catch (Exception ex)
            {
                // Rollback transaction if an error occurred
                if (transaction != null)
                {
                    await _documentRepository.RollbackTransactionAsync(transaction);
                }
                
                _logger.LogError(ex, "Error creating document {DocumentName}", documentDto.Name);
                throw;
            }
        }

        public async Task<DocumentDto?> UpdateDocumentAsync(Guid id, DocumentUpdateDto documentDto)
        {
            IDbContextTransaction? transaction = null;
            
            try
            {
                // Start transaction
                transaction = await _documentRepository.BeginTransactionAsync();
                
                // Get document with metadata
                var existingDocument = await _documentRepository.GetWithMetadataAsync(id);
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
                    var documentType = await _documentTypeRepository.GetByIdAsync(documentDto.DocumentTypeId.Value);
                    if (documentType == null)
                    {
                        throw new ValidationException($"Document type with ID {documentDto.DocumentTypeId.Value} not found");
                    }
                }
                
                // Update document
                var updatedDocument = await _documentRepository.UpdateAsync(existingDocument);
                
                // Update metadata if provided
                if (documentDto.Metadata != null)
                {
                    // First delete existing metadata
                    await _documentMetadataRepository.DeleteByDocumentIdAsync(id);
                    
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
                        
                        await _documentMetadataRepository.AddAsync(metadata);
                    }
                }
                
                // Commit transaction
                await _documentRepository.CommitTransactionAsync(transaction);
                
                // Return mapped DTO
                return _mapper.Map<DocumentDto>(updatedDocument);
            }
            catch (Exception ex)
            {
                // Rollback transaction if an error occurred
                if (transaction != null)
                {
                    await _documentRepository.RollbackTransactionAsync(transaction);
                }
                
                _logger.LogError(ex, "Error updating document with ID {DocumentId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteDocumentAsync(Guid id)
        {
            IDbContextTransaction? transaction = null;
            
            try
            {
                // Start transaction
                transaction = await _documentRepository.BeginTransactionAsync();
                
                var document = await _documentRepository.GetByIdAsync(id);
                if (document == null)
                {
                    return false;
                }

                // Soft delete the document
                await _documentRepository.SoftDeleteAsync(id);
                
                // Commit transaction
                await _documentRepository.CommitTransactionAsync(transaction);
                
                return true;
            }
            catch (Exception ex)
            {
                // Rollback transaction if an error occurred
                if (transaction != null)
                {
                    await _documentRepository.RollbackTransactionAsync(transaction);
                }
                
                _logger.LogError(ex, "Error deleting document with ID {DocumentId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<DocumentDto>> SearchDocumentsAsync(string searchTerm, Guid? documentTypeId = null, int skip = 0, int limit = 100)
        {
            try
            {
                var documents = await _documentRepository.SearchAsync(searchTerm, documentTypeId, skip, limit);
                var totalCount = await _documentRepository.GetSearchResultCountAsync(searchTerm, documentTypeId);
                
                var result = _mapper.Map<IEnumerable<DocumentDto>>(documents);
                
                // If needed, you could add pagination metadata to the response here
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching documents with term {SearchTerm}", searchTerm);
                throw;
            }
        }

        public async Task<IEnumerable<DocumentDto>> GetDocumentsByTypeAsync(Guid typeId, int skip = 0, int limit = 100)
        {
            try
            {
                var documents = await _documentRepository.GetByDocumentTypeAsync(typeId);
                return _mapper.Map<IEnumerable<DocumentDto>>(documents)
                    .Skip(skip)
                    .Take(limit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving documents by type ID {TypeId}", typeId);
                throw;
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
        
        /// <summary>
        /// Gets the total count of documents
        /// </summary>
        /// <param name="documentTypeId">Optional document type identifier for filtering</param>
        /// <returns>Total count of documents</returns>
        public async Task<int> GetDocumentCountAsync(Guid? documentTypeId = null)
        {
            try
            {
                return await _documentRepository.GetDocumentCountAsync(documentTypeId);
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
                return await _documentRepository.GetSearchResultCountAsync(searchTerm, documentTypeId);
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
                var documents = await _documentRepository.GetRecentDocumentsAsync(count);
                return _mapper.Map<IEnumerable<DocumentDto>>(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent documents with count: {Count}", count);
                return Enumerable.Empty<DocumentDto>();
            }
        }
    }
}