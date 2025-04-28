// DocumentTypeService.cs
using AutoMapper;
using DocumentManagementML.Application.DTOs;
using DocumentManagementML.Application.Exceptions;
using DocumentManagementML.Application.Interfaces;
using DocumentManagementML.Domain.Entities;
using DocumentManagementML.Domain.Repositories;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DocumentManagementML.Application.Services
{
    /// <summary>
    /// Service implementation for document type operations
    /// </summary>
    public class DocumentTypeService : IDocumentTypeService
    {
        private readonly IDocumentTypeRepository _documentTypeRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<DocumentTypeService> _logger;

        /// <summary>
        /// Initializes a new instance of the DocumentTypeService class
        /// </summary>
        /// <param name="documentTypeRepository">Document type repository</param>
        /// <param name="documentRepository">Document repository</param>
        /// <param name="mapper">AutoMapper instance</param>
        /// <param name="logger">Logger</param>
        public DocumentTypeService(
            IDocumentTypeRepository documentTypeRepository,
            IDocumentRepository documentRepository,
            IMapper mapper,
            ILogger<DocumentTypeService> logger)
        {
            _documentTypeRepository = documentTypeRepository;
            _documentRepository = documentRepository;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Gets all document types
        /// </summary>
        /// <returns>Collection of document type DTOs</returns>
        public async Task<IEnumerable<DocumentTypeDto>> GetAllDocumentTypesAsync()
        {
            try
            {
                var documentTypes = await _documentTypeRepository.GetAllAsync();
                return _mapper.Map<IEnumerable<DocumentTypeDto>>(documentTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all document types");
                throw;
            }
        }

        /// <summary>
        /// Gets a document type by its identifier
        /// </summary>
        /// <param name="id">Document type identifier</param>
        /// <returns>Document type DTO</returns>
        public async Task<DocumentTypeDto?> GetDocumentTypeByIdAsync(Guid id)
        {
            try
            {
                var documentType = await _documentTypeRepository.GetByIdAsync(id);
                return documentType != null ? _mapper.Map<DocumentTypeDto>(documentType) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document type with ID {DocumentTypeId}", id);
                throw;
            }
        }

        /// <summary>
        /// Creates a new document type
        /// </summary>
        /// <param name="documentTypeDto">Document type creation DTO</param>
        /// <returns>Created document type DTO</returns>
        public async Task<DocumentTypeDto> CreateDocumentTypeAsync(DocumentTypeCreateDto documentTypeDto)
        {
            try
            {
                // Check if a document type with the same name already exists
                var existingDocumentType = await _documentTypeRepository.GetByNameAsync(documentTypeDto.Name);
                if (existingDocumentType != null)
                {
                    throw new ValidationException($"A document type with the name '{documentTypeDto.Name}' already exists");
                }

                var documentType = _mapper.Map<DocumentType>(documentTypeDto);
                documentType.CreatedDate = DateTime.UtcNow;
                documentType.LastModifiedDate = DateTime.UtcNow;
                
                var createdDocumentType = await _documentTypeRepository.AddAsync(documentType);
                return _mapper.Map<DocumentTypeDto>(createdDocumentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating document type {DocumentTypeName}", documentTypeDto.Name);
                throw;
            }
        }

        /// <summary>
        /// Updates an existing document type
        /// </summary>
        /// <param name="id">Document type identifier</param>
        /// <param name="documentTypeDto">Document type update DTO</param>
        /// <returns>Updated document type DTO</returns>
        public async Task<DocumentTypeDto?> UpdateDocumentTypeAsync(Guid id, DocumentTypeUpdateDto documentTypeDto)
        {
            IDbContextTransaction? transaction = null;
            
            try
            {
                // Start transaction
                transaction = await _documentTypeRepository.BeginTransactionAsync();
                
                var existingDocumentType = await _documentTypeRepository.GetByIdAsync(id);
                if (existingDocumentType == null)
                {
                    return null;
                }

                // Check if name is being changed and if new name already exists
                if (documentTypeDto.Name != existingDocumentType.TypeName)
                {
                    var nameExists = await _documentTypeRepository.GetByNameAsync(documentTypeDto.Name);
                    if (nameExists != null && nameExists.DocumentTypeId != id)
                    {
                        throw new ValidationException($"A document type with the name '{documentTypeDto.Name}' already exists");
                    }
                }

                // Update properties
                existingDocumentType.TypeName = documentTypeDto.Name;
                existingDocumentType.Description = documentTypeDto.Description;
                existingDocumentType.IsActive = documentTypeDto.IsActive;
                existingDocumentType.LastModifiedDate = DateTime.UtcNow;

                var updatedDocumentType = await _documentTypeRepository.UpdateAsync(existingDocumentType);
                
                // Commit transaction
                await _documentTypeRepository.CommitTransactionAsync(transaction);
                
                return _mapper.Map<DocumentTypeDto>(updatedDocumentType);
            }
            catch (Exception ex)
            {
                // Rollback transaction if an error occurred
                if (transaction != null)
                {
                    await _documentTypeRepository.RollbackTransactionAsync(transaction);
                }
                
                _logger.LogError(ex, "Error updating document type with ID {DocumentTypeId}", id);
                throw;
            }
        }

        /// <summary>
        /// Deletes a document type
        /// </summary>
        /// <param name="id">Document type identifier</param>
        /// <returns>True if the document type was deleted, false otherwise</returns>
        public async Task<bool> DeleteDocumentTypeAsync(Guid id)
        {
            IDbContextTransaction? transaction = null;
            
            try
            {
                // Start transaction
                transaction = await _documentTypeRepository.BeginTransactionAsync();
                
                var documentType = await _documentTypeRepository.GetByIdAsync(id);
                if (documentType == null)
                {
                    return false;
                }

                // Check if there are documents using this type
                var documents = await _documentRepository.GetByDocumentTypeAsync(id);
                if (documents.Any())
                {
                    throw new ValidationException("Cannot delete document type that is in use by documents");
                }

                var result = await _documentTypeRepository.DeleteAsync(id);
                
                // Commit transaction
                await _documentTypeRepository.CommitTransactionAsync(transaction);
                
                return result;
            }
            catch (Exception ex)
            {
                // Rollback transaction if an error occurred
                if (transaction != null)
                {
                    await _documentTypeRepository.RollbackTransactionAsync(transaction);
                }
                
                _logger.LogError(ex, "Error deleting document type with ID {DocumentTypeId}", id);
                throw;
            }
        }

        /// <summary>
        /// Gets document types with pagination
        /// </summary>
        /// <param name="skip">Number of types to skip</param>
        /// <param name="limit">Maximum number of types to return</param>
        /// <returns>Paged collection of document type DTOs</returns>
        public async Task<IEnumerable<DocumentTypeDto>> GetDocumentTypesAsync(int skip = 0, int limit = 100)
        {
            try
            {
                var documentTypes = await _documentTypeRepository.GetAllDocumentTypesAsync(skip, limit);
                var totalCount = await _documentTypeRepository.GetDocumentTypeCountAsync();
                
                var result = _mapper.Map<IEnumerable<DocumentTypeDto>>(documentTypes);
                
                // If needed, you could add pagination metadata to the response here
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document types with pagination (skip: {Skip}, limit: {Limit})", skip, limit);
                throw;
            }
        }

        /// <summary>
        /// Deactivates a document type (soft delete)
        /// </summary>
        /// <param name="id">Document type identifier</param>
        public async Task DeactivateDocumentTypeAsync(Guid id)
        {
            IDbContextTransaction? transaction = null;
            
            try 
            {
                // Start transaction
                transaction = await _documentTypeRepository.BeginTransactionAsync();
                
                var documentType = await _documentTypeRepository.GetByIdAsync(id);
                if (documentType == null)
                {
                    throw new NotFoundException($"Document type with ID {id} not found");
                }
                
                documentType.IsActive = false;
                documentType.LastModifiedDate = DateTime.UtcNow;
                
                await _documentTypeRepository.UpdateAsync(documentType);
                
                // Commit transaction
                await _documentTypeRepository.CommitTransactionAsync(transaction);
                
                _logger.LogInformation("Document type with ID {DocumentTypeId} deactivated", id);
            }
            catch (Exception ex)
            {
                // Rollback transaction if an error occurred
                if (transaction != null)
                {
                    await _documentTypeRepository.RollbackTransactionAsync(transaction);
                }
                
                _logger.LogError(ex, "Error deactivating document type with ID {DocumentTypeId}", id);
                throw;
            }
        }
        
        /// <summary>
        /// Gets active document types with pagination
        /// </summary>
        /// <param name="skip">Number of types to skip</param>
        /// <param name="limit">Maximum number of types to return</param>
        /// <returns>Paged collection of active document type DTOs</returns>
        public async Task<IEnumerable<DocumentTypeDto>> GetActiveDocumentTypesAsync(int skip = 0, int limit = 100)
        {
            try
            {
                var documentTypes = await _documentTypeRepository.GetActiveDocumentTypesAsync(skip, limit);
                var totalCount = await _documentTypeRepository.GetDocumentTypeCountAsync(activeOnly: true);
                
                var result = _mapper.Map<IEnumerable<DocumentTypeDto>>(documentTypes);
                
                // If needed, you could add pagination metadata to the response here
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active document types with pagination (skip: {Skip}, limit: {Limit})", skip, limit);
                throw;
            }
        }
        
        /// <summary>
        /// Gets the total count of document types
        /// </summary>
        /// <param name="activeOnly">If true, only count active document types</param>
        /// <returns>Total count of document types</returns>
        public async Task<int> GetDocumentTypeCountAsync(bool activeOnly = false)
        {
            try
            {
                return await _documentTypeRepository.GetDocumentTypeCountAsync(activeOnly);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document type count with activeOnly: {ActiveOnly}", activeOnly);
                return 0;
            }
        }
        
        /// <summary>
        /// Gets a document type by name
        /// </summary>
        /// <param name="name">Document type name</param>
        /// <returns>Document type DTO if found, null otherwise</returns>
        public async Task<DocumentTypeDto?> GetDocumentTypeByNameAsync(string name)
        {
            try
            {
                var documentType = await _documentTypeRepository.GetByNameAsync(name);
                return documentType != null ? _mapper.Map<DocumentTypeDto>(documentType) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document type by name {Name}", name);
                return null;
            }
        }
    }
}