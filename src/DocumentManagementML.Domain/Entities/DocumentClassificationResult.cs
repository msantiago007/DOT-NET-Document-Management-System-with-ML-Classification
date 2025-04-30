// -----------------------------------------------------------------------------
// <copyright file="DocumentClassificationResult.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            February 22, 2025
// Last Modified:      April 29, 2025
// Version:            0.9.0
// Description:        Defines the result of document classification operations
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace DocumentManagementML.Domain.Entities
{
    public class DocumentClassificationResult
    {
        public bool Success { get; set; }
        
        /// <summary>
        /// Gets a value indicating whether the classification was successful.
        /// This is an alias for Success for backward compatibility.
        /// </summary>
        public bool IsSuccessful { get => Success; set => Success = value; }
        public string? ErrorMessage { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public float Confidence { get; set; }
        public Dictionary<string, float> AllPredictions { get; set; } = new Dictionary<string, float>();
        public Guid? DocumentId { get; set; }
        public string DocumentName { get; set; } = string.Empty;
    }
} 