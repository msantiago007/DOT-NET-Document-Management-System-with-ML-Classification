// -----------------------------------------------------------------------------
// <copyright file="SimpleEntityTests.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 30, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        Simple entity tests to verify core functionality
// -----------------------------------------------------------------------------

using DocumentManagementML.Domain.Entities;
using System;
using System.Collections.Generic;
using Xunit;

namespace DocumentManagementML.UnitTests.Entities
{
    /// <summary>
    /// Contains simplified tests for the core entities.
    /// </summary>
    public class SimpleEntityTests
    {
        [Fact]
        public void Document_MetadataDictionary_WorksCorrectly()
        {
            // Arrange
            var document = new Document();
            var metadata1 = new DocumentMetadata { MetadataKey = "Key1", MetadataValue = "Value1" };
            var metadata2 = new DocumentMetadata { MetadataKey = "Key2", MetadataValue = "Value2" };
            
            // Act
            document.MetadataItems.Add(metadata1);
            document.MetadataItems.Add(metadata2);
            var dictionary = document.MetadataDictionary;
            
            // Assert
            Assert.Equal(2, dictionary.Count);
            Assert.Equal("Value1", dictionary["Key1"]);
            Assert.Equal("Value2", dictionary["Key2"]);
        }
        
        [Fact]
        public void DocumentMetadata_KeyValue_WorksCorrectly()
        {
            // Arrange
            var metadata = new DocumentMetadata
            {
                MetadataKey = "TestKey",
                MetadataValue = "TestValue"
            };
            
            // Act & Assert
            Assert.Equal("TestKey", metadata.Key);
            Assert.Equal("TestValue", metadata.Value);
        }
        
        [Fact]
        public void DocumentType_Properties_CanBeSet()
        {
            // Arrange
            var id = Guid.NewGuid();
            var documentType = new DocumentType
            {
                DocumentTypeId = id,
                Name = "Test Type",
                TypeName = "testtype",
                Description = "Test description",
                SchemaDefinition = "{}",
                IsActive = true
            };
            
            // Act & Assert
            Assert.Equal(id, documentType.DocumentTypeId);
            Assert.Equal("Test Type", documentType.Name);
            Assert.Equal("testtype", documentType.TypeName);
            Assert.Equal("Test description", documentType.Description);
            Assert.Equal("{}", documentType.SchemaDefinition);
            Assert.True(documentType.IsActive);
        }
        
        [Fact]
        public void DocumentClassificationResult_Properties_CanBeSet()
        {
            // Arrange
            var result = new DocumentClassificationResult
            {
                Success = true,
                DocumentType = "Invoice",
                Confidence = 0.95f,
                DocumentName = "test.pdf"
            };
            
            // Act & Assert
            Assert.True(result.Success);
            Assert.True(result.IsSuccessful); // Alias for Success
            Assert.Equal("Invoice", result.DocumentType);
            Assert.Equal(0.95f, result.Confidence);
            Assert.Equal("test.pdf", result.DocumentName);
        }
        
        [Fact]
        public void Document_NavigationProperties_CanBeSet()
        {
            // Arrange
            var document = new Document();
            var documentType = new DocumentType { Name = "Test Type" };
            
            // Act
            document.DocumentType = documentType;
            
            // Assert
            Assert.NotNull(document.DocumentType);
            Assert.Equal("Test Type", document.DocumentType.Name);
        }
    }
}