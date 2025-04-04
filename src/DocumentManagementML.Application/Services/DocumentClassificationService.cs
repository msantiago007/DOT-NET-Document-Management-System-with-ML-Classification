// DocumentClassificationService.cs
namespace DocumentManagementML.Application.Services
{
    public class DocumentClassificationService : IDocumentClassificationService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IDocumentClassificationModel _classificationModel;
        private readonly IMapper _mapper;
        private readonly ILogger<DocumentClassificationService> _logger;

        public DocumentClassificationService(
            IDocumentRepository documentRepository,
            IFileStorageService fileStorageService,
            IDocumentClassificationModel classificationModel,
            IMapper mapper,
            ILogger<DocumentClassificationService> logger)
        {
            _documentRepository = documentRepository;
            _fileStorageService = fileStorageService;
            _classificationModel = classificationModel;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<DocumentClassificationResultDto> ClassifyDocumentAsync(int documentId)
        {
            // Get document
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null)
            {
                throw new NotFoundException($"Document with ID {documentId} not found");
            }

            // Retrieve document file
            using var fileStream = await _fileStorageService.RetrieveFileAsync(document.FileLocation);
            var fileExtension = Path.GetExtension(document.FileLocation);

            // Classify document
            var result = await _classificationModel.ClassifyAsync(fileStream, fileExtension);

            // Map to DTO
            var resultDto = _mapper.Map<DocumentClassificationResultDto>(result);
            
            // Update document type if high confidence
            if (result.Success && result.Confidence > 0.7) // Configurable threshold
            {
                try
                {
                    // Logic to map predicted label to document type ID
                    // This would involve looking up the document type by name or implementing a mapping
                    // For simplicity, we're not implementing this part in this example
                    _logger.LogInformation($"Document {documentId} classified as {result.DocumentType} with confidence {result.Confidence}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error updating document type after classification: {ex.Message}");
                }
            }

            return resultDto;
        }

        public async Task<DocumentClassificationResultDto> ClassifyDocumentAsync(Stream documentStream, string fileName)
        {
            var fileExtension = Path.GetExtension(fileName);
            
            // Classify document
            var result = await _classificationModel.ClassifyAsync(documentStream, fileExtension);
            
            // Map to DTO
            return _mapper.Map<DocumentClassificationResultDto>(result);
        }

        public async Task TrainModelAsync()
        {
            try
            {
                _logger.LogInformation("Starting model training");
                
                // Get training data
                var trainingData = await GetTrainingDocumentsAsync();
                
                if (trainingData.Count == 0)
                {
                    _logger.LogWarning("No training data available");
                    return;
                }
                
                _logger.LogInformation($"Training with {trainingData.Count} documents");
                
                // Train model
                var success = await _classificationModel.TrainModelAsync(trainingData);
                
                if (success)
                {
                    _logger.LogInformation("Model training completed successfully");
                }
                else
                {
                    _logger.LogWarning("Model training completed with issues");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error training model: {ex.Message}");
                throw;
            }
        }

        public async Task<ModelMetricsDto> EvaluateModelAsync()
        {
            try
            {
                _logger.LogInformation("Starting model evaluation");
                
                // Get test data (in a real implementation, you would have separate test data)
                var testData = await GetTestDocumentsAsync();
                
                if (testData.Count == 0)
                {
                    _logger.LogWarning("No test data available");
                    return new ModelMetricsDto
                    {
                        Success = false,
                        ErrorMessage = "No test data available"
                    };
                }
                
                _logger.LogInformation($"Evaluating with {testData.Count} documents");
                
                // Evaluate model
                var metrics = await _classificationModel.EvaluateModelAsync(testData);
                
                // Map to DTO
                return _mapper.Map<ModelMetricsDto>(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error evaluating model: {ex.Message}");
                return new ModelMetricsDto
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        private async Task<List<TrainingDocument>> GetTrainingDocumentsAsync()
        {
            // In a real implementation, you would have a dedicated training data repository
            // This is a simplified example that uses existing documents
            
            var documents = await _documentRepository.GetAllAsync();
            var trainingData = new List<TrainingDocument>();
            
            foreach (var document in documents)
            {
                // Only use documents with a type assigned
                if (document.DocumentTypeId.HasValue && !document.IsDeleted)
                {
                    try
                    {
                        // Get document type name
                        var documentType = await _context.DocumentTypes
                            .Where(dt => dt.DocumentTypeId == document.DocumentTypeId.Value)
                            .FirstOrDefaultAsync();
                            
                        if (documentType == null || !documentType.IsActive)
                        {
                            continue;
                        }
                        
                        // Get document text
                        using var stream = await _fileStorageService.RetrieveFileAsync(document.FileLocation);
                        var fileExtension = Path.GetExtension(document.FileLocation);
                        var text = await _textExtractor.ExtractTextAsync(stream, fileExtension);
                        
                        if (string.IsNullOrWhiteSpace(text))
                        {
                            continue;
                        }
                        
                        trainingData.Add(new TrainingDocument
                        {
                            Text = text,
                            DocumentType = documentType.TypeName
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error processing document {document.DocumentId} for training: {ex.Message}");
                    }
                }
            }
            
            return trainingData;
        }

        private async Task<List<TrainingDocument>> GetTestDocumentsAsync()
        {
            // In a real implementation, you would have separate test data
            // For simplicity, we're using a subset of the training data
            var trainingData = await GetTrainingDocumentsAsync();
            
            // Take a small subset for testing
            return trainingData.Take(Math.Min(20, trainingData.Count / 5)).ToList();
        }
    }
}