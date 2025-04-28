// DocumentClassificationService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DocumentManagementML.Application.DTOs;
using DocumentManagementML.Application.Exceptions;
using DocumentManagementML.Application.Interfaces;
using DocumentManagementML.Domain.Entities;
using DocumentManagementML.Domain.Repositories;
using DocumentManagementML.Domain.Services;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace DocumentManagementML.Application.Services
{
    /// <summary>
    /// Service implementation for document classification operations
    /// </summary>
    public class DocumentClassificationService : IDocumentClassificationService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IDocumentTypeRepository _documentTypeRepository;
        private readonly IDocumentMetadataRepository _documentMetadataRepository;
        private readonly IMLModelService _mlModelService;
        private readonly Application.Interfaces.IFileStorageService _fileStorageService;
        private readonly ITextExtractor _textExtractor;
        private readonly IMapper _mapper;
        private readonly ILogger<DocumentClassificationService> _logger;

        /// <summary>
        /// Initializes a new instance of the DocumentClassificationService class
        /// </summary>
        public DocumentClassificationService(
            IDocumentRepository documentRepository,
            IDocumentTypeRepository documentTypeRepository,
            IDocumentMetadataRepository documentMetadataRepository,
            IMLModelService mlModelService,
            Application.Interfaces.IFileStorageService fileStorageService,
            ITextExtractor textExtractor,
            IMapper mapper,
            ILogger<DocumentClassificationService> logger)
        {
            _documentRepository = documentRepository;
            _documentTypeRepository = documentTypeRepository;
            _documentMetadataRepository = documentMetadataRepository;
            _mlModelService = mlModelService;
            _fileStorageService = fileStorageService;
            _textExtractor = textExtractor;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Classifies a document based on its ID
        /// </summary>
        public async Task<DocumentClassificationResultDto> ClassifyDocumentAsync(Guid documentId)
        {
            try
            {
                var document = await _documentRepository.GetByIdAsync(documentId);
                if (document == null)
                {
                    return new DocumentClassificationResultDto
                    {
                        IsSuccessful = false,
                        ErrorMessage = $"Document with ID {documentId} not found"
                    };
                }

                // Get file stream
                var fileStream = await _fileStorageService.RetrieveFileAsync(document.FilePath);
                
                // Get file extension
                var fileExtension = Path.GetExtension(document.FilePath);
                
                // Classify using the document stream
                var result = await ClassifyDocumentAsync(fileStream, fileExtension);
                
                // Add document information to result
                result.DocumentId = documentId;
                result.DocumentName = document.DocumentName;
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error classifying document with ID {DocumentId}", documentId);
                
                return new DocumentClassificationResultDto
                {
                    IsSuccessful = false,
                    DocumentId = documentId,
                    ErrorMessage = $"Classification error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Classifies a document using provided file stream
        /// </summary>
        public async Task<DocumentClassificationResultDto> ClassifyDocumentAsync(Stream fileStream, string fileName)
        {
            try
            {
                // Extract text from document
                var fileExtension = Path.GetExtension(fileName);
                var extractedText = await _textExtractor.ExtractTextAsync(fileStream, fileExtension);
                
                if (string.IsNullOrWhiteSpace(extractedText))
                {
                    return new DocumentClassificationResultDto
                    {
                        IsSuccessful = false,
                        ErrorMessage = "No text could be extracted from the document"
                    };
                }

                // Classify text
                // Since the interface only defines ClassifyDocumentAsync, we'll need to adapt
                // Create a temporary file with the extracted text to classify
                string tempFilePath = Path.GetTempFileName();
                await File.WriteAllTextAsync(tempFilePath, extractedText);
                var classificationResult = await _mlModelService.ClassifyDocumentAsync(tempFilePath, "txt");
                // Clean up temp file
                File.Delete(tempFilePath);
                
                if (!classificationResult.IsSuccessful)
                {
                    return new DocumentClassificationResultDto
                    {
                        IsSuccessful = false,
                        ErrorMessage = classificationResult.ErrorMessage
                    };
                }

                // Get document type information if available
                DocumentTypeDto? documentTypeDto = null;
                if (classificationResult.PredictedDocumentTypeId.HasValue)
                {
                    var documentType = await _documentTypeRepository.GetByIdAsync(
                        classificationResult.PredictedDocumentTypeId.Value);
                    
                    if (documentType != null)
                    {
                        documentTypeDto = _mapper.Map<DocumentTypeDto>(documentType);
                    }
                }

                // Create result
                var result = new DocumentClassificationResultDto
                {
                    IsSuccessful = true,
                    PredictedDocumentTypeId = classificationResult.PredictedDocumentTypeId,
                    PredictedDocumentTypeName = documentTypeDto?.Name,
                    Confidence = classificationResult.Confidence,
                    AllPredictions = classificationResult.AllTypePredictions?
                        .Select(p => new DocumentTypeScoreDto
                        {
                            DocumentTypeId = p.DocumentTypeId,
                            DocumentTypeName = p.DocumentTypeName,
                            Score = p.Score
                        })
                        .ToList()
                };
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error classifying uploaded document");
                
                return new DocumentClassificationResultDto
                {
                    IsSuccessful = false,
                    ErrorMessage = $"Classification error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Classifies a document using a classification request
        /// </summary>
        public async Task<DocumentClassificationResultDto> ClassifyDocumentAsync(ClassificationRequestDto request)
        {
            IDbContextTransaction? transaction = null;
            
            try
            {
                // Start transaction
                transaction = await _documentRepository.BeginTransactionAsync();
                
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

                // Open the file and classify using the document stream
                using var fileStream = await _fileStorageService.RetrieveFileAsync(request.FilePath);
                var result = await ClassifyDocumentAsync(fileStream, request.FileType);
                
                if (result.IsSuccessful && result.PredictedDocumentTypeId.HasValue)
                {
                    // Update document with classification result
                    document.DocumentTypeId = result.PredictedDocumentTypeId;
                    document.ClassificationConfidence = result.Confidence;
                    document.LastModifiedDate = DateTime.UtcNow;
                    
                    await _documentRepository.UpdateAsync(document);
                    
                    // Store classification metadata
                    var classificationMetadata = new DocumentMetadata
                    {
                        DocumentId = request.DocumentId,
                        MetadataKey = "ClassificationResult",
                        MetadataValue = System.Text.Json.JsonSerializer.Serialize(result),
                        DataType = "json",
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedDate = DateTime.UtcNow
                    };
                    
                    await _documentMetadataRepository.UpsertAsync(classificationMetadata);
                }

                // Commit transaction
                await _documentRepository.CommitTransactionAsync(transaction);

                result.DocumentId = request.DocumentId;
                result.DocumentName = document.Name;

                return result;
            }
            catch (Exception ex)
            {
                // Rollback transaction if an error occurred
                if (transaction != null)
                {
                    await _documentRepository.RollbackTransactionAsync(transaction);
                }
                
                _logger.LogError(ex, "Error classifying document with ID {DocumentId}", request.DocumentId);
                
                return new DocumentClassificationResultDto
                {
                    DocumentId = request.DocumentId,
                    IsSuccessful = false,
                    ErrorMessage = $"Classification error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Applies classification result to document metadata
        /// </summary>
        public async Task<bool> ApplyClassificationResultAsync(DocumentClassificationResultDto result)
        {
            IDbContextTransaction? transaction = null;
            
            try
            {
                // Start transaction
                transaction = await _documentRepository.BeginTransactionAsync();
                
                if (!result.IsSuccessful || !result.PredictedDocumentTypeId.HasValue)
                {
                    return false;
                }

                var document = await _documentRepository.GetByIdAsync(result.DocumentId);
                if (document == null)
                {
                    return false;
                }

                document.DocumentTypeId = result.PredictedDocumentTypeId;
                document.ClassificationConfidence = result.Confidence;
                document.LastModifiedDate = DateTime.UtcNow;

                await _documentRepository.UpdateAsync(document);
                
                // Store classification metadata
                var classificationMetadata = new DocumentMetadata
                {
                    DocumentId = result.DocumentId,
                    MetadataKey = "ClassificationResult",
                    MetadataValue = System.Text.Json.JsonSerializer.Serialize(result),
                    DataType = "json",
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow
                };
                
                await _documentMetadataRepository.UpsertAsync(classificationMetadata);
                
                // Commit transaction
                await _documentRepository.CommitTransactionAsync(transaction);
                
                return true;
            }
            catch (Exception ex)
            {
                // Rollback transaction if an error occurred
                if (transaction != null)
                {
                    await _documentRepository.RollbackTransactionAsync(transaction);
                }
                
                _logger.LogError(ex, "Error applying classification result to document with ID {DocumentId}", result.DocumentId);
                return false;
            }
        }

        /// <summary>
        /// Gets the current model metrics
        /// </summary>
        public async Task<ModelMetricsDto> GetModelMetricsAsync()
        {
            try
            {
                var metrics = await _mlModelService.GetModelMetricsAsync();
                var metricsDto = _mapper.Map<ModelMetricsDto>(metrics);
                metricsDto.Success = metrics.IsSuccessful;
                return metricsDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting model metrics");
                
                return new ModelMetricsDto
                {
                    Success = false,
                    ErrorMessage = $"Error getting model metrics: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Trains the classification model with existing documents
        /// </summary>
        public async Task<bool> TrainModelAsync()
        {
            try
            {
                _logger.LogInformation("Starting model training");
                
                // Get all documents with known document types
                var documents = await _documentRepository.GetAllAsync();
                var trainingDocuments = new List<TrainingDocument>();
                
                foreach (var document in documents.Where(d => d.DocumentTypeId.HasValue && !d.IsDeleted))
                {
                    try
                    {
                        // Get file stream
                        var fileStream = await _fileStorageService.RetrieveFileAsync(document.FilePath);
                        
                        // Extract text
                        var fileExtension = Path.GetExtension(document.FilePath);
                        var extractedText = await _textExtractor.ExtractTextAsync(fileStream, fileExtension);
                        
                        if (!string.IsNullOrWhiteSpace(extractedText))
                        {
                            var documentType = await _documentTypeRepository.GetByIdAsync(document.DocumentTypeId.Value);
                            if (documentType != null)
                            {
                                trainingDocuments.Add(new TrainingDocument
                                {
                                    DocumentId = document.DocumentId,
                                    Text = extractedText,
                                    DocumentType = documentType.Name
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error processing document {DocumentId} for training", document.DocumentId);
                    }
                }
                
                if (trainingDocuments.Count == 0)
                {
                    _logger.LogWarning("No training documents found");
                    return false;
                }
                
                _logger.LogInformation("Training model with {Count} documents", trainingDocuments.Count);
                return await _mlModelService.TrainModelAsync(trainingDocuments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error training model");
                return false;
            }
        }

        /// <summary>
        /// Evaluates the model using test data
        /// </summary>
        public async Task<ModelMetricsDto> EvaluateModelAsync()
        {
            try
            {
                // Similar to TrainModelAsync, but we'll split data into test set
                var documents = await _documentRepository.GetAllAsync();
                var testDocuments = new List<TrainingDocument>();
                
                foreach (var document in documents.Where(d => d.DocumentTypeId.HasValue && !d.IsDeleted))
                {
                    try
                    {
                        // Get file stream
                        var fileStream = await _fileStorageService.RetrieveFileAsync(document.FilePath);
                        
                        // Extract text
                        var fileExtension = Path.GetExtension(document.FilePath);
                        var extractedText = await _textExtractor.ExtractTextAsync(fileStream, fileExtension);
                        
                        if (!string.IsNullOrWhiteSpace(extractedText))
                        {
                            var documentType = await _documentTypeRepository.GetByIdAsync(document.DocumentTypeId.Value);
                            if (documentType != null)
                            {
                                // Take only 20% for testing
                                if (document.DocumentId.GetHashCode() % 5 == 0)
                                {
                                    testDocuments.Add(new TrainingDocument
                                    {
                                        DocumentId = document.DocumentId,
                                        Text = extractedText,
                                        DocumentType = documentType.Name
                                    });
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error processing document {DocumentId} for evaluation", document.DocumentId);
                    }
                }
                
                if (testDocuments.Count == 0)
                {
                    return new ModelMetricsDto
                    {
                        Success = false,
                        ErrorMessage = "No test documents found"
                    };
                }
                
                _logger.LogInformation("Evaluating model with {Count} documents", testDocuments.Count);
                var metrics = await _mlModelService.EvaluateModelAsync(testDocuments);
                
                var metricsDto = _mapper.Map<ModelMetricsDto>(metrics);
                metricsDto.Success = true;
                return metricsDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating model");
                
                return new ModelMetricsDto
                {
                    Success = false,
                    ErrorMessage = $"Error evaluating model: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Batch classifies multiple documents
        /// </summary>
        public async Task<IEnumerable<DocumentClassificationResultDto>> BatchClassifyDocumentsAsync(IEnumerable<Guid> documentIds)
        {
            var results = new List<DocumentClassificationResultDto>();
            
            foreach (var documentId in documentIds)
            {
                try
                {
                    var result = await ClassifyDocumentAsync(documentId);
                    results.Add(result);
                    
                    if (result.IsSuccessful && result.PredictedDocumentTypeId.HasValue)
                    {
                        await ApplyClassificationResultAsync(result);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error batch classifying document {DocumentId}", documentId);
                    
                    results.Add(new DocumentClassificationResultDto
                    {
                        DocumentId = documentId,
                        IsSuccessful = false,
                        ErrorMessage = $"Classification error: {ex.Message}"
                    });
                }
            }
            
            return results;
        }

        /// <summary>
        /// Gets classification history for a document
        /// </summary>
        public async Task<IEnumerable<DocumentClassificationResultDto>> GetClassificationHistoryAsync(Guid documentId)
        {
            try
            {
                var metadataItems = await _documentMetadataRepository.GetByDocumentIdAsync(documentId);
                var classificationItems = metadataItems
                    .Where(m => m.MetadataKey == "ClassificationResult")
                    .OrderByDescending(m => m.LastModifiedDate)
                    .ToList();
                
                var results = new List<DocumentClassificationResultDto>();
                
                foreach (var item in classificationItems)
                {
                    try
                    {
                        var result = System.Text.Json.JsonSerializer.Deserialize<DocumentClassificationResultDto>(
                            item.MetadataValue);
                        
                        if (result != null)
                        {
                            results.Add(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error deserializing classification result metadata for document {DocumentId}", documentId);
                    }
                }
                
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting classification history for document {DocumentId}", documentId);
                return Enumerable.Empty<DocumentClassificationResultDto>();
            }
        }

        /// <summary>
        /// Resets the classification for a document
        /// </summary>
        public async Task<bool> ResetDocumentClassificationAsync(Guid documentId)
        {
            IDbContextTransaction? transaction = null;
            
            try
            {
                // Start transaction
                transaction = await _documentRepository.BeginTransactionAsync();
                
                var document = await _documentRepository.GetByIdAsync(documentId);
                if (document == null)
                {
                    return false;
                }

                // Reset document classification
                document.DocumentTypeId = null;
                document.ClassificationConfidence = null;
                document.LastModifiedDate = DateTime.UtcNow;
                
                await _documentRepository.UpdateAsync(document);
                
                // Delete classification metadata
                var metadataItems = await _documentMetadataRepository.GetByDocumentIdAsync(documentId);
                var classificationItems = metadataItems
                    .Where(m => m.MetadataKey == "ClassificationResult")
                    .ToList();
                
                foreach (var item in classificationItems)
                {
                    await _documentMetadataRepository.DeleteAsync(item.Id);
                }
                
                // Commit transaction
                await _documentRepository.CommitTransactionAsync(transaction);
                
                return true;
            }
            catch (Exception ex)
            {
                // Rollback transaction if an error occurred
                if (transaction != null)
                {
                    await _documentRepository.RollbackTransactionAsync(transaction);
                }
                
                _logger.LogError(ex, "Error resetting classification for document {DocumentId}", documentId);
                return false;
            }
        }
    }
}