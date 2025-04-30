// -----------------------------------------------------------------------------
// <copyright file="EnhancedDocumentTypesController.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 29, 2025
// Last Modified:      April 29, 2025
// Version:            0.9.0
// Description:        Enhanced API controller for document types with standardized
//                     responses and improved error handling
// -----------------------------------------------------------------------------
using DocumentManagementML.Application.DTOs;
using DocumentManagementML.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocumentManagementML.API.Controllers
{
    /// <summary>
    /// Enhanced API controller for document types with standardized responses and improved error handling
    /// </summary>
    [ApiController]
    [Route("api/documenttypes")]
    public class EnhancedDocumentTypesController : BaseApiController
    {
        private readonly IDocumentTypeService _documentTypeService;

        /// <summary>
        /// Initializes a new instance of the EnhancedDocumentTypesController class
        /// </summary>
        /// <param name="documentTypeService">Document type service</param>
        /// <param name="logger">Logger</param>
        public EnhancedDocumentTypesController(
            IDocumentTypeService documentTypeService,
            ILogger<EnhancedDocumentTypesController> logger)
            : base(logger)
        {
            _documentTypeService = documentTypeService ?? throw new ArgumentNullException(nameof(documentTypeService));
        }

        /// <summary>
        /// Gets all document types
        /// </summary>
        /// <returns>Collection of document type DTOs</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseDto<IEnumerable<DocumentTypeDto>>), 200)]
        [ProducesResponseType(typeof(ResponseDto), 500)]
        public async Task<IActionResult> GetAllDocumentTypes()
        {
            return await ExecuteAsync(
                () => _documentTypeService.GetAllDocumentTypesAsync(),
                "Error retrieving all document types");
        }

        /// <summary>
        /// Gets all active document types
        /// </summary>
        /// <returns>Collection of active document type DTOs</returns>
        [HttpGet("active")]
        [ProducesResponseType(typeof(ResponseDto<IEnumerable<DocumentTypeDto>>), 200)]
        [ProducesResponseType(typeof(ResponseDto), 500)]
        public async Task<IActionResult> GetActiveDocumentTypes()
        {
            return await ExecuteAsync(
                () => _documentTypeService.GetActiveDocumentTypesAsync(),
                "Error retrieving active document types");
        }

        /// <summary>
        /// Gets a document type by its identifier
        /// </summary>
        /// <param name="id">Document type identifier</param>
        /// <returns>Document type DTO if found</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ResponseDto<DocumentTypeDto>), 200)]
        [ProducesResponseType(typeof(ResponseDto), 404)]
        [ProducesResponseType(typeof(ResponseDto), 500)]
        public async Task<IActionResult> GetDocumentType(Guid id)
        {
            return await ExecuteAsync(
                () => _documentTypeService.GetDocumentTypeByIdAsync(id),
                $"Error retrieving document type with ID {id}");
        }

        /// <summary>
        /// Creates a new document type
        /// </summary>
        /// <param name="documentTypeDto">Document type creation DTO</param>
        /// <returns>Created document type DTO</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseDto<DocumentTypeDto>), 201)]
        [ProducesResponseType(typeof(ResponseDto), 400)]
        [ProducesResponseType(typeof(ResponseDto), 500)]
        public async Task<IActionResult> CreateDocumentType(DocumentTypeCreateDto documentTypeDto)
        {
            var result = await ExecuteAsync<DocumentTypeDto>(
                () => _documentTypeService.CreateDocumentTypeAsync(documentTypeDto),
                $"Error creating document type {documentTypeDto.Name}");
            
            if (result is OkObjectResult okResult)
            {
                // Change status code to 201 Created
                return CreatedAtAction(
                    nameof(GetDocumentType),
                    new { id = ((ResponseDto<DocumentTypeDto>)okResult.Value).Data.Id },
                    okResult.Value
                );
            }
            
            return result;
        }

        /// <summary>
        /// Updates an existing document type
        /// </summary>
        /// <param name="id">Document type identifier</param>
        /// <param name="documentTypeDto">Document type update DTO</param>
        /// <returns>Updated document type DTO if found</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ResponseDto<DocumentTypeDto>), 200)]
        [ProducesResponseType(typeof(ResponseDto), 400)]
        [ProducesResponseType(typeof(ResponseDto), 404)]
        [ProducesResponseType(typeof(ResponseDto), 500)]
        public async Task<IActionResult> UpdateDocumentType(Guid id, DocumentTypeUpdateDto documentTypeDto)
        {
            return await ExecuteAsync(
                () => _documentTypeService.UpdateDocumentTypeAsync(id, documentTypeDto),
                $"Error updating document type with ID {id}");
        }

        /// <summary>
        /// Deletes a document type
        /// </summary>
        /// <param name="id">Document type identifier</param>
        /// <returns>Success response if deleted</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ResponseDto), 200)]
        [ProducesResponseType(typeof(ResponseDto), 400)]
        [ProducesResponseType(typeof(ResponseDto), 404)]
        [ProducesResponseType(typeof(ResponseDto), 500)]
        public async Task<IActionResult> DeleteDocumentType(Guid id)
        {
            return await ExecuteBooleanAsync(
                () => _documentTypeService.DeleteDocumentTypeAsync(id),
                $"Error deleting document type with ID {id}",
                "Document type deleted successfully");
        }
    }
}