// MLDTOs.cs - This file will be replaced by individual DTO files in a future phase

using System;
using System.Collections.Generic;

namespace DocumentManagementML.Application.DTOs
{
    // Note: These DTOs are defined in their own files but kept here temporarily
    // for backward compatibility. They will be removed in a future phase.
    
    // See DocumentClassificationResultDto.cs for the current implementation
    
    // See DocumentTypeScoreDto.cs for the current implementation
    
    // See ClassificationRequestDto.cs for the current implementation
    public class ClassificationRequestDto
    {
        public Guid DocumentId { get; set; }
        
        public string FilePath { get; set; } = string.Empty;
        
        public string FileType { get; set; } = string.Empty;
        
        public bool ApplyResult { get; set; } = false;
        
        public bool ForceReclassify { get; set; } = false;
        
        // Added for compatibility with tests
        public string? Text { get; set; }
    }
    
    // See ModelMetricsDto.cs for the current implementation
}