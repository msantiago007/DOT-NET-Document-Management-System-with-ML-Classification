// src/DocumentManagementML.Domain/Entities/DocumentTypeScore.cs
using System;

namespace DocumentManagementML.Domain.Entities
{
    /// <summary>
    /// Represents a score for a document type in classification results.
    /// Used for storing confidence scores for different document types during classification.
    /// </summary>
    public class DocumentTypeScore
    {
        /// <summary>
        /// Gets or sets the unique identifier for the document type score.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the classification result this score belongs to.
        /// </summary>
        public Guid ClassificationResultId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the document type.
        /// </summary>
        public Guid DocumentTypeId { get; set; }

        /// <summary>
        /// Gets or sets the confidence score for the document type.
        /// </summary>
        public double Score { get; set; }

        /// <summary>
        /// Gets or sets the rank of this score within the classification result.
        /// </summary>
        public int Rank { get; set; }

        // Navigation properties

        /// <summary>
        /// Gets or sets the classification result this score belongs to.
        /// </summary>
        public ClassificationResult? ClassificationResult { get; set; }

        /// <summary>
        /// Gets or sets the document type this score is for.
        /// </summary>
        public DocumentType? DocumentType { get; set; }
    }
}