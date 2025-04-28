// DocumentClassificationModel.cs
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using DocumentManagementML.Infrastructure.Settings;
using DocumentManagementML.Domain.Services;
using DocumentManagementML.Domain.Entities;

namespace DocumentManagementML.Infrastructure.ML
{
    public class DocumentClassificationModel : IDocumentClassificationModel
    {
        private readonly ITextExtractor _textExtractor;
        private readonly IOptions<MLSettings> _mlSettings;
        private readonly ILogger<DocumentClassificationModel> _logger;
        private PredictionEngine<DocumentInput, DocumentPrediction>? _predictionEngine;
        private readonly object _lockObj = new object();

        public DocumentClassificationModel(
            ITextExtractor textExtractor,
            IOptions<MLSettings> mlSettings,
            ILogger<DocumentClassificationModel> logger)
        {
            _textExtractor = textExtractor;
            _mlSettings = mlSettings;
            _logger = logger;
            LoadModel();
        }

        public async Task<DocumentClassificationResult> ClassifyAsync(Stream documentStream, string fileExtension)
        {
            try
            {
                // Extract text from document
                var text = await _textExtractor.ExtractTextAsync(documentStream, fileExtension);
                if (string.IsNullOrWhiteSpace(text))
                {
                    _logger.LogWarning("No text could be extracted from the document");
                    return new DocumentClassificationResult
                    {
                        IsSuccessful = false,
                        ErrorMessage = "No text could be extracted from the document"
                    };
                }

                // Ensure model is loaded
                if (_predictionEngine == null)
                {
                    LoadModel();
                    if (_predictionEngine == null)
                    {
                        return new DocumentClassificationResult
                        {
                            IsSuccessful = false,
                            ErrorMessage = "Model could not be loaded"
                        };
                    }
                }

                // Make prediction
                var input = new DocumentInput { Text = text };
                var prediction = _predictionEngine.Predict(input);

                // Create result
                return new DocumentClassificationResult
                {
                    IsSuccessful = true,
                    DocumentType = prediction.PredictedLabel,
                    Confidence = prediction.Score.Max(),
                    AllPredictions = prediction.Score
                        .Select((score, index) => new KeyValuePair<string, float>($"Class{index}", score))
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error classifying document: {ex.Message}");
                return new DocumentClassificationResult
                {
                    IsSuccessful = false,
                    ErrorMessage = $"Classification error: {ex.Message}"
                };
            }
        }

        public async Task<bool> TrainModelAsync(IEnumerable<TrainingDocument> trainingData)
        {
            try
            {
                _logger.LogInformation("Starting model training");

                // Create ML context
                var mlContext = new MLContext(seed: 1);

                // Load training data
                var data = mlContext.Data.LoadFromEnumerable(trainingData.Select(td => new DocumentInput
                {
                    Text = td.Text,
                    Label = td.DocumentType
                }));

                // Create data processing pipeline
                var pipeline = mlContext.Transforms.Text
                    .FeaturizeText("Features", nameof(DocumentInput.Text))
                    .Append(mlContext.Transforms.NormalizeMinMax("Features"))
                    .AppendCacheCheckpoint(mlContext)
                    .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
                    .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

                // Train model
                _logger.LogInformation("Training the model...");
                var model = pipeline.Fit(data);

                // Save model
                var modelPath = Path.Combine(_mlSettings.Value.ModelDirectory, "document_classification_model.zip");
                Directory.CreateDirectory(_mlSettings.Value.ModelDirectory);
                mlContext.Model.Save(model, data.Schema, modelPath);

                _logger.LogInformation($"Model saved to: {modelPath}");

                // Update prediction engine
                lock (_lockObj)
                {
                    _predictionEngine = mlContext.Model.CreatePredictionEngine<DocumentInput, DocumentPrediction>(model);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error training model: {ex.Message}");
                return false;
            }
        }

        public async Task<ModelMetrics> EvaluateModelAsync(IEnumerable<TrainingDocument> testData)
        {
            try
            {
                _logger.LogInformation("Starting model evaluation");

                // Create ML context
                var mlContext = new MLContext(seed: 1);

                // Load model
                var modelPath = Path.Combine(_mlSettings.Value.ModelDirectory, "document_classification_model.zip");
                if (!File.Exists(modelPath))
                {
                    _logger.LogError("Model file not found for evaluation");
                    return new ModelMetrics { IsSuccessful = false, ErrorMessage = "Model file not found" };
                }

                ITransformer model;
                using (var stream = File.OpenRead(modelPath))
                {
                    model = mlContext.Model.Load(stream, out var _);
                }

                // Load test data
                var testDataEnumerable = testData.Select(td => new DocumentInput
                {
                    Text = td.Text,
                    Label = td.DocumentType
                });
                
                var testDataset = mlContext.Data.LoadFromEnumerable(testDataEnumerable);

                // Evaluate model
                var predictions = model.Transform(testDataset);
                var metrics = mlContext.MulticlassClassification.Evaluate(predictions);

                // Return metrics
                return new ModelMetrics
                {
                    IsSuccessful = true,
                    MicroAccuracy = metrics.MicroAccuracy,
                    MacroAccuracy = metrics.MacroAccuracy,
                    LogLoss = metrics.LogLoss,
                    LogLossReduction = metrics.LogLossReduction
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error evaluating model: {ex.Message}");
                return new ModelMetrics
                {
                    IsSuccessful = false,
                    ErrorMessage = $"Evaluation error: {ex.Message}"
                };
            }
        }

        private void LoadModel()
        {
            try
            {
                var modelPath = Path.Combine(_mlSettings.Value.ModelDirectory, "document_classification_model.zip");
                if (!File.Exists(modelPath))
                {
                    _logger.LogWarning($"Model file not found at: {modelPath}");
                    return;
                }

                var mlContext = new MLContext(seed: 1);
                ITransformer model;
                
                using (var stream = File.OpenRead(modelPath))
                {
                    model = mlContext.Model.Load(stream, out var _);
                }

                lock (_lockObj)
                {
                    _predictionEngine = mlContext.Model.CreatePredictionEngine<DocumentInput, DocumentPrediction>(model);
                }

                _logger.LogInformation("Model loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading model: {ex.Message}");
            }
        }
    }

    public class DocumentInput
    {
        [LoadColumn(0)]
        public string Text { get; set; } = string.Empty;

        [LoadColumn(1), ColumnName("Label")]
        public string Label { get; set; } = string.Empty;
    }

    public class DocumentPrediction
    {
        [ColumnName("PredictedLabel")]
        public string PredictedLabel { get; set; } = string.Empty;

        [ColumnName("Score")]
        public float[] Score { get; set; } = Array.Empty<float>();
    }
}