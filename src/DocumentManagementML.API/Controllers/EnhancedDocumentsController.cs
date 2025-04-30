// -----------------------------------------------------------------------------
// <copyright file="EnhancedDocumentsController.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 30, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        Enhanced API controller for documents with standardized
//                     responses and improved error handling
// -----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using DocumentManagementML.Application.DTOs;
using DocumentManagementML.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DocumentManagementML.API.Controllers
{
    /// <summary>
    /// Enhanced API controller for documents with standardized responses and improved error handling
    /// </summary>
    [ApiController]
    [Route("api/documents")]
    public class EnhancedDocumentsController : BaseApiController
    {
        private readonly IDocumentService _documentService;
        private readonly IDocumentClassificationService _classificationService;
        private readonly IVersionedFileStorageService _fileStorageService;

        /// <summary>
        /// Initializes a new instance of the EnhancedDocumentsController class
        /// </summary>
        /// <param name="documentService">Document service</param>
        /// <param name="classificationService">Document classification service</param>
        /// <param name="fileStorageService">File storage service</param>
        /// <param name="logger">Logger</param>
        public EnhancedDocumentsController(
            IDocumentService documentService,
            IDocumentClassificationService classificationService,
            IVersionedFileStorageService fileStorageService,
            ILogger<EnhancedDocumentsController> logger)
            : base(logger)
        {
            _documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
            _classificationService = classificationService ?? throw new ArgumentNullException(nameof(classificationService));
            _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        }

        /// <summary>
        /// Gets all documents with pagination
        /// </summary>
        /// <param name="skip">Number of documents to skip</param>
        /// <param name="limit">Maximum number of documents to return</param>
        /// <returns>Collection of document DTOs</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseDto<IEnumerable<DocumentDto>>), 200)]
        [ProducesResponseType(typeof(ResponseDto), 500)]
        public async Task<IActionResult> GetDocuments(
            [FromQuery] int skip = 0,
            [FromQuery] int limit = 100)
        {
            return await ExecuteAsync(
                () => _documentService.GetDocumentsAsync(skip, limit),
                "Error retrieving documents");
        }

        /// <summary>
        /// Gets a document by its identifier
        /// </summary>
        /// <param name="id">Document identifier</param>
        /// <returns>Document DTO if found</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ResponseDto<DocumentDto>), 200)]
        [ProducesResponseType(typeof(ResponseDto), 404)]
        [ProducesResponseType(typeof(ResponseDto), 500)]
        public async Task<IActionResult> GetDocument(Guid id)
        {
            return await ExecuteAsync(
                () => _documentService.GetDocumentByIdAsync(id),
                $"Error retrieving document with ID {id}");
        }

        /// <summary>
        /// Model for document upload
        /// </summary>
        public class DocumentUploadModel
        {
            /// <summary>
            /// Document metadata
            /// </summary>
            [Required]
            public DocumentCreateDto Document { get; set; }
            
            /// <summary>
            /// Document file
            /// </summary>
            [Required]
            public IFormFile File { get; set; }
        }

        /// <summary>
        /// Creates a new document
        /// </summary>
        /// <param name="model">Document upload model</param>
        /// <returns>Created document DTO</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseDto<DocumentDto>), 201)]
        [ProducesResponseType(typeof(ResponseDto), 400)]
        [ProducesResponseType(typeof(ResponseDto), 500)]
        public async Task<IActionResult> CreateDocument([FromForm] DocumentUploadModel model)
        {
            if (model.File == null || model.File.Length == 0)
            {
                return BadRequest(ResponseDto.Fail("No file uploaded"));
            }

            var result = await ExecuteAsync<DocumentDto>(async () =>
            {
                using var stream = model.File.OpenReadStream();
                model.Document.FileType = Path.GetExtension(model.File.FileName).TrimStart('.');
                
                return await _documentService.CreateDocumentAsync(model.Document, stream, model.File.FileName);
            }, $"Error creating document: {model.Document.Name}");
            
            if (result is OkObjectResult okResult)
            {
                // Change status code to 201 Created
                return CreatedAtAction(
                    nameof(GetDocument),
                    new { id = ((ResponseDto<DocumentDto>)okResult.Value).Data.Id },
                    okResult.Value
                );
            }
            
            return result;
        }

        /// <summary>
        /// Updates an existing document
        /// </summary>
        /// <param name="id">Document identifier</param>
        /// <param name="documentDto">Document update DTO</param>
        /// <returns>Updated document DTO if found</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ResponseDto<DocumentDto>), 200)]
        [ProducesResponseType(typeof(ResponseDto), 400)]
        [ProducesResponseType(typeof(ResponseDto), 404)]
        [ProducesResponseType(typeof(ResponseDto), 500)]
        public async Task<IActionResult> UpdateDocument(Guid id, DocumentUpdateDto documentDto)
        {
            return await ExecuteAsync(
                () => _documentService.UpdateDocumentAsync(id, documentDto),
                $"Error updating document with ID {id}");
        }

        /// <summary>
        /// Deletes a document
        /// </summary>
        /// <param name="id">Document identifier</param>
        /// <returns>Success response if deleted</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ResponseDto), 200)]
        [ProducesResponseType(typeof(ResponseDto), 404)]
        [ProducesResponseType(typeof(ResponseDto), 500)]
        public async Task<IActionResult> DeleteDocument(Guid id)
        {
            return await ExecuteBooleanAsync(
                () => _documentService.DeleteDocumentAsync(id),
                $"Error deleting document with ID {id}",
                "Document deleted successfully");
        }

        /// <summary>
        /// Classifies an existing document
        /// </summary>
        /// <param name="id">Document identifier</param>
        /// <returns>Classification result DTO</returns>
        [HttpPost("{id}/classify")]
        [ProducesResponseType(typeof(ResponseDto<DocumentClassificationResultDto>), 200)]
        [ProducesResponseType(typeof(ResponseDto), 400)]
        [ProducesResponseType(typeof(ResponseDto), 404)]
        [ProducesResponseType(typeof(ResponseDto), 500)]
        public async Task<IActionResult> ClassifyDocument(Guid id)
        {
            return await ExecuteAsync(
                async () => {
                    var result = await _classificationService.ClassifyDocumentAsync(id);
                    if (!result.IsSuccessful)
                    {
                        throw new Application.Exceptions.ValidationException(result.ErrorMessage);
                    }
                    return result;
                },
                $"Error classifying document with ID {id}");
        }

        /// <summary>
        /// Model for file classification
        /// </summary>
        public class ClassifyFileModel
        {
            /// <summary>
            /// File to classify
            /// </summary>
            [Required]
            public IFormFile File { get; set; }
        }

        /// <summary>
        /// Classifies a document file without saving it
        /// </summary>
        /// <param name="model">File classification model</param>
        /// <returns>Classification result DTO</returns>
        [HttpPost("classify")]
        [ProducesResponseType(typeof(ResponseDto<DocumentClassificationResultDto>), 200)]
        [ProducesResponseType(typeof(ResponseDto), 400)]
        [ProducesResponseType(typeof(ResponseDto), 500)]
        public async Task<IActionResult> ClassifyDocument([FromForm] ClassifyFileModel model)
        {
            if (model.File == null || model.File.Length == 0)
            {
                return BadRequest(ResponseDto.Fail("No file uploaded"));
            }

            return await ExecuteAsync(async () =>
            {
                using var stream = model.File.OpenReadStream();
                return await _classificationService.ClassifyDocumentAsync(stream, model.File.FileName);
            }, "Error classifying uploaded document");
        }
        
        /// <summary>
        /// Uploads a new version of an existing document
        /// </summary>
        /// <param name="id">Document identifier</param>
        /// <param name="file">Document file</param>
        /// <param name="userId">User identifier (optional)</param>
        /// <returns>Version information</returns>
        [HttpPost("{id}/versions")]
        [ProducesResponseType(typeof(ResponseDto<VersionInfoDto>), 201)]
        [ProducesResponseType(typeof(ResponseDto), 400)]
        [ProducesResponseType(typeof(ResponseDto), 404)]
        [ProducesResponseType(typeof(ResponseDto), 500)]
        public async Task<IActionResult> UploadDocumentVersion(Guid id, IFormFile file, [FromQuery] Guid? userId = null)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(ResponseDto.Fail("No file uploaded"));
            }

            var result = await ExecuteAsync<VersionInfoDto>(async () =>
            {
                // Verify document exists
                var document = await _documentService.GetDocumentByIdAsync(id);
                if (document == null)
                {
                    throw new DocumentManagementML.Domain.Exceptions.NotFoundException($"Document with ID {id} not found");
                }
                
                // Save the new version
                using var stream = file.OpenReadStream();
                var versionResult = await _fileStorageService.SaveVersionAsync(id, stream, file.FileName, userId ?? Guid.Empty);
                
                // Return version info
                return new VersionInfoDto
                {
                    DocumentId = id,
                    VersionNumber = versionResult.VersionNumber,
                    FilePath = versionResult.FilePath,
                    FileName = file.FileName,
                    FileSizeBytes = file.Length,
                    ContentType = file.ContentType,
                    CreatedDate = DateTime.UtcNow
                };
            }, $"Error uploading new version for document with ID {id}");
            
            if (result is OkObjectResult okResult)
            {
                // Change status code to 201 Created
                return CreatedAtAction(
                    nameof(GetDocument),
                    new { id },
                    okResult.Value
                );
            }
            
            return result;
        }
        
        /// <summary>
        /// Gets all versions of a document
        /// </summary>
        /// <param name="id">Document identifier</param>
        /// <returns>Collection of version information</returns>
        [HttpGet("{id}/versions")]
        [ProducesResponseType(typeof(ResponseDto<IEnumerable<FileVersionInfo>>), 200)]
        [ProducesResponseType(typeof(ResponseDto), 404)]
        [ProducesResponseType(typeof(ResponseDto), 500)]
        public async Task<IActionResult> GetDocumentVersions(Guid id)
        {
            return await ExecuteAsync(
                async () => {
                    // Verify document exists
                    var document = await _documentService.GetDocumentByIdAsync(id);
                    if (document == null)
                    {
                        throw new DocumentManagementML.Domain.Exceptions.NotFoundException($"Document with ID {id} not found");
                    }
                    
                    return await _fileStorageService.GetVersionHistoryAsync(id);
                },
                $"Error retrieving versions for document with ID {id}");
        }
        
        /// <summary>
        /// Gets a specific version of a document
        /// </summary>
        /// <param name="id">Document identifier</param>
        /// <param name="versionNumber">Version number (0 for latest)</param>
        /// <returns>File content</returns>
        [HttpGet("{id}/versions/{versionNumber}")]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        [ProducesResponseType(typeof(ResponseDto), 404)]
        [ProducesResponseType(typeof(ResponseDto), 500)]
        public async Task<IActionResult> GetDocumentVersion(Guid id, int versionNumber = 0)
        {
            try
            {
                // Verify document exists
                var document = await _documentService.GetDocumentByIdAsync(id);
                if (document == null)
                {
                    return NotFound(ResponseDto.Fail($"Document with ID {id} not found"));
                }
                
                var result = await _fileStorageService.GetVersionAsync(id, versionNumber);
                
                // Return file content
                var memoryStream = new MemoryStream();
                await result.Content.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                
                return File(memoryStream, GetContentType(document.FileType), GetFileName(document.Name, document.FileType));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error retrieving version {versionNumber} for document with ID {id}");
                return StatusCode(500, ResponseDto.Fail("An error occurred while retrieving the document version"));
            }
        }
        
        /// <summary>
        /// Gets the content type for a file type
        /// </summary>
        /// <param name="fileType">File type</param>
        /// <returns>Content type</returns>
        private string GetContentType(string fileType)
        {
            if (string.IsNullOrWhiteSpace(fileType))
            {
                return "application/octet-stream";
            }
            
            return fileType.ToLowerInvariant() switch
            {
                "pdf" => "application/pdf",
                "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "doc" => "application/msword",
                "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "xls" => "application/vnd.ms-excel",
                "pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                "ppt" => "application/vnd.ms-powerpoint",
                "txt" => "text/plain",
                "html" => "text/html",
                "htm" => "text/html",
                "csv" => "text/csv",
                "json" => "application/json",
                "xml" => "application/xml",
                "jpg" => "image/jpeg",
                "jpeg" => "image/jpeg",
                "png" => "image/png",
                "gif" => "image/gif",
                "bmp" => "image/bmp",
                _ => "application/octet-stream"
            };
        }
        
        /// <summary>
        /// Gets a file name for download
        /// </summary>
        /// <param name="documentName">Document name</param>
        /// <param name="fileType">File type</param>
        /// <returns>File name</returns>
        private string GetFileName(string documentName, string fileType)
        {
            if (string.IsNullOrWhiteSpace(fileType))
            {
                return documentName;
            }
            
            var normalizedFileType = fileType.TrimStart('.');
            return $"{documentName}.{normalizedFileType}";
        }
    }
    
    /// <summary>
    /// Version information DTO
    /// </summary>
    public class VersionInfoDto
    {
        /// <summary>
        /// Document identifier
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
        /// Creation date
        /// </summary>
        public DateTime CreatedDate { get; set; }
    }
}