// DocumentClassificationService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentManagementML.Application.DTOs;
using DocumentManagementML.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace DocumentManagementML.Application.Services
{
    /// <summary>
    /// Service implementation for document classification operations.
    /// Phase 1 implementation - simply redirects to SimpleDocumentClassificationService.
    /// Will be replaced by a full implementation in a later phase.
    /// </summary>
    public class DocumentClassificationService : IDocumentClassificationService
    {
        private readonly SimpleDocumentClassificationService _simpleService;
        private readonly ILogger<DocumentClassificationService> _logger;

        /// <summary>
        /// Initializes a new instance of the DocumentClassificationService class.
        /// </summary>
        public DocumentClassificationService(ILogger<DocumentClassificationService> logger)
        {
            _simpleService = new SimpleDocumentClassificationService();
            _logger = logger;
        }

        /// <summary>
        /// Classifies a document based on its ID.
        /// </summary>
        public async Task<DocumentClassificationResultDto> ClassifyDocumentAsync(Guid documentId)
        {
            _logger.LogInformation("Classification requested for document {DocumentId}", documentId);
            return await _simpleService.ClassifyDocumentAsync(documentId);
        }

        /// <summary>
        /// Batch classifies multiple documents.
        /// </summary>
        public async Task<List<DocumentClassificationResultDto>> ClassifyDocumentsAsync(IEnumerable<Guid> documentIds)
        {
            _logger.LogInformation("Batch classification requested");
            return await _simpleService.ClassifyDocumentsAsync(documentIds);
        }

        /// <summary>
        /// Gets the current model metrics.
        /// </summary>
        public async Task<ModelMetricsDto> GetModelMetricsAsync()
        {
            _logger.LogInformation("Model metrics requested");
            return await _simpleService.GetModelMetricsAsync();
        }
        
        /// <summary>
        /// Classifies a document from a file stream.
        /// </summary>
        public async Task<DocumentClassificationResultDto> ClassifyDocumentAsync(System.IO.Stream fileStream, string fileName)
        {
            _logger.LogInformation("Classification requested for file {FileName}", fileName);
            return await _simpleService.ClassifyDocumentAsync(fileStream, fileName);
        }
        
        /// <summary>
        /// Trains the document classification model.
        /// </summary>
        public async Task TrainModelAsync()
        {
            _logger.LogInformation("Model training requested");
            await _simpleService.TrainModelAsync();
        }
        
        /// <summary>
        /// Evaluates the document classification model.
        /// </summary>
        public async Task<ModelMetricsDto> EvaluateModelAsync()
        {
            _logger.LogInformation("Model evaluation requested");
            return await _simpleService.EvaluateModelAsync();
        }
    }
}