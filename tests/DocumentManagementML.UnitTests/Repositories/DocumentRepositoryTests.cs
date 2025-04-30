// -----------------------------------------------------------------------------
// <copyright file="DocumentRepositoryTests.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 29, 2025
// Last Modified:      April 29, 2025
// Version:            0.9.0
// Description:        Unit tests for the DocumentRepository class
// -----------------------------------------------------------------------------

using DocumentManagementML.Domain.Entities;
using DocumentManagementML.Infrastructure.Data;
using DocumentManagementML.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DocumentManagementML.UnitTests.Repositories
{
    public class DocumentRepositoryTests
    {
        private DocumentManagementDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<DocumentManagementDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
                
            var context = new DocumentManagementDbContext(options);
            return context;
        }
        
        private async Task<DocumentManagementDbContext> CreateDbContextWithData()
        {
            var context = CreateDbContext();
            
            // Create document types
            var invoiceType = new DocumentType
            {
                DocumentTypeId = Guid.NewGuid(),
                Name = "Invoice",
                TypeName = "invoice"
            };
            
            var receiptType = new DocumentType
            {
                DocumentTypeId = Guid.NewGuid(),
                Name = "Receipt",
                TypeName = "receipt"
            };
            
            await context.DocumentTypes.AddRangeAsync(new[] { invoiceType, receiptType });
            await context.SaveChangesAsync();
            
            // Create users
            var user1 = new User
            {
                UserId = Guid.NewGuid(),
                Username = "user1",
                Email = "user1@example.com",
                IsActive = true
            };
            
            var user2 = new User
            {
                UserId = Guid.NewGuid(),
                Username = "user2",
                Email = "user2@example.com",
                IsActive = true
            };
            
            await context.Users.AddRangeAsync(new[] { user1, user2 });
            await context.SaveChangesAsync();
            
            // Add documents
            var documents = new List<Document>
            {
                new Document
                {
                    DocumentId = Guid.NewGuid(),
                    DocumentName = "Invoice-2025-001",
                    FileType = "pdf",
                    FileLocation = "/storage/invoices/invoice-2025-001.pdf",
                    FileSizeBytes = 1024,
                    DocumentTypeId = invoiceType.DocumentTypeId,
                    UploadedById = user1.UserId,
                    CreatedDate = DateTime.UtcNow.AddDays(-5),
                    LastModifiedDate = DateTime.UtcNow.AddDays(-5),
                    IsDeleted = false,
                    Description = "January invoice"
                },
                new Document
                {
                    DocumentId = Guid.NewGuid(),
                    DocumentName = "Invoice-2025-002",
                    FileType = "pdf",
                    FileLocation = "/storage/invoices/invoice-2025-002.pdf",
                    FileSizeBytes = 1536,
                    DocumentTypeId = invoiceType.DocumentTypeId,
                    UploadedById = user1.UserId,
                    CreatedDate = DateTime.UtcNow.AddDays(-3),
                    LastModifiedDate = DateTime.UtcNow.AddDays(-2),
                    IsDeleted = false,
                    Description = "February invoice"
                },
                new Document
                {
                    DocumentId = Guid.NewGuid(),
                    DocumentName = "Receipt-2025-001",
                    FileType = "pdf",
                    FileLocation = "/storage/receipts/receipt-2025-001.pdf",
                    FileSizeBytes = 512,
                    DocumentTypeId = receiptType.DocumentTypeId,
                    UploadedById = user2.UserId,
                    CreatedDate = DateTime.UtcNow.AddDays(-4),
                    LastModifiedDate = DateTime.UtcNow.AddDays(-4),
                    IsDeleted = false,
                    Description = "Office supplies receipt"
                },
                new Document
                {
                    DocumentId = Guid.NewGuid(),
                    DocumentName = "Deleted-Document",
                    FileType = "pdf",
                    FileLocation = "/storage/documents/deleted-document.pdf",
                    FileSizeBytes = 2048,
                    DocumentTypeId = invoiceType.DocumentTypeId,
                    UploadedById = user1.UserId,
                    CreatedDate = DateTime.UtcNow.AddDays(-10),
                    LastModifiedDate = DateTime.UtcNow.AddDays(-1),
                    IsDeleted = true,
                    Description = "This document is deleted"
                }
            };
            
            // Add metadata for the first document
            var firstDocumentMetadata = new List<DocumentMetadata>
            {
                new DocumentMetadata
                {
                    Id = Guid.NewGuid(),
                    DocumentId = documents[0].DocumentId,
                    MetadataKey = "InvoiceNumber",
                    MetadataValue = "INV-2025-001",
                    DataType = "string"
                },
                new DocumentMetadata
                {
                    Id = Guid.NewGuid(),
                    DocumentId = documents[0].DocumentId,
                    MetadataKey = "Amount",
                    MetadataValue = "1250.00",
                    DataType = "decimal"
                }
            };
            
            await context.Documents.AddRangeAsync(documents);
            await context.DocumentMetadata.AddRangeAsync(firstDocumentMetadata);
            await context.SaveChangesAsync();
            
            return context;
        }
        
        [Fact]
        public async Task GetByTypeIdAsync_ReturnsDocumentsOfSpecifiedType()
        {
            // Arrange
            var context = await CreateDbContextWithData();
            var repository = new DocumentRepository(context);
            var invoiceType = await context.DocumentTypes.FirstAsync(dt => dt.Name == "Invoice");
            
            // Act
            var result = await repository.GetByTypeIdAsync(invoiceType.DocumentTypeId);
            
            // Assert
            Assert.Equal(2, result.Count()); // 2 non-deleted invoice documents
            Assert.All(result, doc => Assert.Equal(invoiceType.DocumentTypeId, doc.DocumentTypeId));
            Assert.All(result, doc => Assert.False(doc.IsDeleted));
        }
        
        [Fact]
        public async Task GetActiveDocumentsAsync_ReturnsPaginatedActiveDocuments()
        {
            // Arrange
            var context = await CreateDbContextWithData();
            var repository = new DocumentRepository(context);
            
            // Act
            var result = await repository.GetActiveDocumentsAsync(0, 10);
            
            // Assert
            Assert.Equal(3, result.Count()); // 3 non-deleted documents
            Assert.All(result, doc => Assert.False(doc.IsDeleted));
        }
        
        [Fact]
        public async Task GetActiveDocumentsAsync_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            var context = await CreateDbContextWithData();
            var repository = new DocumentRepository(context);
            
            // Act
            var result = await repository.GetActiveDocumentsAsync(1, 1);
            
            // Assert
            Assert.Single(result); // Should return 1 document (second page with 1 item per page)
            Assert.All(result, doc => Assert.False(doc.IsDeleted));
        }
        
        [Fact]
        public async Task GetWithMetadataAsync_ReturnsDocumentWithItsMetadata()
        {
            // Arrange
            var context = await CreateDbContextWithData();
            var repository = new DocumentRepository(context);
            var documentWithMetadata = await context.Documents
                .FirstAsync(d => d.DocumentName == "Invoice-2025-001");
            
            // Act
            var result = await repository.GetWithMetadataAsync(documentWithMetadata.DocumentId);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(documentWithMetadata.DocumentId, result.DocumentId);
            Assert.Equal("Invoice-2025-001", result.DocumentName);
            Assert.Equal(2, result.MetadataItems.Count);
            Assert.Contains(result.MetadataItems, m => m.MetadataKey == "InvoiceNumber");
            Assert.Contains(result.MetadataItems, m => m.MetadataKey == "Amount");
        }
        
        [Fact]
        public async Task SoftDeleteAsync_MarksDocumentAsDeleted()
        {
            // Arrange
            var context = await CreateDbContextWithData();
            var repository = new DocumentRepository(context);
            var document = await context.Documents.FirstAsync(d => d.DocumentName == "Invoice-2025-001");
            Assert.False(document.IsDeleted); // Verify it's not deleted initially
            
            // Act
            await repository.SoftDeleteAsync(document.DocumentId);
            
            // Assert
            var updatedDocument = await context.Documents.FindAsync(document.DocumentId);
            Assert.NotNull(updatedDocument);
            Assert.True(updatedDocument.IsDeleted);
        }
        
        [Fact]
        public async Task GetByDocumentTypeAsync_ReturnsDocumentsOfSpecifiedType()
        {
            // Arrange
            var context = await CreateDbContextWithData();
            var repository = new DocumentRepository(context);
            var receiptType = await context.DocumentTypes.FirstAsync(dt => dt.Name == "Receipt");
            
            // Act
            var result = await repository.GetByDocumentTypeAsync(receiptType.DocumentTypeId);
            
            // Assert
            Assert.Single(result); // 1 non-deleted receipt document
            Assert.All(result, doc => Assert.Equal(receiptType.DocumentTypeId, doc.DocumentTypeId));
            Assert.All(result, doc => Assert.Equal("Receipt-2025-001", doc.DocumentName));
        }
        
        [Fact]
        public async Task GetByUploadedByAsync_ReturnsDocumentsUploadedBySpecifiedUser()
        {
            // Arrange
            var context = await CreateDbContextWithData();
            var repository = new DocumentRepository(context);
            var user = await context.Users.FirstAsync(u => u.Username == "user1");
            
            // Act
            var result = await repository.GetByUploadedByAsync(user.UserId);
            
            // Assert
            Assert.Equal(2, result.Count()); // 2 non-deleted documents from user1
            Assert.All(result, doc => Assert.Equal(user.UserId, doc.UploadedById));
            Assert.All(result, doc => Assert.False(doc.IsDeleted));
        }
        
        [Fact]
        public async Task SearchAsync_WithSearchTerm_ReturnsMatchingDocuments()
        {
            // Arrange
            var context = await CreateDbContextWithData();
            var repository = new DocumentRepository(context);
            
            // Act
            var result = await repository.SearchAsync("invoice");
            
            // Assert
            Assert.Equal(2, result.Count()); // 2 non-deleted invoice documents
            Assert.All(result, doc => Assert.True(
                doc.DocumentName.Contains("Invoice", StringComparison.OrdinalIgnoreCase) ||
                (doc.Description != null && doc.Description.Contains("invoice", StringComparison.OrdinalIgnoreCase))));
        }
        
        [Fact]
        public async Task SearchAsync_WithDocumentTypeFilter_ReturnsFilteredDocuments()
        {
            // Arrange
            var context = await CreateDbContextWithData();
            var repository = new DocumentRepository(context);
            var invoiceType = await context.DocumentTypes.FirstAsync(dt => dt.Name == "Invoice");
            
            // Act
            var result = await repository.SearchAsync("invoice", invoiceType.DocumentTypeId);
            
            // Assert
            Assert.Equal(2, result.Count()); // 2 non-deleted invoice documents
            Assert.All(result, doc => Assert.Equal(invoiceType.DocumentTypeId, doc.DocumentTypeId));
        }
        
        [Fact]
        public async Task GetRecentDocumentsAsync_ReturnsSpecifiedNumberOfMostRecentDocuments()
        {
            // Arrange
            var context = await CreateDbContextWithData();
            var repository = new DocumentRepository(context);
            
            // Act
            var result = await repository.GetRecentDocumentsAsync(2);
            
            // Assert
            Assert.Equal(2, result.Count()); // Top 2 most recent documents
            
            // Verify they are ordered by created date descending
            var orderedResult = result.OrderByDescending(d => d.CreatedDate).ToList();
            for (int i = 0; i < orderedResult.Count; i++)
            {
                Assert.Equal(orderedResult[i].DocumentId, result.ElementAt(i).DocumentId);
            }
        }
        
        [Fact]
        public async Task GetDocumentCountAsync_ReturnsCorrectCount()
        {
            // Arrange
            var context = await CreateDbContextWithData();
            var repository = new DocumentRepository(context);
            var invoiceType = await context.DocumentTypes.FirstAsync(dt => dt.Name == "Invoice");
            
            // Act
            var totalCount = await repository.GetDocumentCountAsync();
            var invoiceCount = await repository.GetDocumentCountAsync(invoiceType.DocumentTypeId);
            
            // Assert
            Assert.Equal(3, totalCount); // 3 non-deleted documents
            Assert.Equal(2, invoiceCount); // 2 non-deleted invoice documents
        }
        
        [Fact]
        public async Task GetSearchResultCountAsync_ReturnsCorrectCount()
        {
            // Arrange
            var context = await CreateDbContextWithData();
            var repository = new DocumentRepository(context);
            var invoiceType = await context.DocumentTypes.FirstAsync(dt => dt.Name == "Invoice");
            
            // Act
            var invoiceSearchCount = await repository.GetSearchResultCountAsync("invoice");
            var receiptSearchCount = await repository.GetSearchResultCountAsync("receipt");
            var filteredSearchCount = await repository.GetSearchResultCountAsync("invoice", invoiceType.DocumentTypeId);
            
            // Assert
            Assert.Equal(2, invoiceSearchCount); // 2 documents matching "invoice"
            Assert.Equal(1, receiptSearchCount); // 1 document matching "receipt"
            Assert.Equal(2, filteredSearchCount); // 2 invoice documents matching "invoice"
        }
    }
}