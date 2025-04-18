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
        /// Gets all active document types
        /// </summary>
        /// <returns>Collection of active document types</returns>
        public async Task<IEnumerable<DocumentType>> GetActiveTypesAsync()
        {
            return await _dbContext.DocumentTypes
                .Where(dt => dt.IsActive)
                .OrderBy(dt => dt.TypeName)
                .ToListAsync();
        }

        /// <summary>
        /// Deactivates a document type (soft delete)
        /// </summary>
        /// <param name="id">Document type identifier</param>
        public async Task DeactivateAsync(int id)
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
        public async Task<DocumentType?> GetWithDocumentsAsync(int id)
        {
            return await _dbContext.DocumentTypes
                .Include(dt => dt.Documents.Where(d => !d.IsDeleted))
                .Where(dt => dt.DocumentTypeId == id && dt.IsActive)
                .FirstOrDefaultAsync();
        }

        public async Task<DocumentType?> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(dt => dt.TypeName == name);
        }

        public async Task<IEnumerable<DocumentType>> GetActiveDocumentTypesAsync(int skip, int take)
        {
            return await _dbSet
                .Where(dt => dt.IsActive)
                .OrderBy(dt => dt.TypeName)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }
    }
}