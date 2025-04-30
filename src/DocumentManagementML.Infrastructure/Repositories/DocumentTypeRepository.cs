// -----------------------------------------------------------------------------
// <copyright file="DocumentTypeRepository.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            February 22, 2025
// Last Modified:      April 29, 2025
// Version:            0.9.0
// Description:        Repository implementation for DocumentType entity that provides
//                     data access operations for document type management.
// -----------------------------------------------------------------------------
using DocumentManagementML.Domain.Entities;
using DocumentManagementML.Domain.Repositories;
using DocumentManagementML.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DocumentManagementML.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for DocumentType entity
    /// </summary>
    public class DocumentTypeRepository : BaseRepository<DocumentType>, IDocumentTypeRepository
    {
        private new readonly DocumentManagementDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the DocumentTypeRepository class
        /// </summary>
        /// <param name="context">Database context</param>
        public DocumentTypeRepository(DocumentManagementDbContext context) : base(context)
        {
            _dbContext = context;
        }

        /// <summary>
        /// Gets all document types
        /// </summary>
        /// <returns>Collection of document types</returns>
        public new async Task<IEnumerable<DocumentType>> GetAllAsync()
        {
            return await _dbContext.DocumentTypes
                .OrderBy(dt => dt.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Gets a document type by its identifier
        /// </summary>
        /// <param name="id">Document type identifier</param>
        /// <returns>Document type if found, null otherwise</returns>
        public new async Task<DocumentType?> GetByIdAsync(Guid id)
        {
            return await _dbContext.DocumentTypes
                .FirstOrDefaultAsync(dt => dt.DocumentTypeId == id);
        }

        /// <summary>
        /// Adds a new document type
        /// </summary>
        /// <param name="documentType">Document type to add</param>
        /// <returns>Added document type</returns>
        public new async Task<DocumentType> AddAsync(DocumentType documentType)
        {
            // Ensure proper initialization
            if (documentType.CreatedDate == default)
            {
                documentType.CreatedDate = DateTime.UtcNow;
            }
            
            if (documentType.LastModifiedDate == default)
            {
                documentType.LastModifiedDate = DateTime.UtcNow;
            }

            if (string.IsNullOrEmpty(documentType.TypeName))
            {
                documentType.TypeName = documentType.Name.Replace(" ", "").ToLower();
            }

            // Add to database
            await _dbContext.DocumentTypes.AddAsync(documentType);
            await _dbContext.SaveChangesAsync();
            
            return documentType;
        }

        /// <summary>
        /// Updates an existing document type
        /// </summary>
        /// <param name="documentType">Document type to update</param>
        /// <returns>Updated document type</returns>
        public new async Task<DocumentType> UpdateAsync(DocumentType documentType)
        {
            // Update last modified date
            documentType.LastModifiedDate = DateTime.UtcNow;
            
            // Update entity
            _dbContext.DocumentTypes.Update(documentType);
            await _dbContext.SaveChangesAsync();
            
            return documentType;
        }

        /// <summary>
        /// Deletes a document type
        /// </summary>
        /// <param name="id">Document type identifier</param>
        /// <returns>True if the document type was deleted, false otherwise</returns>
        public new async Task<bool> DeleteAsync(Guid id)
        {
            var documentType = await GetByIdAsync(id);
            if (documentType == null)
            {
                return false;
            }

            // Check if there are any documents with this type before deleting
            var hasDocuments = await _dbContext.Documents
                .AnyAsync(d => d.DocumentTypeId == id && !d.IsDeleted);
                
            if (hasDocuments)
            {
                // Can't delete document type with associated documents
                return false;
            }

            _dbContext.DocumentTypes.Remove(documentType);
            await _dbContext.SaveChangesAsync();
            
            return true;
        }

        /// <summary>
        /// Gets all document types with pagination
        /// </summary>
        /// <param name="skip">Number of document types to skip</param>
        /// <param name="take">Number of document types to take</param>
        /// <returns>Paged collection of document types</returns>
        public async Task<IEnumerable<DocumentType>> GetAllDocumentTypesAsync(int skip, int take)
        {
            return await _dbContext.DocumentTypes
                .OrderBy(dt => dt.Name)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        /// <summary>
        /// Gets all active document types with pagination
        /// </summary>
        /// <param name="skip">Number of document types to skip</param>
        /// <param name="take">Number of document types to take</param>
        /// <returns>Paged collection of active document types</returns>
        public async Task<IEnumerable<DocumentType>> GetActiveDocumentTypesAsync(int skip, int take)
        {
            return await _dbContext.DocumentTypes
                .Where(dt => dt.IsActive)
                .OrderBy(dt => dt.Name)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        /// <summary>
        /// Gets all active document types
        /// </summary>
        /// <returns>Collection of active document types</returns>
        public async Task<IEnumerable<DocumentType>> GetActiveTypesAsync()
        {
            return await _dbContext.DocumentTypes
                .Where(dt => dt.IsActive)
                .OrderBy(dt => dt.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Gets a document type by its name
        /// </summary>
        /// <param name="name">Document type name</param>
        /// <returns>Document type if found, null otherwise</returns>
        public async Task<DocumentType?> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(dt => dt.Name == name);
        }

        /// <summary>
        /// Gets the total count of document types
        /// </summary>
        /// <param name="activeOnly">If true, count only active document types</param>
        /// <returns>Total count of document types</returns>
        public async Task<int> GetDocumentTypeCountAsync(bool activeOnly = false)
        {
            var query = _dbContext.DocumentTypes.AsQueryable();
            
            if (activeOnly)
            {
                query = query.Where(dt => dt.IsActive);
            }
            
            return await query.CountAsync();
        }

        /// <summary>
        /// Deactivates a document type (soft delete)
        /// </summary>
        /// <param name="id">Document type identifier</param>
        public async Task DeactivateAsync(Guid id)
        {
            var documentType = await _dbSet.FindAsync(id);
            if (documentType != null)
            {
                documentType.IsActive = false;
                documentType.LastModifiedDate = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Gets a document type by its identifier including related documents
        /// </summary>
        /// <param name="id">Document type identifier</param>
        /// <returns>Document type with documents if found, null otherwise</returns>
        public async Task<DocumentType?> GetWithDocumentsAsync(Guid id)
        {
            return await _dbContext.DocumentTypes
                .Include(dt => dt.Documents.Where(d => !d.IsDeleted))
                .Where(dt => dt.DocumentTypeId == id && dt.IsActive)
                .FirstOrDefaultAsync();
        }
    }
}