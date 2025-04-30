// -----------------------------------------------------------------------------
// <copyright file="TransactionHandlingTests.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 29, 2025
// Last Modified:      April 29, 2025
// Version:            0.9.0
// Description:        Integration tests for transaction handling across repositories
// -----------------------------------------------------------------------------

using DocumentManagementML.Domain.Entities;
using DocumentManagementML.Domain.Repositories;
using DocumentManagementML.Infrastructure.Data;
using DocumentManagementML.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DocumentManagementML.UnitTests.Integration
{
    public class TransactionHandlingTests
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
            
            // Add a document type
            var documentType = new DocumentType
            {
                DocumentTypeId = Guid.NewGuid(),
                Name = "Invoice",
                TypeName = "invoice",
                IsActive = true
            };
            
            context.DocumentTypes.Add(documentType);
            await context.SaveChangesAsync();
            
            return context;
        }
        
        [Fact]
        public async Task Transaction_CommittedAcrossRepositories_ChangesArePersisted()
        {
            // Arrange
            var context = await CreateDbContextWithData();
            var documentTypeRepository = new DocumentTypeRepository(context);
            var documentRepository = new DocumentRepository(context);
            
            var documentType = await context.DocumentTypes.FirstAsync();
            var documentId = Guid.NewGuid();
            
            // Start a transaction
            var transaction = await documentTypeRepository.BeginTransactionAsync();
            
            try
            {
                // Update document type
                documentType.Description = "Updated in transaction";
                await documentTypeRepository.UpdateAsync(documentType);
                
                // Create a new document
                var document = new Document
                {
                    DocumentId = documentId,
                    DocumentName = "Created in transaction",
                    DocumentTypeId = documentType.DocumentTypeId,
                    FileType = "pdf"
                };
                await documentRepository.AddAsync(document);
                
                // Commit the transaction
                await documentTypeRepository.CommitTransactionAsync(transaction);
            }
            finally
            {
                if (transaction is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            
            // Assert - changes should be persisted after commit
            var updatedType = await context.DocumentTypes.FirstAsync();
            var createdDocument = await context.Documents.FindAsync(documentId);
            
            Assert.Equal("Updated in transaction", updatedType.Description);
            Assert.NotNull(createdDocument);
            Assert.Equal("Created in transaction", createdDocument.DocumentName);
        }
        
        [Fact]
        public async Task Transaction_RolledBackAcrossRepositories_ChangesAreDiscarded()
        {
            // Arrange
            var context = await CreateDbContextWithData();
            var documentTypeRepository = new DocumentTypeRepository(context);
            var documentRepository = new DocumentRepository(context);
            
            var documentType = await context.DocumentTypes.FirstAsync();
            var originalDescription = documentType.Description;
            var documentId = Guid.NewGuid();
            
            // Start a transaction
            var transaction = await documentTypeRepository.BeginTransactionAsync();
            
            try
            {
                // Update document type
                documentType.Description = "This will be rolled back";
                await documentTypeRepository.UpdateAsync(documentType);
                
                // Create a new document
                var document = new Document
                {
                    DocumentId = documentId,
                    DocumentName = "This will be rolled back",
                    DocumentTypeId = documentType.DocumentTypeId,
                    FileType = "pdf"
                };
                await documentRepository.AddAsync(document);
                
                // Roll back the transaction
                await documentTypeRepository.RollbackTransactionAsync(transaction);
            }
            finally
            {
                if (transaction is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            
            // Assert - changes should be discarded after rollback
            var updatedType = await context.DocumentTypes.FirstAsync();
            var createdDocument = await context.Documents.FindAsync(documentId);
            
            Assert.Equal(originalDescription, updatedType.Description);
            Assert.NotEqual("This will be rolled back", updatedType.Description);
            Assert.Null(createdDocument);
        }
        
        [Fact]
        public async Task Transaction_ThrowsExceptionDuringOperation_ChangesAreRolledBack()
        {
            // Arrange
            var context = await CreateDbContextWithData();
            var documentTypeRepository = new DocumentTypeRepository(context);
            var documentRepository = new DocumentRepository(context);
            
            var documentType = await context.DocumentTypes.FirstAsync();
            var originalDescription = documentType.Description;
            var documentId = Guid.NewGuid();
            
            // Act & Assert
            ITransaction transaction = null;
            try
            {
                // Start a transaction
                transaction = await documentTypeRepository.BeginTransactionAsync();
                
                // Update document type
                documentType.Description = "This will be rolled back due to error";
                await documentTypeRepository.UpdateAsync(documentType);
                
                // Create a document
                var document = new Document
                {
                    DocumentId = documentId,
                    DocumentName = "This will be rolled back due to error",
                    DocumentTypeId = documentType.DocumentTypeId,
                    FileType = "pdf"
                };
                await documentRepository.AddAsync(document);
                
                // Simulate an exception
                throw new InvalidOperationException("Simulated error during transaction");
                
                // This line should not be reached
                // await documentTypeRepository.CommitTransactionAsync(transaction);
            }
            catch (InvalidOperationException)
            {
                // Expected exception, roll back the transaction
                if (transaction != null)
                {
                    await documentTypeRepository.RollbackTransactionAsync(transaction);
                }
            }
            finally
            {
                if (transaction is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            
            // Assert - changes should be discarded after rollback
            var updatedType = await context.DocumentTypes.FirstAsync();
            var createdDocument = await context.Documents.FindAsync(documentId);
            
            Assert.Equal(originalDescription, updatedType.Description);
            Assert.NotEqual("This will be rolled back due to error", updatedType.Description);
            Assert.Null(createdDocument);
        }
    }
}