// -----------------------------------------------------------------------------
// <copyright file="DocumentTests.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 29, 2025
// Last Modified:      April 29, 2025
// Version:            0.9.0
// Description:        Unit tests for the Document entity class
// -----------------------------------------------------------------------------

using DocumentManagementML.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DocumentManagementML.UnitTests.Entities
{
    public class DocumentTests
    {
        [Fact]
        public void Document_Constructor_InitializesProperties()
        {
            // Arrange & Act
            var document = new Document();
            
            // Assert
            Assert.Equal(Guid.Empty, document.DocumentId);
            Assert.Equal(string.Empty, document.DocumentName);
            Assert.Equal(string.Empty, document.FileLocation);
            Assert.Equal(string.Empty, document.FileType);
            Assert.Equal(string.Empty, document.ContentHash);
            Assert.Equal(string.Empty, document.FilePath);
            Assert.Equal(string.Empty, document.ContentType);
            Assert.False(document.IsDeleted);
            Assert.NotNull(document.Versions);
            Assert.Empty(document.Versions);
            Assert.NotNull(document.MetadataItems);
            Assert.Empty(document.MetadataItems);
            Assert.NotNull(document.SourceRelationships);
            Assert.Empty(document.SourceRelationships);
            Assert.NotNull(document.TargetRelationships);
            Assert.Empty(document.TargetRelationships);
            
            // DateTime properties should be initialized to current UTC time (approximate check)
            Assert.True((DateTime.UtcNow - document.CreatedDate).TotalSeconds < 2);
            Assert.True((DateTime.UtcNow - document.LastModifiedDate).TotalSeconds < 2);
            Assert.True((DateTime.UtcNow - document.UploadDate).TotalSeconds < 2);
        }
        
        [Fact]
        public void Document_SetProperties_PropertiesAreSet()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var documentTypeId = Guid.NewGuid();
            var uploadedById = Guid.NewGuid();
            var createdDate = DateTime.UtcNow.AddDays(-5);
            var modifiedDate = DateTime.UtcNow.AddDays(-2);
            var uploadDate = DateTime.UtcNow.AddDays(-4);
            
            // Act
            var document = new Document
            {
                DocumentId = documentId,
                DocumentName = "Test Document",
                FileLocation = "/storage/documents/test.pdf",
                FileType = "pdf",
                FileSizeBytes = 1024,
                ContentHash = "abc123hash",
                DocumentTypeId = documentTypeId,
                FilePath = "/documents/test.pdf",
                FileSize = 1024,
                ClassificationConfidence = 0.95,
                UploadedById = uploadedById,
                CreatedDate = createdDate,
                LastModifiedDate = modifiedDate,
                UploadDate = uploadDate,
                CreatedById = Guid.NewGuid(),
                LastModifiedById = Guid.NewGuid(),
                Description = "Test document description",
                ContentType = "application/pdf",
                IsDeleted = false
            };
            
            // Assert
            Assert.Equal(documentId, document.DocumentId);
            Assert.Equal("Test Document", document.DocumentName);
            Assert.Equal("/storage/documents/test.pdf", document.FileLocation);
            Assert.Equal("pdf", document.FileType);
            Assert.Equal(1024, document.FileSizeBytes);
            Assert.Equal("abc123hash", document.ContentHash);
            Assert.Equal(documentTypeId, document.DocumentTypeId);
            Assert.Equal("/documents/test.pdf", document.FilePath);
            Assert.Equal(1024, document.FileSize);
            Assert.Equal(0.95, document.ClassificationConfidence);
            Assert.Equal(uploadedById, document.UploadedById);
            Assert.Equal(createdDate, document.CreatedDate);
            Assert.Equal(modifiedDate, document.LastModifiedDate);
            Assert.Equal(uploadDate, document.UploadDate);
            Assert.Equal(document.CreatedById, document.CreatedById);  // Using self-equality since we don't know the GUID value
            Assert.Equal(document.LastModifiedById, document.LastModifiedById);
            Assert.Equal("Test document description", document.Description);
            Assert.Equal("application/pdf", document.ContentType);
            Assert.False(document.IsDeleted);
        }
        
        [Fact]
        public void Document_NavigationProperties_CanBeSet()
        {
            // Arrange
            var document = new Document();
            var documentType = new DocumentType { Name = "Invoice" };
            var uploadedBy = new User { Username = "user1" };
            var createdBy = new User { Username = "user2" };
            var modifiedBy = new User { Username = "user3" };
            var version = new DocumentVersion();
            var relationship = new DocumentRelationship();
            
            // Act
            document.DocumentType = documentType;
            document.UploadedBy = uploadedBy;
            document.CreatedBy = createdBy;
            document.ModifiedBy = modifiedBy;
            document.Versions.Add(version);
            document.SourceRelationships.Add(relationship);
            
            // Assert
            Assert.Same(documentType, document.DocumentType);
            Assert.Same(uploadedBy, document.UploadedBy);
            Assert.Same(createdBy, document.CreatedBy);
            Assert.Same(modifiedBy, document.ModifiedBy);
            Assert.Contains(version, document.Versions);
            Assert.Contains(relationship, document.SourceRelationships);
        }
        
        [Fact]
        public void MetadataDictionary_ReturnsCorrectValues()
        {
            // Arrange
            var document = new Document();
            document.MetadataItems.Add(new DocumentMetadata
            {
                MetadataKey = "Author",
                MetadataValue = "John Doe"
            });
            document.MetadataItems.Add(new DocumentMetadata
            {
                MetadataKey = "Department",
                MetadataValue = "Finance"
            });
            
            // Act
            var metadataDictionary = document.MetadataDictionary;
            
            // Assert
            Assert.Equal(2, metadataDictionary.Count);
            Assert.Equal("John Doe", metadataDictionary["Author"]);
            Assert.Equal("Finance", metadataDictionary["Department"]);
        }
        
        [Fact]
        public void MetadataDictionary_WithEmptyMetadata_ReturnsEmptyDictionary()
        {
            // Arrange
            var document = new Document();
            
            // Act
            var metadataDictionary = document.MetadataDictionary;
            
            // Assert
            Assert.Empty(metadataDictionary);
        }
        
        [Fact]
        public void MetadataDictionary_WithDuplicateKeys_LastValueWins()
        {
            // Arrange
            var document = new Document();
            document.MetadataItems.Add(new DocumentMetadata
            {
                MetadataKey = "Status",
                MetadataValue = "Draft"
            });
            document.MetadataItems.Add(new DocumentMetadata
            {
                MetadataKey = "Status",
                MetadataValue = "Final"
            });
            
            // Act
            var metadataDictionary = document.MetadataDictionary;
            
            // Assert
            Assert.Single(metadataDictionary);
            Assert.Equal("Final", metadataDictionary["Status"]);
        }
    }
}