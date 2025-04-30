// -----------------------------------------------------------------------------
// <copyright file="DocumentValidators.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 30, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        FluentValidation validators for Document DTOs
// -----------------------------------------------------------------------------
using System;
using System.Linq;
using DocumentManagementML.Application.DTOs;
using FluentValidation;

namespace DocumentManagementML.API.Validators
{
    /// <summary>
    /// Validator for document creation DTOs
    /// </summary>
    public class DocumentCreateDtoValidator : AbstractValidator<DocumentCreateDto>
    {
        // List of allowed file types
        private static readonly string[] AllowedFileTypes = new[]
        {
            "pdf", "docx", "doc", "txt", "rtf", "xlsx", "xls", "pptx", "ppt", 
            "csv", "html", "htm", "xml", "json", "jpg", "jpeg", "png", "gif", "bmp"
        };
        
        /// <summary>
        /// Initializes a new instance of the DocumentCreateDtoValidator class
        /// </summary>
        public DocumentCreateDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Document name is required")
                .MaximumLength(255).WithMessage("Document name cannot exceed 255 characters")
                .Matches(@"^[^<>\\/:*?""'|]+$").WithMessage("Document name contains invalid characters");
            
            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");
            
            RuleFor(x => x.FileType)
                .NotEmpty().WithMessage("File type is required")
                .Must(BeValidFileType).WithMessage($"File type must be one of: {string.Join(", ", AllowedFileTypes)}");
            
            RuleForEach(x => x.Metadata)
                .Must(kv => !string.IsNullOrWhiteSpace(kv.Key))
                .WithMessage("Metadata keys cannot be empty");

            RuleForEach(x => x.Metadata)
                .Must(kv => kv.Key.Length <= 50)
                .WithMessage("Metadata key length cannot exceed 50 characters");

            RuleForEach(x => x.Metadata)
                .Must(kv => kv.Value == null || kv.Value.Length <= 500)
                .WithMessage("Metadata value length cannot exceed 500 characters");
        }
        
        private bool BeValidFileType(string fileType)
        {
            if (string.IsNullOrWhiteSpace(fileType))
            {
                return false;
            }
            
            // Normalize file type (remove dot, convert to lowercase)
            var normalizedFileType = fileType.TrimStart('.').ToLowerInvariant();
            
            return AllowedFileTypes.Contains(normalizedFileType);
        }
    }
    
    /// <summary>
    /// Validator for document update DTOs
    /// </summary>
    public class DocumentUpdateDtoValidator : AbstractValidator<DocumentUpdateDto>
    {
        /// <summary>
        /// Initializes a new instance of the DocumentUpdateDtoValidator class
        /// </summary>
        public DocumentUpdateDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Document name is required")
                .MaximumLength(255).WithMessage("Document name cannot exceed 255 characters")
                .Matches(@"^[^<>\\/:*?""'|]+$").WithMessage("Document name contains invalid characters");
            
            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");
            
            RuleForEach(x => x.Metadata)
                .Must(kv => !string.IsNullOrWhiteSpace(kv.Key))
                .WithMessage("Metadata keys cannot be empty");

            RuleForEach(x => x.Metadata)
                .Must(kv => kv.Key.Length <= 50)
                .WithMessage("Metadata key length cannot exceed 50 characters");

            RuleForEach(x => x.Metadata)
                .Must(kv => kv.Value == null || kv.Value.Length <= 500)
                .WithMessage("Metadata value length cannot exceed 500 characters");
        }
    }
}