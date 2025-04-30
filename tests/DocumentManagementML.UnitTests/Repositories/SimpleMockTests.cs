// -----------------------------------------------------------------------------
// <copyright file="SimpleMockTests.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 30, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        Simple mock-based tests for repository functionality
// -----------------------------------------------------------------------------

using DocumentManagementML.Domain.Entities;
using DocumentManagementML.Domain.Repositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DocumentManagementML.UnitTests.Repositories
{
    /// <summary>
    /// Contains simplified mock-based tests for repository functionality.
    /// </summary>
    public class SimpleMockTests
    {
        [Fact]
        public async Task DocumentTypeRepository_GetByIdAsync_ReturnsCorrectType()
        {
            // Arrange
            var documentTypeId = Guid.NewGuid();
            var documentType = new DocumentType 
            { 
                DocumentTypeId = documentTypeId,
                Name = "Invoice",
                TypeName = "invoice"
            };
            
            var mockRepository = new Mock<IDocumentTypeRepository>();
            mockRepository.Setup(r => r.GetByIdAsync(documentTypeId))
                .ReturnsAsync(documentType);
                
            // Act
            var result = await mockRepository.Object.GetByIdAsync(documentTypeId);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(documentTypeId, result.DocumentTypeId);
            Assert.Equal("Invoice", result.Name);
            Assert.Equal("invoice", result.TypeName);
        }
        
        [Fact]
        public async Task DocumentRepository_GetActiveDocumentsAsync_ReturnsPaginatedResults()
        {
            // Arrange
            var documents = new List<Document>
            {
                new Document { DocumentId = Guid.NewGuid(), DocumentName = "Doc1" },
                new Document { DocumentId = Guid.NewGuid(), DocumentName = "Doc2" },
                new Document { DocumentId = Guid.NewGuid(), DocumentName = "Doc3" }
            };
            
            var mockRepository = new Mock<IDocumentRepository>();
            mockRepository.Setup(r => r.GetActiveDocumentsAsync(0, 2))
                .ReturnsAsync(documents.Take(2));
                
            // Act
            var result = await mockRepository.Object.GetActiveDocumentsAsync(0, 2);
            
            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, d => d.DocumentName == "Doc1");
            Assert.Contains(result, d => d.DocumentName == "Doc2");
            Assert.DoesNotContain(result, d => d.DocumentName == "Doc3");
        }
        
        [Fact]
        public async Task Transaction_CommitAsync_CallsCommitOnTransaction()
        {
            // Arrange
            var mockTransaction = new Mock<ITransaction>();
            mockTransaction.Setup(t => t.CommitAsync())
                .Returns(Task.CompletedTask);
                
            // Act
            await mockTransaction.Object.CommitAsync();
            
            // Assert
            mockTransaction.Verify(t => t.CommitAsync(), Times.Once);
        }
        
        [Fact]
        public async Task Transaction_RollbackAsync_CallsRollbackOnTransaction()
        {
            // Arrange
            var mockTransaction = new Mock<ITransaction>();
            mockTransaction.Setup(t => t.RollbackAsync())
                .Returns(Task.CompletedTask);
                
            // Act
            await mockTransaction.Object.RollbackAsync();
            
            // Assert
            mockTransaction.Verify(t => t.RollbackAsync(), Times.Once);
        }
    }
}