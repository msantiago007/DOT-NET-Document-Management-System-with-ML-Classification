using System;
using System.Collections.Generic;

namespace DocumentManagementML.Application.DTOs
{
    /// <summary>
    /// Data transfer object for document classification results.
    /// </summary>
    public class DocumentClassificationResultDto
    {
        /// <summary>
        /// Gets or sets a value indicating whether the classification was successful.
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Gets or sets the error message, if any.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the document ID.
        /// </summary>
        public Guid DocumentId { get; set; }

        /// <summary>
        /// Gets or sets the predicted document type ID.
        /// </summary>
        public Guid? PredictedDocumentTypeId { get; set; }

        /// <summary>
        /// Gets or sets the predicted document type name.
        /// </summary>
        public string? PredictedDocumentTypeName { get; set; }

        /// <summary>
        /// Gets or sets the confidence score.
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// Gets or sets the classification date.
        /// </summary>
        public DateTime ClassificationDate { get; set; }

        /// <summary>
        /// Gets or sets the document type scores.
        /// </summary>
        public List<DocumentTypeScoreDto> DocumentTypeScores { get; set; } = new List<DocumentTypeScoreDto>();
    }
}