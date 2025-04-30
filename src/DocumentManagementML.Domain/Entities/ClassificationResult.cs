using System;
using System.Collections.Generic;

namespace DocumentManagementML.Domain.Entities
{
    /// <summary>
    /// Represents the result of a document classification operation.
    /// </summary>
    public class ClassificationResult
    {
        /// <summary>
        /// Gets or sets the unique identifier for the classification result.
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Gets or sets the document ID that was classified.
        /// </summary>
        public Guid DocumentId { get; set; }
        
        /// <summary>
        /// Gets or sets the document name that was classified.
        /// </summary>
        public string DocumentName { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the predicted document type ID.
        /// </summary>
        public Guid? PredictedDocumentTypeId { get; set; }
        
        /// <summary>
        /// Gets or sets the predicted document type.
        /// </summary>
        public DocumentType? PredictedDocumentType { get; set; }
        
        /// <summary>
        /// Gets or sets the confidence score for the prediction.
        /// </summary>
        public double Confidence { get; set; }
        
        /// <summary>
        /// Gets or sets the collection of all document type predictions with their scores.
        /// </summary>
        public ICollection<DocumentTypeScore> AllScores { get; set; } = new List<DocumentTypeScore>();
        
        /// <summary>
        /// Gets or sets a value indicating whether the classification was successful.
        /// </summary>
        public bool IsSuccessful { get; set; }
        
        /// <summary>
        /// Gets or sets the error message if the classification was not successful.
        /// </summary>
        public string? ErrorMessage { get; set; }
        
        /// <summary>
        /// Gets or sets the date and time when the classification was performed.
        /// </summary>
        public DateTime ClassificationDate { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Gets or sets the document that was classified.
        /// </summary>
        public Document? Document { get; set; }
    }
}