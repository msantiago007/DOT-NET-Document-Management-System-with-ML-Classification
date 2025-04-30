// -----------------------------------------------------------------------------
// <copyright file="IDocumentClassificationService.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            February 22, 2025
// Last Modified:      April 29, 2025
// Version:            0.9.0
// Description:        Service interface for document classification operations
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentManagementML.Application.DTOs;

namespace DocumentManagementML.Application.Interfaces
{
    /// <summary>
    /// Service interface for document classification operations.
    /// Phase 1 implementation with minimal ML functionality.
    /// </summary>
    public interface IDocumentClassificationService
    {
        /// <summary>
        /// Classifies a document based on its ID.
        /// </summary>
        /// <param name="documentId">Document identifier.</param>
        /// <returns>Classification result DTO.</returns>
        Task<DocumentClassificationResultDto> ClassifyDocumentAsync(Guid documentId);
        
        /// <summary>
        /// Classifies a document from a file stream.
        /// </summary>
        /// <param name="fileStream">The document file stream.</param>
        /// <param name="fileName">The document file name.</param>
        /// <returns>Classification result DTO.</returns>
        Task<DocumentClassificationResultDto> ClassifyDocumentAsync(System.IO.Stream fileStream, string fileName);
        
        /// <summary>
        /// Batch classifies multiple documents.
        /// </summary>
        /// <param name="documentIds">Collection of document identifiers.</param>
        /// <returns>Collection of classification results.</returns>
        Task<List<DocumentClassificationResultDto>> ClassifyDocumentsAsync(IEnumerable<Guid> documentIds);
        
        /// <summary>
        /// Gets the current model metrics.
        /// </summary>
        /// <returns>Model metrics DTO.</returns>
        Task<ModelMetricsDto> GetModelMetricsAsync();
        
        /// <summary>
        /// Trains the document classification model.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task TrainModelAsync();
        
        /// <summary>
        /// Evaluates the document classification model.
        /// </summary>
        /// <returns>Model metrics DTO.</returns>
        Task<ModelMetricsDto> EvaluateModelAsync();
    }
}