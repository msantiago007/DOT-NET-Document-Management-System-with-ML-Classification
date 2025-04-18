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

namespace DocumentManagementML.Application.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IDocumentTypeRepository _documentTypeRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ILogger<DocumentService> _logger;

        public DocumentService(
            IDocumentRepository documentRepository,
            IDocumentTypeRepository documentTypeRepository,
            IUserService userService,
            IMapper mapper,
            ILogger<DocumentService> logger)
        {
            _documentRepository = documentRepository;
            _documentTypeRepository = documentTypeRepository;
            _userService = userService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<DocumentDto>> GetAllDocumentsAsync()
        {
            try
            {
                var documents = await _documentRepository.GetAllAsync();
                return _mapper.Map<IEnumerable<DocumentDto>>(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all documents");
                throw;
            }
        }

        public async Task<DocumentDto?> GetDocumentByIdAsync(Guid id)
        {
            try
            {
                var document = await _documentRepository.GetByIdAsync(id);
                return document != null ? _mapper.Map<DocumentDto>(document) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document with ID {DocumentId}", id);
                throw;
            }
        }

        public async Task<DocumentDto> CreateDocumentAsync(DocumentCreateDto documentDto, string filePath, string fileType, long fileSize, Guid? userId)
        {
            try
            {
                var document = _mapper.Map<Document>(documentDto);
                
                document.FilePath = filePath;
                document.FileType = fileType;
                document.FileSize = fileSize;
                document.UploadDate = DateTime.UtcNow;
                document.UploadedById = userId;
                
                if (documentDto.DocumentTypeId.HasValue)
                {
                    var documentType = await _documentTypeRepository.GetByIdAsync(documentDto.DocumentTypeId.Value);
                    if (documentType == null)
                    {
                        throw new ArgumentException($"Document type with ID {documentDto.DocumentTypeId.Value} not found");
                    }
                }

                document.DocumentName = documentDto.Name;
                document.MetadataDictionary = documentDto.Metadata;

                var createdDocument = await _documentRepository.AddAsync(document);
                return _mapper.Map<DocumentDto>(createdDocument);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating document {DocumentName}", documentDto.Name);
                throw;
            }
        }

        public async Task<DocumentDto?> UpdateDocumentAsync(Guid id, DocumentUpdateDto documentDto)
        {
            try
            {
                var existingDocument = await _documentRepository.GetByIdAsync(id);
                if (existingDocument == null)
                {
                    return null;
                }

                // Update properties
                existingDocument.DocumentName = documentDto.Name;
                existingDocument.Description = documentDto.Description;
                existingDocument.DocumentTypeId = documentDto.DocumentTypeId;
                existingDocument.MetadataDictionary = documentDto.Metadata;
                existingDocument.LastModifiedDate = DateTime.UtcNow;

                if (documentDto.DocumentTypeId.HasValue)
                {
                    var documentType = await _documentTypeRepository.GetByIdAsync(documentDto.DocumentTypeId.Value);
                    if (documentType == null)
                    {
                        throw new ArgumentException($"Document type with ID {documentDto.DocumentTypeId.Value} not found");
                    }
                }

                var updatedDocument = await _documentRepository.UpdateAsync(existingDocument);
                return _mapper.Map<DocumentDto>(updatedDocument);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating document with ID {DocumentId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteDocumentAsync(Guid id)
        {
            try
            {
                var document = await _documentRepository.GetByIdAsync(id);
                if (document == null)
                {
                    return false;
                }

                return await _documentRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document with ID {DocumentId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<DocumentDto>> SearchDocumentsAsync(string searchTerm, Guid? documentTypeId = null)
        {
            try
            {
                var documents = await _documentRepository.SearchAsync(searchTerm, documentTypeId);
                return _mapper.Map<IEnumerable<DocumentDto>>(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching documents with term {SearchTerm}", searchTerm);
                throw;
            }
        }

        public async Task<IEnumerable<DocumentDto>> GetDocumentsAsync(int skip = 0, int limit = 100)
        {
            var documents = await _documentRepository.GetActiveDocumentsAsync(skip, limit);
            return _mapper.Map<IEnumerable<DocumentDto>>(documents);
        }

        // Updated to use Guid instead of int
        public async Task<IEnumerable<DocumentDto>> GetDocumentsByTypeAsync(Guid typeId, int skip = 0, int limit = 100)
        {
            var documents = await _documentRepository.GetByDocumentTypeAsync(typeId);
            return _mapper.Map<IEnumerable<DocumentDto>>(documents)
                .Skip(skip)
                .Take(limit);
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
    }
}