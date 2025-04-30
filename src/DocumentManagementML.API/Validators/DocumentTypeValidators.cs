// -----------------------------------------------------------------------------
// <copyright file="DocumentTypeValidators.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 30, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        FluentValidation validators for DocumentType DTOs
// -----------------------------------------------------------------------------
using DocumentManagementML.Application.DTOs;
using FluentValidation;

namespace DocumentManagementML.API.Validators
{
    /// <summary>
    /// Validator for document type creation DTOs
    /// </summary>
    public class DocumentTypeCreateDtoValidator : AbstractValidator<DocumentTypeCreateDto>
    {
        /// <summary>
        /// Initializes a new instance of the DocumentTypeCreateDtoValidator class
        /// </summary>
        public DocumentTypeCreateDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Document type name is required")
                .MaximumLength(100).WithMessage("Document type name cannot exceed 100 characters")
                .Matches(@"^[^<>\\/:*?""'|]+$").WithMessage("Document type name contains invalid characters");
            
            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
        }
    }
    
    /// <summary>
    /// Validator for document type update DTOs
    /// </summary>
    public class DocumentTypeUpdateDtoValidator : AbstractValidator<DocumentTypeUpdateDto>
    {
        /// <summary>
        /// Initializes a new instance of the DocumentTypeUpdateDtoValidator class
        /// </summary>
        public DocumentTypeUpdateDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Document type name is required")
                .MaximumLength(100).WithMessage("Document type name cannot exceed 100 characters")
                .Matches(@"^[^<>\\/:*?""'|]+$").WithMessage("Document type name contains invalid characters");
            
            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
        }
    }
}