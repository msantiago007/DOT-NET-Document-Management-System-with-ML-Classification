// -----------------------------------------------------------------------------
// <copyright file="EnhancedDocumentTypeService.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 29, 2025
// Last Modified:      April 29, 2025
// Version:            0.9.0
// Description:        Enhanced service for document type management with improved
//                     transaction handling and validation.
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
using System.Linq;
using System.Threading.Tasks;

namespace DocumentManagementML.Application.Services
{
    /// <summary>
    /// Enhanced service for document type management with improved transaction handling and validation
    /// </summary>
    public class EnhancedDocumentTypeService : BaseApplicationService, IDocumentTypeService
    {
        private readonly ILogger<EnhancedDocumentTypeService> _logger;

        /// <summary>
        /// Initializes a new instance of the EnhancedDocumentTypeService class
        /// </summary>
        /// <param name="unitOfWork">Unit of work for coordinating multiple repositories</param>
        /// <param name="mapper">AutoMapper instance</param>
        /// <param name="logger">Logger</param>
        public EnhancedDocumentTypeService(
            IUnitOfWorkExtended unitOfWork,
            IMapper mapper,
            ILogger<EnhancedDocumentTypeService> logger)
            : base(unitOfWork, mapper, logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Gets a document type by its identifier
        /// </summary>
        /// <param name="id">Document type identifier</param>
        /// <returns>Document type DTO if found, null otherwise</returns>
        public async Task<DocumentTypeDto?> GetDocumentTypeByIdAsync(Guid id)
        {
            try
            {
                var documentType = await UnitOfWork.DocumentTypeRepository.GetByIdAsync(id);
                return documentType != null ? Mapper.Map<DocumentTypeDto>(documentType) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document type with ID {DocumentTypeId}", id);
                throw;
            }
        }

        /// <summary>
        /// Gets all document types
        /// </summary>
        /// <returns>Collection of document type DTOs</returns>
        public async Task<IEnumerable<DocumentTypeDto>> GetAllDocumentTypesAsync()
        {
            try
            {
                var documentTypes = await UnitOfWork.DocumentTypeRepository.GetAllAsync();
                return Mapper.Map<IEnumerable<DocumentTypeDto>>(documentTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all document types");
                throw;
            }
        }

        /// <summary>
        /// Gets all active document types
        /// </summary>
        /// <returns>Collection of active document type DTOs</returns>
        public async Task<IEnumerable<DocumentTypeDto>> GetActiveDocumentTypesAsync()
        {
            try
            {
                var documentTypes = await UnitOfWork.DocumentTypeRepository.GetActiveTypesAsync();
                return Mapper.Map<IEnumerable<DocumentTypeDto>>(documentTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active document types");
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
            return await ExecuteInTransactionAsync(async transaction =>
            {
                // Check if a document type with the same name already exists
                var existingDocumentType = await UnitOfWork.DocumentTypeRepository.GetByNameAsync(documentTypeDto.Name);
                if (existingDocumentType != null)
                {
                    throw new ValidationException("Name", $"A document type with the name '{documentTypeDto.Name}' already exists");
                }

                var documentType = Mapper.Map<DocumentType>(documentTypeDto);
                documentType.TypeName = documentTypeDto.Name.Replace(" ", "").ToLower();
                documentType.CreatedDate = DateTime.UtcNow;
                documentType.LastModifiedDate = DateTime.UtcNow;
                
                var createdDocumentType = await UnitOfWork.DocumentTypeRepository.AddAsync(documentType);
                
                return Mapper.Map<DocumentTypeDto>(createdDocumentType);
            }, $"Error creating document type {documentTypeDto.Name}");
        }

        /// <summary>
        /// Updates an existing document type
        /// </summary>
        /// <param name="id">Document type identifier</param>
        /// <param name="documentTypeDto">Document type update DTO</param>
        /// <returns>Updated document type DTO if found, null otherwise</returns>
        public async Task<DocumentTypeDto?> UpdateDocumentTypeAsync(Guid id, DocumentTypeUpdateDto documentTypeDto)
        {
            return await ExecuteInTransactionAsync(async transaction =>
            {
                var existingDocumentType = await UnitOfWork.DocumentTypeRepository.GetByIdAsync(id);
                if (existingDocumentType == null)
                {
                    return null;
                }

                // If the name is being changed, check that the new name doesn't already exist
                if (existingDocumentType.Name != documentTypeDto.Name)
                {
                    var duplicateType = await UnitOfWork.DocumentTypeRepository.GetByNameAsync(documentTypeDto.Name);
                    if (duplicateType != null && duplicateType.DocumentTypeId != id)
                    {
                        throw new ValidationException("Name", $"A document type with the name '{documentTypeDto.Name}' already exists");
                    }
                }

                // Update properties
                existingDocumentType.Name = documentTypeDto.Name;
                existingDocumentType.TypeName = documentTypeDto.Name.Replace(" ", "").ToLower();
                existingDocumentType.Description = documentTypeDto.Description;
                existingDocumentType.IsActive = documentTypeDto.IsActive;
                existingDocumentType.LastModifiedDate = DateTime.UtcNow;
                
                var updatedDocumentType = await UnitOfWork.DocumentTypeRepository.UpdateAsync(existingDocumentType);
                
                return Mapper.Map<DocumentTypeDto>(updatedDocumentType);
            }, $"Error updating document type with ID {id}");
        }

        /// <summary>
        /// Gets all document types with pagination
        /// </summary>
        /// <param name="skip">Number of document types to skip</param>
        /// <param name="limit">Maximum number of document types to return</param>
        /// <returns>Paged collection of document type DTOs</returns>
        public async Task<IEnumerable<DocumentTypeDto>> GetDocumentTypesAsync(int skip = 0, int limit = 100)
        {
            try
            {
                var documentTypes = await UnitOfWork.DocumentTypeRepository.GetPagedAsync(skip, limit);
                return Mapper.Map<IEnumerable<DocumentTypeDto>>(documentTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document types with pagination (skip: {Skip}, limit: {Limit})", skip, limit);
                throw;
            }
        }
        
        /// <summary>
        /// Gets all active document types with pagination
        /// </summary>
        /// <param name="skip">Number of document types to skip</param>
        /// <param name="limit">Maximum number of document types to return</param>
        /// <returns>Paged collection of active document type DTOs</returns>
        public async Task<IEnumerable<DocumentTypeDto>> GetActiveDocumentTypesAsync(int skip = 0, int limit = 100)
        {
            try
            {
                var allActiveTypes = await UnitOfWork.DocumentTypeRepository.GetActiveTypesAsync();
                return Mapper.Map<IEnumerable<DocumentTypeDto>>(allActiveTypes)
                    .Skip(skip)
                    .Take(limit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active document types with pagination (skip: {Skip}, limit: {Limit})", skip, limit);
                throw;
            }
        }
        
        /// <summary>
        /// Deactivates a document type (soft delete)
        /// </summary>
        /// <param name="id">Document type identifier</param>
        public async Task DeactivateDocumentTypeAsync(Guid id)
        {
            await ExecuteInTransactionAsync(async transaction =>
            {
                var documentType = await UnitOfWork.DocumentTypeRepository.GetByIdAsync(id);
                if (documentType == null)
                {
                    throw new NotFoundException($"Document type with ID {id} not found");
                }
                
                documentType.IsActive = false;
                documentType.LastModifiedDate = DateTime.UtcNow;
                
                await UnitOfWork.DocumentTypeRepository.UpdateAsync(documentType);
            }, $"Error deactivating document type with ID {id}");
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
                if (activeOnly)
                {
                    var activeTypes = await UnitOfWork.DocumentTypeRepository.GetActiveTypesAsync();
                    return activeTypes.Count();
                }
                else
                {
                    var allTypes = await UnitOfWork.DocumentTypeRepository.GetAllAsync();
                    return allTypes.Count();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document type count (activeOnly: {ActiveOnly})", activeOnly);
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
                var documentType = await UnitOfWork.DocumentTypeRepository.GetByNameAsync(name);
                return documentType != null ? Mapper.Map<DocumentTypeDto>(documentType) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document type with name {Name}", name);
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
            return await ExecuteInTransactionAsync(async transaction =>
            {
                // Check if document type exists
                var documentType = await UnitOfWork.DocumentTypeRepository.GetByIdAsync(id);
                if (documentType == null)
                {
                    return false;
                }

                // Check if the document type is in use by any documents
                var documentsWithType = await UnitOfWork.DocumentRepository.GetByDocumentTypeAsync(id);
                if (documentsWithType.Any())
                {
                    throw new ValidationException("Cannot delete document type that is in use by documents");
                }

                return await UnitOfWork.DocumentTypeRepository.DeleteAsync(id);
            }, $"Error deleting document type with ID {id}");
        }
    }
}