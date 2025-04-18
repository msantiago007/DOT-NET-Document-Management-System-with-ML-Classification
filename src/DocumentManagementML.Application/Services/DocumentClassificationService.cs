// DocumentClassificationService.cs

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using DocumentManagementML.Application.DTOs;
using DocumentManagementML.Application.Interfaces;
using DocumentManagementML.Domain.Entities;
using DocumentManagementML.Domain.Repositories;
using DocumentManagementML.Domain.Services;
using Microsoft.Extensions.Logging;

namespace DocumentManagementML.Application.Services
{
    public class DocumentClassificationService : IDocumentClassificationService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IDocumentTypeRepository _documentTypeRepository;
        private readonly IMLModelService _mlModelService;
        private readonly IMapper _mapper;
        private readonly ILogger<DocumentClassificationService> _logger;

        public DocumentClassificationService(
            IDocumentRepository documentRepository,
            IDocumentTypeRepository documentTypeRepository,
            IMLModelService mlModelService,
            IMapper mapper,
            ILogger<DocumentClassificationService> logger)
        {
            _documentRepository = documentRepository;
            _documentTypeRepository = documentTypeRepository;
            _mlModelService = mlModelService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<DocumentClassificationResultDto> ClassifyDocumentAsync(ClassificationRequestDto request)
        {
            try
            {
                var document = await _documentRepository.GetByIdAsync(request.DocumentId);
                if (document == null)
                {
                    return new DocumentClassificationResultDto
                    {
                        DocumentId = request.DocumentId,
                        IsSuccessful = false,
                        ErrorMessage = $"Document with ID {request.DocumentId} not found"
                    };
                }

                var classificationResult = await _mlModelService.ClassifyDocumentAsync(
                    request.FilePath,
                    request.FileType);

                if (classificationResult.IsSuccessful && classificationResult.PredictedDocumentTypeId.HasValue)
                {
                    var documentType = await _documentTypeRepository.GetByIdAsync(classificationResult.PredictedDocumentTypeId.Value);
                    classificationResult.PredictedDocumentType = documentType;
                }

                classificationResult.DocumentId = request.DocumentId;
                classificationResult.DocumentName = document.DocumentName;

                var resultDto = _mapper.Map<DocumentClassificationResultDto>(classificationResult);
                return resultDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error classifying document with ID {DocumentId}", request.DocumentId);
                
                return new DocumentClassificationResultDto
                {
                    DocumentId = request.DocumentId,
                    IsSuccessful = false,
                    ErrorMessage = $"Classification error: {ex.Message}"
                };
            }
        }

        public async Task<bool> ApplyClassificationResultAsync(DocumentClassificationResultDto result)
        {
            try
            {
                if (!result.IsSuccessful || !result.PredictedDocumentTypeId.HasValue)
                {
                    return false;
                }

                var document = await _documentRepository.GetByIdAsync(result.DocumentId);
                if (document == null)
                {
                    return false;
                }

                document.DocumentTypeId = result.PredictedDocumentTypeId.HasValue 
                    ? Convert.ToInt32(result.PredictedDocumentTypeId.Value.ToString()) 
                    : null;
                document.ClassificationConfidence = result.Confidence;
                document.LastModifiedDate = DateTime.UtcNow;

                await _documentRepository.UpdateAsync(document);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying classification result to document with ID {DocumentId}", result.DocumentId);
                return false;
            }
        }

        public async Task<ModelMetricsDto> GetModelMetricsAsync()
        {
            // This is a placeholder implementation
            return new ModelMetricsDto
            {
                Id = Guid.NewGuid(),
                ModelName = "Document Classification Model",
                ModelVersion = "1.0.0",
                Accuracy = 0.85,
                Precision = 0.82,
                Recall = 0.79,
                F1Score = 0.80,
                TotalSamples = 1000,
                TrainingDate = DateTime.UtcNow.AddDays(-7),
                ClassMetrics = new Dictionary<string, double>
                {
                    { "Invoice", 0.92 },
                    { "Contract", 0.88 },
                    { "Report", 0.85 },
                    { "Letter", 0.79 }
                }
            };
        }
    }
}