// DocumentTypeService.cs
using AutoMapper;
using DocumentManagementML.Application.DTOs;
using DocumentManagementML.Application.Exceptions;
using DocumentManagementML.Application.Interfaces;
using DocumentManagementML.Domain.Entities;
using DocumentManagementML.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocumentManagementML.Application.Services
{
    /// <summary>
    /// Service implementation for document type operations
    /// </summary>
    public class DocumentTypeService : IDocumentTypeService
    {
        private readonly IDocumentTypeRepository _documentTypeRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<DocumentTypeService> _logger;

        /// <summary>
        /// Initializes a new instance of the DocumentTypeService class
        /// </summary>
        /// <param name="documentTypeRepository">Document type repository</param>
        /// <param name="mapper">AutoMapper instance</param>
        /// <param name="logger">Logger</param>
        public DocumentTypeService(
            IDocumentTypeRepository documentTypeRepository,
            IMapper mapper,
            ILogger<DocumentTypeService> logger)
        {
            _documentTypeRepository = documentTypeRepository;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Gets a document type by its identifier
        /// </summary>
        /// <param name="id">Document type identifier</param>
        /// <returns>Document type DTO</returns>
        public async Task<DocumentTypeDto> GetDocumentTypeByIdAsync(int id)
        {
            var documentType = await _documentTypeRepository.GetByIdAsync(id);
            if (documentType == null || !documentType.IsActive)
            {
                throw new NotFoundException($"Document type with ID {id} not found");
            }

            return _mapper.Map<DocumentTypeDto>(documentType);
        }

        /// <summary>
        /// Gets all document types with pagination
        /// </summary>
        /// <param name="skip">Number of types to skip</param>
        /// <param name="limit">Maximum number of types to return</param>
        /// <returns>Collection of document type DTOs</returns>
        public async Task<IEnumerable<DocumentTypeDto>> GetDocumentTypesAsync(int skip = 0, int limit = 100)
        {
            var documentTypes = await _documentTypeRepository.GetActiveTypesAsync();
            return _mapper.Map<IEnumerable<DocumentTypeDto>>(documentTypes)
                .Skip(skip)
                .Take(limit);
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
                _logger.LogInformation($"Creating document type: {documentTypeDto.TypeName}");
                
                var documentType = new DocumentType
                {
                    TypeName = documentTypeDto.TypeName,
                    Description = documentTypeDto.Description,
                    SchemaDefinition = documentTypeDto.SchemaDefinition,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow
                };
                
                await _documentTypeRepository.AddAsync(documentType);
                await _documentTypeRepository.SaveChangesAsync();
                
                _logger.LogInformation($"Document type created with ID: {documentType.DocumentTypeId}");
                return _mapper.Map<DocumentTypeDto>(documentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating document type: {ex.Message}");
                throw new ApplicationException($"Error creating document type: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates an existing document type
        /// </summary>
        /// <param name="id">Document type identifier</param>
        /// <param name="documentTypeDto">Document type update DTO</param>
        /// <returns>Updated document type DTO</returns>
        public async Task<DocumentTypeDto> UpdateDocumentTypeAsync(int id, DocumentTypeUpdateDto documentTypeDto)
        {
            var documentType = await _documentTypeRepository.GetByIdAsync(id);
            if (documentType == null || !documentType.IsActive)
            {
                throw new NotFoundException($"Document type with ID {id} not found");
            }
            
            // Update properties if provided
            if (documentTypeDto.TypeName != null)
            {
                documentType.TypeName = documentTypeDto.TypeName;
            }
            
            if (documentTypeDto.Description != null)
            {
                documentType.Description = documentTypeDto.Description;
            }
            
            if (documentTypeDto.SchemaDefinition != null)
            {
                documentType.SchemaDefinition = documentTypeDto.SchemaDefinition;
            }
            
            if (documentTypeDto.IsActive.HasValue)
            {
                documentType.IsActive = documentTypeDto.IsActive.Value;
            }
            
            documentType.LastModifiedDate = DateTime.UtcNow;
            
            try
            {
                await _documentTypeRepository.UpdateAsync(documentType);
                await _documentTypeRepository.SaveChangesAsync();
                
                _logger.LogInformation($"Document type updated: {id}");
                return _mapper.Map<DocumentTypeDto>(documentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating document type: {ex.Message}");
                throw new ApplicationException($"Error updating document type: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Deactivates a document type
        /// </summary>
        /// <param name="id">Document type identifier</param>
        public async Task DeactivateDocumentTypeAsync(int id)
        {
            var documentType = await _documentTypeRepository.GetByIdAsync(id);
            if (documentType == null || !documentType.IsActive)
            {
                throw new NotFoundException($"Document type with ID {id} not found");
            }
            
            try
            {
                await _documentTypeRepository.DeactivateAsync(id);
                await _documentTypeRepository.SaveChangesAsync();
                
                _logger.LogInformation($"Document type deactivated: {id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deactivating document type: {ex.Message}");
                throw new ApplicationException($"Error deactivating document type: {ex.Message}", ex);
            }
        }
    }
}