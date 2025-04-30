// DocumentsController.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using DocumentManagementML.Application.DTOs;
using DocumentManagementML.Application.Exceptions;
using DocumentManagementML.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DocumentManagementML.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly IDocumentClassificationService _classificationService;
        private readonly ILogger<DocumentsController> _logger;

        public DocumentsController(
            IDocumentService documentService,
            IDocumentClassificationService classificationService,
            ILogger<DocumentsController> logger)
        {
            _documentService = documentService;
            _classificationService = classificationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DocumentDto>>> GetDocuments(
            [FromQuery] int skip = 0,
            [FromQuery] int limit = 100)
        {
            try
            {
                var documents = await _documentService.GetDocumentsAsync(skip, limit);
                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving documents: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving documents");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentDto>> GetDocument(Guid id)
        {
            try
            {
                var document = await _documentService.GetDocumentByIdAsync(id);
                if (document == null)
                {
                    return NotFound($"Document with ID {id} not found");
                }
                return Ok(document);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving document {id}: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving the document");
            }
        }

        public class DocumentUploadModel
        {
            [Required]
            public DocumentCreateDto Document { get; set; }
            
            [Required]
            public IFormFile File { get; set; }
        }

        [HttpPost]
        public async Task<ActionResult<DocumentDto>> CreateDocument([FromForm] DocumentUploadModel model)
        {
            try
            {
                if (model.File == null || model.File.Length == 0)
                {
                    return BadRequest("No file uploaded");
                }

                using var stream = model.File.OpenReadStream();
                model.Document.FileType = Path.GetExtension(model.File.FileName).TrimStart('.');
                
                var document = await _documentService.CreateDocumentAsync(model.Document, stream, model.File.FileName);
                
                return CreatedAtAction(
                    nameof(GetDocument),
                    new { id = document.Id },
                    document
                );
            }
            catch (Application.Exceptions.ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating document: {ex.Message}");
                return StatusCode(500, "An error occurred while creating the document");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<DocumentDto>> UpdateDocument(Guid id, DocumentUpdateDto documentDto)
        {
            try
            {
                var document = await _documentService.UpdateDocumentAsync(id, documentDto);
                if (document == null)
                {
                    return NotFound($"Document with ID {id} not found");
                }
                return Ok(document);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating document {id}: {ex.Message}");
                return StatusCode(500, "An error occurred while updating the document");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteDocument(Guid id)
        {
            try
            {
                var result = await _documentService.DeleteDocumentAsync(id);
                if (!result)
                {
                    return NotFound($"Document with ID {id} not found");
                }
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting document {id}: {ex.Message}");
                return StatusCode(500, "An error occurred while deleting the document");
            }
        }

        [HttpPost("{id}/classify")]
        public async Task<ActionResult<DocumentClassificationResultDto>> ClassifyDocument(Guid id)
        {
            try
            {
                var result = await _classificationService.ClassifyDocumentAsync(id);
                if (!result.IsSuccessful)
                {
                    return BadRequest(result.ErrorMessage);
                }
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error classifying document {id}: {ex.Message}");
                return StatusCode(500, "An error occurred while classifying the document");
            }
        }

        public class ClassifyFileModel
        {
            [Required]
            public IFormFile File { get; set; }
        }

        [HttpPost("classify")]
        public async Task<ActionResult<DocumentClassificationResultDto>> ClassifyDocument([FromForm] ClassifyFileModel model)
        {
            try
            {
                if (model.File == null || model.File.Length == 0)
                {
                    return BadRequest("No file uploaded");
                }

                using var stream = model.File.OpenReadStream();
                var result = await _classificationService.ClassifyDocumentAsync(stream, model.File.FileName);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error classifying uploaded document: {ex.Message}");
                return StatusCode(500, "An error occurred while classifying the document");
            }
        }
    }
}

