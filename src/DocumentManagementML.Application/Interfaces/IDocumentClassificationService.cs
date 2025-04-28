// IDocumentClassificationService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DocumentManagementML.Application.DTOs;

namespace DocumentManagementML.Application.Interfaces
{
    /// <summary>
    /// Service interface for document classification operations
    /// </summary>
    public interface IDocumentClassificationService
    {
        /// <summary>
        /// Classifies a document based on its ID
        /// </summary>
        /// <param name="documentId">Document identifier</param>
        /// <returns>Classification result DTO</returns>
        Task<DocumentClassificationResultDto> ClassifyDocumentAsync(Guid documentId);
        
        /// <summary>
        /// Classifies a document using provided file stream
        /// </summary>
        /// <param name="fileStream">Document file stream</param>
        /// <param name="fileName">Original file name</param>
        /// <returns>Classification result DTO</returns>
        Task<DocumentClassificationResultDto> ClassifyDocumentAsync(Stream fileStream, string fileName);
        
        /// <summary>
        /// Classifies a document using a classification request
        /// </summary>
        /// <param name="request">Classification request DTO</param>
        /// <returns>Classification result DTO</returns>
        Task<DocumentClassificationResultDto> ClassifyDocumentAsync(ClassificationRequestDto request);
        
        /// <summary>
        /// Applies classification result to document metadata
        /// </summary>
        /// <param name="result">Classification result DTO</param>
        /// <returns>True if successfully applied, false otherwise</returns>
        Task<bool> ApplyClassificationResultAsync(DocumentClassificationResultDto result);
        
        /// <summary>
        /// Gets the current model metrics
        /// </summary>
        /// <returns>Model metrics DTO</returns>
        Task<ModelMetricsDto> GetModelMetricsAsync();
        
        /// <summary>
        /// Trains the classification model with existing documents
        /// </summary>
        /// <returns>True if successfully trained, false otherwise</returns>
        Task<bool> TrainModelAsync();
        
        /// <summary>
        /// Evaluates the model using test data
        /// </summary>
        /// <returns>Model evaluation metrics</returns>
        Task<ModelMetricsDto> EvaluateModelAsync();
        
        /// <summary>
        /// Batch classifies multiple documents
        /// </summary>
        /// <param name="documentIds">Collection of document identifiers</param>
        /// <returns>Collection of classification results</returns>
        Task<IEnumerable<DocumentClassificationResultDto>> BatchClassifyDocumentsAsync(IEnumerable<Guid> documentIds);
        
        /// <summary>
        /// Gets classification history for a document
        /// </summary>
        /// <param name="documentId">Document identifier</param>
        /// <returns>Collection of classification results</returns>
        Task<IEnumerable<DocumentClassificationResultDto>> GetClassificationHistoryAsync(Guid documentId);
        
        /// <summary>
        /// Resets the classification for a document
        /// </summary>
        /// <param name="documentId">Document identifier</param>
        /// <returns>True if successfully reset, false otherwise</returns>
        Task<bool> ResetDocumentClassificationAsync(Guid documentId);
    }
}