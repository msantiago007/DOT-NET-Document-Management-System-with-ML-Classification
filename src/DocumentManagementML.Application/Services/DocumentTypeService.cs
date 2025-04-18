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
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
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
                var documentType = _mapper.Map<DocumentType>(documentTypeDto);
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
            try
            {
                var existingDocumentType = await _documentTypeRepository.GetByIdAsync(id);
                if (existingDocumentType == null)
                {
                    return null;
                }

                // Update properties
                existingDocumentType.Name = documentTypeDto.Name;
                existingDocumentType.Description = documentTypeDto.Description;
                existingDocumentType.IsActive = documentTypeDto.IsActive;

                var updatedDocumentType = await _documentTypeRepository.UpdateAsync(existingDocumentType);
                return _mapper.Map<DocumentTypeDto>(updatedDocumentType);
            }
            catch (Exception ex)
            {
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
            try
            {
                var documentType = await _documentTypeRepository.GetByIdAsync(id);
                if (documentType == null)
                {
                    return false;
                }

                return await _documentTypeRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document type with ID {DocumentTypeId}", id);
                throw;
            }
        }

        // Additional methods to implement the interface
        public async Task<IEnumerable<DocumentTypeDto>> GetDocumentTypesAsync(int skip = 0, int limit = 100)
        {
            try
            {
                var documentTypes = await _documentTypeRepository.GetAllAsync();
                return _mapper.Map<IEnumerable<DocumentTypeDto>>(documentTypes)
                    .Skip(skip)
                    .Take(limit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document types with pagination");
                throw;
            }
        }

        public async Task DeactivateDocumentTypeAsync(int id)
        {
            try
            {
                // Since we're using Guid in our repository but int in this method,
                // we need to handle this mismatch. For now, we'll throw an exception.
                throw new NotImplementedException("Method not implemented due to ID type mismatch");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating document type with ID {DocumentTypeId}", id);
                throw;
            }
        }
    }
}