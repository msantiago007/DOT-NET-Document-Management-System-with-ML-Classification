// -----------------------------------------------------------------------------
// <copyright file="ValidationExceptionTests.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 29, 2025
// Last Modified:      April 29, 2025
// Version:            0.9.0
// Description:        Unit tests for ValidationException
// -----------------------------------------------------------------------------
using DocumentManagementML.Application.Exceptions;
using System.Collections.Generic;
using Xunit;

namespace DocumentManagementML.UnitTests.Exceptions
{
    public class ValidationExceptionTests
    {
        [Fact]
        public void ValidationException_DefaultConstructor_ShouldCreateEmptyErrorsDictionary()
        {
            // Act
            var exception = new ValidationException();
            
            // Assert
            Assert.NotNull(exception.Errors);
            Assert.Empty(exception.Errors);
            Assert.Equal("One or more validation failures have occurred.", exception.Message);
        }
        
        [Fact]
        public void ValidationException_WithErrorsDictionary_ShouldUseProvidedErrors()
        {
            // Arrange
            var errors = new Dictionary<string, string[]>
            {
                { "Name", new[] { "Name is required" } },
                { "Email", new[] { "Invalid email format", "Email is already in use" } }
            };
            
            // Act
            var exception = new ValidationException(errors);
            
            // Assert
            Assert.Equal(errors, exception.Errors);
            Assert.Equal("One or more validation failures have occurred.", exception.Message);
        }
        
        [Fact]
        public void ValidationException_WithPropertyAndErrorMessage_ShouldCreateSinglePropertyError()
        {
            // Act
            var exception = new ValidationException("Name", "Name is required");
            
            // Assert
            Assert.Single(exception.Errors);
            Assert.Contains("Name", exception.Errors.Keys);
            Assert.Single(exception.Errors["Name"]);
            Assert.Equal("Name is required", exception.Errors["Name"][0]);
            Assert.Equal("One or more validation failures have occurred.", exception.Message);
        }
        
        [Fact]
        public void ValidationException_WithErrorMessage_ShouldCreateGeneralError()
        {
            // Act
            var exception = new ValidationException("Validation failed");
            
            // Assert
            Assert.Equal("Validation failed", exception.Message);
            Assert.Single(exception.Errors);
            Assert.Contains("General", exception.Errors.Keys);
            Assert.Single(exception.Errors["General"]);
            Assert.Equal("Validation failed", exception.Errors["General"][0]);
        }
    }
}