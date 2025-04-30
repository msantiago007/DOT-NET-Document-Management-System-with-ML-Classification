// -----------------------------------------------------------------------------
// <copyright file="TestDbContextFixture.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 29, 2025
// Last Modified:      April 29, 2025
// Version:            0.9.0
// Description:        Test fixture for creating DbContext instances for testing
// -----------------------------------------------------------------------------

using DocumentManagementML.Domain.Entities;
using DocumentManagementML.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace DocumentManagementML.UnitTests.TestFixtures
{
    /// <summary>
    /// Provides utility methods for creating DbContext instances for testing.
    /// </summary>
    public class TestDbContextFixture : IDisposable
    {
        private readonly string _databaseName;
        private bool _disposed;
        
        /// <summary>
        /// Initializes a new instance of the TestDbContextFixture class.
        /// </summary>
        public TestDbContextFixture()
        {
            _databaseName = Guid.NewGuid().ToString();
            _disposed = false;
        }
        
        /// <summary>
        /// Creates a new DbContext instance with a unique in-memory database.
        /// </summary>
        /// <returns>A new DbContext instance.</returns>
        public DocumentManagementDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<DocumentManagementDbContext>()
                .UseInMemoryDatabase(databaseName: _databaseName)
                .Options;
                
            return new DocumentManagementDbContext(options);
        }
        
        /// <summary>
        /// Creates a new DbContext instance with sample test data.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a new DbContext instance with sample data.</returns>
        public async Task<DocumentManagementDbContext> CreateContextWithSampleDataAsync()
        {
            var context = CreateContext();
            
            // Create document types
            var invoiceType = new DocumentType
            {
                DocumentTypeId = Guid.NewGuid(),
                Name = "Invoice",
                TypeName = "invoice",
                Description = "Invoice documents",
                IsActive = true
            };
            
            var receiptType = new DocumentType
            {
                DocumentTypeId = Guid.NewGuid(),
                Name = "Receipt",
                TypeName = "receipt",
                Description = "Receipt documents",
                IsActive = true
            };
            
            var contractType = new DocumentType
            {
                DocumentTypeId = Guid.NewGuid(),
                Name = "Contract",
                TypeName = "contract",
                Description = "Contract documents",
                IsActive = false // Inactive
            };
            
            await context.DocumentTypes.AddRangeAsync(new[] { invoiceType, receiptType, contractType });
            
            // Create users
            var user1 = new User
            {
                UserId = Guid.NewGuid(),
                Username = "john.doe",
                Email = "john.doe@example.com",
                IsActive = true
            };
            
            var user2 = new User
            {
                UserId = Guid.NewGuid(),
                Username = "jane.smith",
                Email = "jane.smith@example.com",
                IsActive = true
            };
            
            await context.Users.AddRangeAsync(new[] { user1, user2 });
            
            // Create documents
            var document1 = new Document
            {
                DocumentId = Guid.NewGuid(),
                DocumentName = "Invoice-2025-001",
                DocumentTypeId = invoiceType.DocumentTypeId,
                UploadedById = user1.UserId,
                FileType = "pdf",
                FileLocation = "/storage/invoices/invoice-2025-001.pdf",
                FileSizeBytes = 1024,
                CreatedDate = DateTime.UtcNow.AddDays(-5),
                IsDeleted = false
            };
            
            var document2 = new Document
            {
                DocumentId = Guid.NewGuid(),
                DocumentName = "Receipt-2025-001",
                DocumentTypeId = receiptType.DocumentTypeId,
                UploadedById = user2.UserId,
                FileType = "pdf",
                FileLocation = "/storage/receipts/receipt-2025-001.pdf",
                FileSizeBytes = 512,
                CreatedDate = DateTime.UtcNow.AddDays(-3),
                IsDeleted = false
            };
            
            await context.Documents.AddRangeAsync(new[] { document1, document2 });
            
            // Create metadata
            var metadata1 = new DocumentMetadata
            {
                Id = Guid.NewGuid(),
                DocumentId = document1.DocumentId,
                MetadataKey = "InvoiceNumber",
                MetadataValue = "INV-2025-001"
            };
            
            var metadata2 = new DocumentMetadata
            {
                Id = Guid.NewGuid(),
                DocumentId = document1.DocumentId,
                MetadataKey = "Amount",
                MetadataValue = "1250.00"
            };
            
            await context.DocumentMetadata.AddRangeAsync(new[] { metadata1, metadata2 });
            
            await context.SaveChangesAsync();
            
            return context;
        }
        
        /// <summary>
        /// Disposes the test fixture.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        /// <summary>
        /// Disposes the test fixture.
        /// </summary>
        /// <param name="disposing">Whether the method is being called by the Dispose method.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Cleanup code for in-memory database if needed
                }
                
                _disposed = true;
            }
        }
    }
}