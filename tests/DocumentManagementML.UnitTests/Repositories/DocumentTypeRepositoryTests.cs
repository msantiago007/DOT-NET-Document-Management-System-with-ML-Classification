// -----------------------------------------------------------------------------
// <copyright file="DocumentTypeRepositoryTests.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 29, 2025
// Last Modified:      April 29, 2025
// Version:            0.9.0
// Description:        Unit tests for the DocumentTypeRepository class
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
    public class DocumentTypeRepositoryTests
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
            
            // Add test document types
            var documentTypes = new List<DocumentType>
            {
                new DocumentType
                {
                    DocumentTypeId = Guid.NewGuid(),
                    Name = "Invoice",
                    TypeName = "invoice",
                    Description = "Invoice documents",
                    SchemaDefinition = "{}",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddDays(-10),
                    LastModifiedDate = DateTime.UtcNow.AddDays(-5)
                },
                new DocumentType
                {
                    DocumentTypeId = Guid.NewGuid(),
                    Name = "Receipt",
                    TypeName = "receipt",
                    Description = "Receipt documents",
                    SchemaDefinition = "{}",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddDays(-8),
                    LastModifiedDate = DateTime.UtcNow.AddDays(-4)
                },
                new DocumentType
                {
                    DocumentTypeId = Guid.NewGuid(),
                    Name = "Contract",
                    TypeName = "contract",
                    Description = "Contract documents",
                    SchemaDefinition = "{}",
                    IsActive = false, // Inactive
                    CreatedDate = DateTime.UtcNow.AddDays(-15),
                    LastModifiedDate = DateTime.UtcNow.AddDays(-2)
                }
            };
            
            await context.DocumentTypes.AddRangeAsync(documentTypes);
            await context.SaveChangesAsync();
            
            return context;
        }
        
        [Fact]
        public async Task GetAllAsync_ReturnsAllDocumentTypes()
        {
            // Arrange
            var context = await CreateDbContextWithData();
            var repository = new DocumentTypeRepository(context);
            
            // Act
            var result = await repository.GetAllAsync();
            
            // Assert
            Assert.Equal(3, result.Count());
            Assert.Contains(result, dt => dt.Name == "Invoice");
            Assert.Contains(result, dt => dt.Name == "Receipt");
            Assert.Contains(result, dt => dt.Name == "Contract");
        }
        
        [Fact]
        public async Task GetActiveTypesAsync_ReturnsOnlyActiveDocumentTypes()
        {
            // Arrange
            var context = await CreateDbContextWithData();
            var repository = new DocumentTypeRepository(context);
            
            // Act
            var result = await repository.GetActiveTypesAsync();
            
            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, dt => dt.Name == "Invoice");
            Assert.Contains(result, dt => dt.Name == "Receipt");
            Assert.DoesNotContain(result, dt => dt.Name == "Contract");
        }
        
        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsDocumentType()
        {
            // Arrange
            var context = await CreateDbContextWithData();
            var repository = new DocumentTypeRepository(context);
            var documentType = await context.DocumentTypes.FirstAsync(dt => dt.Name == "Invoice");
            
            // Act
            var result = await repository.GetByIdAsync(documentType.DocumentTypeId);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(documentType.DocumentTypeId, result.DocumentTypeId);
            Assert.Equal("Invoice", result.Name);
            Assert.Equal("invoice", result.TypeName);
        }
        
        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var context = await CreateDbContextWithData();
            var repository = new DocumentTypeRepository(context);
            
            // Act
            var result = await repository.GetByIdAsync(Guid.NewGuid());
            
            // Assert
            Assert.Null(result);
        }
        
        [Fact]
        public async Task GetByNameAsync_WithValidName_ReturnsDocumentType()
        {
            // Arrange
            var context = await CreateDbContextWithData();
            var repository = new DocumentTypeRepository(context);
            
            // Act
            var result = await repository.GetByNameAsync("Invoice");
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal("Invoice", result.Name);
            Assert.Equal("invoice", result.TypeName);
        }
        
        [Fact]
        public async Task GetByNameAsync_WithInvalidName_ReturnsNull()
        {
            // Arrange
            var context = await CreateDbContextWithData();
            var repository = new DocumentTypeRepository(context);
            
            // Act
            var result = await repository.GetByNameAsync("NonExistent");
            
            // Assert
            Assert.Null(result);
        }
        
        [Fact]
        public async Task GetDocumentTypeCountAsync_ReturnsCorrectCount()
        {
            // Arrange
            var context = await CreateDbContextWithData();
            var repository = new DocumentTypeRepository(context);
            
            // Act
            var totalCount = await repository.GetDocumentTypeCountAsync();
            var activeCount = await repository.GetDocumentTypeCountAsync(activeOnly: true);
            
            // Assert
            Assert.Equal(3, totalCount);
            Assert.Equal(2, activeCount);
        }
        
        [Fact]
        public async Task AddAsync_AddsNewDocumentType()
        {
            // Arrange
            var context = CreateDbContext();
            var repository = new DocumentTypeRepository(context);
            var newDocumentType = new DocumentType
            {
                Name = "Policy",
                Description = "Policy documents"
            };
            
            // Act
            var result = await repository.AddAsync(newDocumentType);
            
            // Assert
            Assert.NotEqual(Guid.Empty, result.DocumentTypeId);
            Assert.Equal("Policy", result.Name);
            Assert.Equal("policy", result.TypeName); // TypeName should be set by the repository
            Assert.Equal("Policy documents", result.Description);
            
            // Verify it was added to the database
            var storedDocumentType = await context.DocumentTypes.FindAsync(result.DocumentTypeId);
            Assert.NotNull(storedDocumentType);
            Assert.Equal("Policy", storedDocumentType.Name);
        }
        
        [Fact]
        public async Task UpdateAsync_UpdatesExistingDocumentType()
        {
            // Arrange
            var context = await CreateDbContextWithData();
            var repository = new DocumentTypeRepository(context);
            var documentType = await context.DocumentTypes.FirstAsync(dt => dt.Name == "Invoice");
            
            // Modify the document type
            documentType.Name = "Updated Invoice";
            documentType.Description = "Updated description";
            
            // Act
            var result = await repository.UpdateAsync(documentType);
            
            // Assert
            Assert.Equal("Updated Invoice", result.Name);
            Assert.Equal("Updated description", result.Description);
            
            // Verify it was updated in the database
            var storedDocumentType = await context.DocumentTypes.FindAsync(documentType.DocumentTypeId);
            Assert.NotNull(storedDocumentType);
            Assert.Equal("Updated Invoice", storedDocumentType.Name);
            Assert.Equal("Updated description", storedDocumentType.Description);
        }
        
        [Fact]
        public async Task DeleteAsync_WithValidId_DeletesDocumentType()
        {
            // Arrange
            var context = await CreateDbContextWithData();
            var repository = new DocumentTypeRepository(context);
            var documentType = await context.DocumentTypes.FirstAsync(dt => dt.Name == "Invoice");
            
            // Act
            var result = await repository.DeleteAsync(documentType.DocumentTypeId);
            
            // Assert
            Assert.True(result);
            
            // Verify it was deleted from the database
            var storedDocumentType = await context.DocumentTypes.FindAsync(documentType.DocumentTypeId);
            Assert.Null(storedDocumentType);
        }
        
        [Fact]
        public async Task DeleteAsync_WithInvalidId_ReturnsFalse()
        {
            // Arrange
            var context = await CreateDbContextWithData();
            var repository = new DocumentTypeRepository(context);
            
            // Act
            var result = await repository.DeleteAsync(Guid.NewGuid());
            
            // Assert
            Assert.False(result);
        }
        
        [Fact]
        public async Task BeginTransactionAsync_ReturnsTransaction()
        {
            // Arrange
            var context = CreateDbContext();
            var repository = new DocumentTypeRepository(context);
            
            // Act
            var transaction = await repository.BeginTransactionAsync();
            
            // Assert
            Assert.NotNull(transaction);
            
            // Clean up
            await transaction.RollbackAsync();
        }
    }
}