// -----------------------------------------------------------------------------
// <copyright file="TestHelper.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 30, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        Helper methods for unit testing
// -----------------------------------------------------------------------------

using DocumentManagementML.Domain.Entities;
using DocumentManagementML.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace DocumentManagementML.UnitTests.TestHelpers
{
    /// <summary>
    /// Provides helper methods for unit testing.
    /// </summary>
    public static class TestHelper
    {
        /// <summary>
        /// Creates a new in-memory database context for testing.
        /// </summary>
        /// <returns>A new DbContext instance with a unique in-memory database.</returns>
        public static DocumentManagementDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<DocumentManagementDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
                
            return new DocumentManagementDbContext(options);
        }
        
        /// <summary>
        /// Creates a new test document type.
        /// </summary>
        /// <param name="name">The name of the document type.</param>
        /// <returns>A new document type instance.</returns>
        public static DocumentType CreateDocumentType(string name)
        {
            return new DocumentType
            {
                DocumentTypeId = Guid.NewGuid(),
                Name = name,
                TypeName = name.Replace(" ", "").ToLower(),
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            };
        }
        
        /// <summary>
        /// Creates a new test document.
        /// </summary>
        /// <param name="name">The name of the document.</param>
        /// <param name="documentTypeId">The ID of the document type.</param>
        /// <returns>A new document instance.</returns>
        public static Document CreateDocument(string name, Guid? documentTypeId = null)
        {
            return new Document
            {
                DocumentId = Guid.NewGuid(),
                DocumentName = name,
                DocumentTypeId = documentTypeId,
                FileType = "pdf",
                FileLocation = $"/storage/{name}.pdf",
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                IsDeleted = false
            };
        }
        
        /// <summary>
        /// Creates and populates a test database context.
        /// </summary>
        /// <returns>A task representing the asynchronous operation. The task result contains a populated database context.</returns>
        public static async Task<DocumentManagementDbContext> CreatePopulatedDbContextAsync()
        {
            var context = CreateInMemoryDbContext();
            
            // Add document types
            var invoiceType = CreateDocumentType("Invoice");
            var receiptType = CreateDocumentType("Receipt");
            
            context.DocumentTypes.Add(invoiceType);
            context.DocumentTypes.Add(receiptType);
            
            // Add documents
            var invoice1 = CreateDocument("Invoice-2025-001", invoiceType.DocumentTypeId);
            var invoice2 = CreateDocument("Invoice-2025-002", invoiceType.DocumentTypeId);
            var receipt = CreateDocument("Receipt-2025-001", receiptType.DocumentTypeId);
            
            context.Documents.Add(invoice1);
            context.Documents.Add(invoice2);
            context.Documents.Add(receipt);
            
            await context.SaveChangesAsync();
            
            return context;
        }
    }
}