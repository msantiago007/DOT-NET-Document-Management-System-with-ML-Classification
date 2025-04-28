// DocumentTypeRepository.cs
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