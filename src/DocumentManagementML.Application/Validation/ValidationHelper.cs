// -----------------------------------------------------------------------------
// <copyright file="ValidationHelper.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 30, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        Helper methods for validation in the application layer
// -----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using DocumentManagementML.Application.Exceptions;

namespace DocumentManagementML.Application.Validation
{
    /// <summary>
    /// Helper methods for validation in the application layer
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Validates a DTO using DataAnnotations
        /// </summary>
        /// <param name="dto">The DTO to validate</param>
        /// <param name="validationContext">Additional validation context (optional)</param>
        /// <exception cref="ValidationException">Thrown when validation fails</exception>
        public static void ValidateDto(object dto, ValidationContext? validationContext = null)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }
            
            var validationResults = new List<ValidationResult>();
            var context = validationContext ?? new ValidationContext(dto);
            
            if (!Validator.TryValidateObject(dto, context, validationResults, validateAllProperties: true))
            {
                var errors = validationResults
                    .GroupBy(r => r.MemberNames.FirstOrDefault() ?? string.Empty)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(r => r.ErrorMessage).ToList());
                
                throw new DocumentManagementML.Application.Exceptions.ValidationException(errors);
            }
        }
        
        /// <summary>
        /// Validates that a name doesn't contain invalid characters
        /// </summary>
        /// <param name="name">The name to validate</param>
        /// <param name="fieldName">Field name for error reporting</param>
        /// <exception cref="ValidationException">Thrown when validation fails</exception>
        public static void ValidateName(string name, string fieldName = "Name")
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new DocumentManagementML.Application.Exceptions.ValidationException(
                    fieldName, $"{fieldName} is required");
            }
            
            // Check for invalid characters in name
            var invalidCharsRegex = new Regex(@"[<>\\/:*?""'|]");
            if (invalidCharsRegex.IsMatch(name))
            {
                throw new DocumentManagementML.Application.Exceptions.ValidationException(
                    fieldName, $"{fieldName} contains invalid characters: < > \\ / : * ? \" ' |");
            }
        }
        
        /// <summary>
        /// Validates an email address
        /// </summary>
        /// <param name="email">Email to validate</param>
        /// <param name="fieldName">Field name for error reporting</param>
        /// <exception cref="ValidationException">Thrown when validation fails</exception>
        public static void ValidateEmail(string email, string fieldName = "Email")
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new DocumentManagementML.Application.Exceptions.ValidationException(
                    fieldName, $"{fieldName} is required");
            }
            
            // Basic email validation (RFC 5322)
            var emailRegex = new Regex(@"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$");
            if (!emailRegex.IsMatch(email))
            {
                throw new DocumentManagementML.Application.Exceptions.ValidationException(
                    fieldName, $"{fieldName} is not a valid email address");
            }
        }
        
        /// <summary>
        /// Validates a password strength
        /// </summary>
        /// <param name="password">Password to validate</param>
        /// <param name="fieldName">Field name for error reporting</param>
        /// <param name="minLength">Minimum password length</param>
        /// <exception cref="ValidationException">Thrown when validation fails</exception>
        public static void ValidatePassword(string password, string fieldName = "Password", int minLength = 6)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new DocumentManagementML.Application.Exceptions.ValidationException(
                    fieldName, $"{fieldName} is required");
            }
            
            if (password.Length < minLength)
            {
                throw new DocumentManagementML.Application.Exceptions.ValidationException(
                    fieldName, $"{fieldName} must be at least {minLength} characters long");
            }
            
            // Check for at least one digit, lowercase, uppercase, and special character
            var hasNumber = new Regex(@"[0-9]+");
            var hasUpperChar = new Regex(@"[A-Z]+");
            var hasLowerChar = new Regex(@"[a-z]+");
            var hasSpecialChar = new Regex(@"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]+");
            
            bool isStrong = hasNumber.IsMatch(password) && 
                            hasUpperChar.IsMatch(password) && 
                            hasLowerChar.IsMatch(password) && 
                            hasSpecialChar.IsMatch(password);
                            
            if (!isStrong)
            {
                throw new DocumentManagementML.Application.Exceptions.ValidationException(
                    fieldName, $"{fieldName} must contain at least one number, one uppercase letter, one lowercase letter, and one special character");
            }
        }
        
        /// <summary>
        /// Validates that a GUID is not empty
        /// </summary>
        /// <param name="id">GUID to validate</param>
        /// <param name="fieldName">Field name for error reporting</param>
        /// <exception cref="ValidationException">Thrown when validation fails</exception>
        public static void ValidateId(Guid id, string fieldName = "Id")
        {
            if (id == Guid.Empty)
            {
                throw new DocumentManagementML.Application.Exceptions.ValidationException(
                    fieldName, $"{fieldName} cannot be empty");
            }
        }
        
        /// <summary>
        /// Validates a file type against allowed types
        /// </summary>
        /// <param name="fileType">File type to validate</param>
        /// <param name="allowedTypes">List of allowed file types</param>
        /// <param name="fieldName">Field name for error reporting</param>
        /// <exception cref="ValidationException">Thrown when validation fails</exception>
        public static void ValidateFileType(string fileType, IEnumerable<string> allowedTypes, string fieldName = "FileType")
        {
            if (string.IsNullOrWhiteSpace(fileType))
            {
                throw new DocumentManagementML.Application.Exceptions.ValidationException(
                    fieldName, $"{fieldName} is required");
            }
            
            // Normalize file type (remove dot, convert to lowercase)
            var normalizedFileType = fileType.TrimStart('.').ToLowerInvariant();
            
            // Convert allowed types to lowercase for case-insensitive comparison
            var normalizedAllowedTypes = allowedTypes.Select(t => t.TrimStart('.').ToLowerInvariant());
            
            if (!normalizedAllowedTypes.Contains(normalizedFileType))
            {
                throw new DocumentManagementML.Application.Exceptions.ValidationException(
                    fieldName, $"File type '{fileType}' is not allowed. Allowed types: {string.Join(", ", allowedTypes)}");
            }
        }
        
        /// <summary>
        /// Validates the size of a file
        /// </summary>
        /// <param name="fileSizeBytes">File size in bytes</param>
        /// <param name="maxSizeBytes">Maximum allowed size in bytes</param>
        /// <param name="fieldName">Field name for error reporting</param>
        /// <exception cref="ValidationException">Thrown when validation fails</exception>
        public static void ValidateFileSize(long fileSizeBytes, long maxSizeBytes, string fieldName = "FileSize")
        {
            if (fileSizeBytes <= 0)
            {
                throw new DocumentManagementML.Application.Exceptions.ValidationException(
                    fieldName, $"{fieldName} must be greater than 0");
            }
            
            if (fileSizeBytes > maxSizeBytes)
            {
                // Convert to MB for more readable error message
                var fileSizeMB = Math.Round(fileSizeBytes / 1024.0 / 1024.0, 2);
                var maxSizeMB = Math.Round(maxSizeBytes / 1024.0 / 1024.0, 2);
                
                throw new DocumentManagementML.Application.Exceptions.ValidationException(
                    fieldName, $"File size ({fileSizeMB} MB) exceeds the maximum allowed size of {maxSizeMB} MB");
            }
        }
        
        /// <summary>
        /// Validates metadata key-value pairs
        /// </summary>
        /// <param name="metadata">Metadata dictionary to validate</param>
        /// <param name="maxKeyLength">Maximum key length</param>
        /// <param name="maxValueLength">Maximum value length</param>
        /// <param name="fieldName">Field name for error reporting</param>
        /// <exception cref="ValidationException">Thrown when validation fails</exception>
        public static void ValidateMetadata(
            Dictionary<string, string> metadata, 
            int maxKeyLength = 50, 
            int maxValueLength = 500,
            string fieldName = "Metadata")
        {
            if (metadata == null || !metadata.Any())
            {
                return; // Empty metadata is valid
            }
            
            var errors = new List<string>();
            
            foreach (var kvp in metadata)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key))
                {
                    errors.Add("Metadata keys cannot be empty");
                    continue;
                }
                
                if (kvp.Key.Length > maxKeyLength)
                {
                    errors.Add($"Metadata key '{kvp.Key}' exceeds maximum length of {maxKeyLength} characters");
                }
                
                if (kvp.Value != null && kvp.Value.Length > maxValueLength)
                {
                    errors.Add($"Metadata value for key '{kvp.Key}' exceeds maximum length of {maxValueLength} characters");
                }
            }
            
            if (errors.Any())
            {
                throw new DocumentManagementML.Application.Exceptions.ValidationException(
                    fieldName, string.Join("; ", errors));
            }
        }
    }
}