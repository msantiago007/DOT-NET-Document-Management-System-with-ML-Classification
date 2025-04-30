// -----------------------------------------------------------------------------
// <copyright file="BaseRepositoryTests.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 29, 2025
// Last Modified:      April 29, 2025
// Version:            0.9.0
// Description:        Unit tests for the BaseRepository class
// -----------------------------------------------------------------------------

using DocumentManagementML.Domain.Entities;
using DocumentManagementML.Domain.Repositories;
using DocumentManagementML.Infrastructure.Data;
using DocumentManagementML.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DocumentManagementML.UnitTests.Repositories
{
    public class BaseRepositoryTests
    {
        // A simple implementation of BaseRepository for testing
        private class TestRepository : BaseRepository<DocumentType>
        {
            public TestRepository(DocumentManagementDbContext context) : base(context)
            {
            }
        }
        
        private DocumentManagementDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<DocumentManagementDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
                
            var context = new DocumentManagementDbContext(options);
            return context;
        }
        
        [Fact]
        public async Task BeginTransactionAsync_CreatesTransaction()
        {
            // Arrange
            var context = CreateDbContext();
            var repository = new TestRepository(context);
            
            // Act
            var transaction = await repository.BeginTransactionAsync();
            
            // Assert
            Assert.NotNull(transaction);
            
            // Clean up
            await transaction.RollbackAsync();
        }
        
        [Fact]
        public async Task CommitTransactionAsync_CommitsChanges()
        {
            // Arrange
            var context = CreateDbContext();
            var repository = new TestRepository(context);
            
            var documentType = new DocumentType
            {
                DocumentTypeId = Guid.NewGuid(),
                Name = "Test Type",
                TypeName = "testtype"
            };
            
            // Start a transaction
            var transaction = await repository.BeginTransactionAsync();
            
            try
            {
                // Add the document type without saving changes
                context.DocumentTypes.Add(documentType);
                
                // Commit the transaction
                await repository.CommitTransactionAsync(transaction);
                
                // Assert
                var savedDocumentType = await context.DocumentTypes.FindAsync(documentType.DocumentTypeId);
                Assert.NotNull(savedDocumentType);
                Assert.Equal("Test Type", savedDocumentType.Name);
            }
            finally
            {
                // Clean up
                if (transaction is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
        
        [Fact]
        public async Task RollbackTransactionAsync_DiscardsChanges()
        {
            // Arrange
            var context = CreateDbContext();
            var repository = new TestRepository(context);
            
            var documentType = new DocumentType
            {
                DocumentTypeId = Guid.NewGuid(),
                Name = "Test Type",
                TypeName = "testtype"
            };
            
            // Start a transaction
            var transaction = await repository.BeginTransactionAsync();
            
            try
            {
                // Add the document type without saving changes
                context.DocumentTypes.Add(documentType);
                
                // Rollback the transaction
                await repository.RollbackTransactionAsync(transaction);
                
                // Assert
                var savedDocumentType = await context.DocumentTypes.FindAsync(documentType.DocumentTypeId);
                Assert.Null(savedDocumentType); // Should not be saved
            }
            finally
            {
                // Clean up
                if (transaction is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
        
        [Fact]
        public async Task CommitTransactionAsync_WithNullTransaction_ThrowsArgumentNullException()
        {
            // Arrange
            var context = CreateDbContext();
            var repository = new TestRepository(context);
            
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                repository.CommitTransactionAsync(null));
        }
        
        [Fact]
        public async Task RollbackTransactionAsync_WithNullTransaction_ThrowsArgumentNullException()
        {
            // Arrange
            var context = CreateDbContext();
            var repository = new TestRepository(context);
            
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                repository.RollbackTransactionAsync(null));
        }
        
        [Fact]
        public async Task AddAsync_AddsEntityAndReturnsIt()
        {
            // Arrange
            var context = CreateDbContext();
            var repository = new TestRepository(context);
            
            var documentType = new DocumentType
            {
                Name = "Test Type",
                TypeName = "testtype"
            };
            
            // Act
            var result = await repository.AddAsync(documentType);
            
            // Assert
            Assert.NotEqual(Guid.Empty, result.DocumentTypeId); // ID is generated
            
            // Verify it was added to the database
            var savedDocumentType = await context.DocumentTypes.FindAsync(result.DocumentTypeId);
            Assert.NotNull(savedDocumentType);
            Assert.Equal("Test Type", savedDocumentType.Name);
        }
        
        [Fact]
        public async Task UpdateAsync_UpdatesEntityAndReturnsIt()
        {
            // Arrange
            var context = CreateDbContext();
            var repository = new TestRepository(context);
            
            // Add an entity
            var documentType = new DocumentType
            {
                DocumentTypeId = Guid.NewGuid(),
                Name = "Original Name",
                TypeName = "originalname"
            };
            
            context.DocumentTypes.Add(documentType);
            await context.SaveChangesAsync();
            
            // Modify the entity
            documentType.Name = "Updated Name";
            
            // Act
            var result = await repository.UpdateAsync(documentType);
            
            // Assert
            Assert.Equal("Updated Name", result.Name);
            
            // Verify it was updated in the database
            var savedDocumentType = await context.DocumentTypes.FindAsync(documentType.DocumentTypeId);
            Assert.NotNull(savedDocumentType);
            Assert.Equal("Updated Name", savedDocumentType.Name);
        }
        
        [Fact]
        public async Task DeleteAsync_RemovesEntityAndReturnsTrue()
        {
            // Arrange
            var context = CreateDbContext();
            var repository = new TestRepository(context);
            
            // Add an entity
            var documentType = new DocumentType
            {
                DocumentTypeId = Guid.NewGuid(),
                Name = "Test Type",
                TypeName = "testtype"
            };
            
            context.DocumentTypes.Add(documentType);
            await context.SaveChangesAsync();
            
            // Act
            var result = await repository.DeleteAsync(documentType.DocumentTypeId);
            
            // Assert
            Assert.True(result);
            
            // Verify it was removed from the database
            var savedDocumentType = await context.DocumentTypes.FindAsync(documentType.DocumentTypeId);
            Assert.Null(savedDocumentType);
        }
        
        [Fact]
        public async Task DeleteAsync_WithInvalidId_ReturnsFalse()
        {
            // Arrange
            var context = CreateDbContext();
            var repository = new TestRepository(context);
            
            // Act
            var result = await repository.DeleteAsync(Guid.NewGuid());
            
            // Assert
            Assert.False(result);
        }
        
        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsEntity()
        {
            // Arrange
            var context = CreateDbContext();
            var repository = new TestRepository(context);
            
            // Add an entity
            var documentType = new DocumentType
            {
                DocumentTypeId = Guid.NewGuid(),
                Name = "Test Type",
                TypeName = "testtype"
            };
            
            context.DocumentTypes.Add(documentType);
            await context.SaveChangesAsync();
            
            // Act
            var result = await repository.GetByIdAsync(documentType.DocumentTypeId);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(documentType.DocumentTypeId, result.DocumentTypeId);
            Assert.Equal("Test Type", result.Name);
        }
        
        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var context = CreateDbContext();
            var repository = new TestRepository(context);
            
            // Act
            var result = await repository.GetByIdAsync(Guid.NewGuid());
            
            // Assert
            Assert.Null(result);
        }
    }
}