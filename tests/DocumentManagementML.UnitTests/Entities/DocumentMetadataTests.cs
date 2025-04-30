// -----------------------------------------------------------------------------
// <copyright file="DocumentMetadataTests.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 29, 2025
// Last Modified:      April 29, 2025
// Version:            0.9.0
// Description:        Unit tests for the DocumentMetadata entity class
// -----------------------------------------------------------------------------

using DocumentManagementML.Domain.Entities;
using System;
using Xunit;

namespace DocumentManagementML.UnitTests.Entities
{
    public class DocumentMetadataTests
    {
        [Fact]
        public void DocumentMetadata_Constructor_InitializesProperties()
        {
            // Arrange & Act
            var metadata = new DocumentMetadata();
            
            // Assert
            Assert.Equal(Guid.Empty, metadata.Id);
            Assert.Equal(Guid.Empty, metadata.DocumentId);
            Assert.Equal(string.Empty, metadata.MetadataKey);
            Assert.Equal(string.Empty, metadata.MetadataValue);
            Assert.Equal("string", metadata.DataType);
            Assert.Null(metadata.Document);
            
            // DateTime properties should be initialized to current UTC time (approximate check)
            Assert.True((DateTime.UtcNow - metadata.CreatedDate).TotalSeconds < 2);
            Assert.True((DateTime.UtcNow - metadata.LastModifiedDate).TotalSeconds < 2);
        }
        
        [Fact]
        public void DocumentMetadata_SetProperties_PropertiesAreSet()
        {
            // Arrange
            var id = Guid.NewGuid();
            var documentId = Guid.NewGuid();
            var createdDate = DateTime.UtcNow.AddDays(-5);
            var modifiedDate = DateTime.UtcNow.AddDays(-2);
            
            // Act
            var metadata = new DocumentMetadata
            {
                Id = id,
                DocumentId = documentId,
                MetadataKey = "Author",
                MetadataValue = "John Doe",
                DataType = "string",
                CreatedDate = createdDate,
                LastModifiedDate = modifiedDate
            };
            
            // Assert
            Assert.Equal(id, metadata.Id);
            Assert.Equal(documentId, metadata.DocumentId);
            Assert.Equal("Author", metadata.MetadataKey);
            Assert.Equal("John Doe", metadata.MetadataValue);
            Assert.Equal("string", metadata.DataType);
            Assert.Equal(createdDate, metadata.CreatedDate);
            Assert.Equal(modifiedDate, metadata.LastModifiedDate);
        }
        
        [Fact]
        public void Key_ReturnsMetadataKey()
        {
            // Arrange
            var metadata = new DocumentMetadata
            {
                MetadataKey = "Department"
            };
            
            // Act & Assert
            Assert.Equal("Department", metadata.Key);
            
            // Verify it's a computed property
            metadata.MetadataKey = "NewDepartment";
            Assert.Equal("NewDepartment", metadata.Key);
        }
        
        [Fact]
        public void Value_ReturnsMetadataValue()
        {
            // Arrange
            var metadata = new DocumentMetadata
            {
                MetadataValue = "Finance"
            };
            
            // Act & Assert
            Assert.Equal("Finance", metadata.Value);
            
            // Verify it's a computed property
            metadata.MetadataValue = "HR";
            Assert.Equal("HR", metadata.Value);
        }
        
        [Fact]
        public void DocumentMetadata_NavigationProperty_CanBeSet()
        {
            // Arrange
            var metadata = new DocumentMetadata();
            var document = new Document { DocumentName = "Test Document" };
            
            // Act
            metadata.Document = document;
            
            // Assert
            Assert.Same(document, metadata.Document);
            Assert.Equal("Test Document", metadata.Document.DocumentName);
        }
        
        [Theory]
        [InlineData("string", "John Doe")]
        [InlineData("number", "42")]
        [InlineData("date", "2025-04-29")]
        [InlineData("boolean", "true")]
        [InlineData("json", "{\"name\":\"John\",\"age\":30}")]
        public void DocumentMetadata_SupportsVariousDataTypes(string dataType, string value)
        {
            // Arrange & Act
            var metadata = new DocumentMetadata
            {
                MetadataKey = "TestKey",
                MetadataValue = value,
                DataType = dataType
            };
            
            // Assert
            Assert.Equal("TestKey", metadata.MetadataKey);
            Assert.Equal(value, metadata.MetadataValue);
            Assert.Equal(dataType, metadata.DataType);
        }
    }
}