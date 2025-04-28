using System;

namespace DocumentManagementML.Application.DTOs
{
    /// <summary>
    /// Data transfer object for document type scores in classification results.
    /// </summary>
    public class DocumentTypeScoreDto
    {
        /// <summary>
        /// Gets or sets the ID of the document type.
        /// </summary>
        public Guid DocumentTypeId { get; set; }

        /// <summary>
        /// Gets or sets the name of the document type.
        /// </summary>
        public string? DocumentTypeName { get; set; }

        /// <summary>
        /// Gets or sets the confidence score for the document type.
        /// </summary>
        public double Score { get; set; }

        /// <summary>
        /// Gets or sets the rank of this score within the classification result.
        /// </summary>
        public int Rank { get; set; }
    }
}