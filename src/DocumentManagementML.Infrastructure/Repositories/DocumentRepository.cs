// DocumentRepository.cs
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
    /// Repository implementation for Document entity
    /// </summary>
    public class DocumentRepository : BaseRepository<Document>, IDocumentRepository
    {
        private new readonly DocumentManagementDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the DocumentRepository class
        /// </summary>
        /// <param name="context">Database context</param>
        public DocumentRepository(DocumentManagementDbContext context) : base(context)
        {
            _dbContext = context;
        }

        /// <summary>
        /// Gets documents by document type
        /// </summary>
        /// <param name="typeId">Document type identifier</param>
        /// <returns>Collection of documents</returns>
        public async Task<IEnumerable<Document>> GetByTypeIdAsync(int typeId)
        {
            return await _dbContext.Documents
                .Where(d => d.DocumentTypeId == typeId && !d.IsDeleted)
                .ToListAsync();
        }

        /// <summary>
        /// Gets active (non-deleted) documents with pagination
        /// </summary>
        /// <param name="skip">Number of documents to skip</param>
        /// <param name="take">Number of documents to take</param>
        /// <returns>Paged collection of active documents</returns>
        public async Task<IEnumerable<Document>> GetActiveDocumentsAsync(int skip, int take)
        {
            return await _dbContext.Documents
                .Where(d => !d.IsDeleted)
                .OrderByDescending(d => d.UploadDate)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        /// <summary>
        /// Gets a document with its metadata
        /// </summary>
        /// <param name="id">Document identifier</param>
        /// <returns>Document with metadata if found, null otherwise</returns>
        public async Task<Document?> GetWithMetadataAsync(Guid id)
        {
            return await _dbContext.Documents
                .Include(d => d.MetadataDictionary)
                .FirstOrDefaultAsync(d => d.DocumentId == id && !d.IsDeleted);
        }

        /// <summary>
        /// Gets a document with its version history
        /// </summary>
        /// <param name="id">Document identifier</param>
        /// <returns>Document with versions if found, null otherwise</returns>
        public async Task<Document?> GetWithVersionsAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        /// <summary>
        /// Soft deletes a document
        /// </summary>
        /// <param name="id">Document identifier</param>
        public async Task SoftDeleteAsync(Guid id)
        {
            var document = await _dbSet.FindAsync(id);
            if (document != null)
            {
                document.IsDeleted = true;
                document.LastModifiedDate = DateTime.UtcNow;
            }
        }

        public async Task<IEnumerable<Document>> GetByDocumentTypeAsync(Guid documentTypeId)
        {
            return await _dbSet
                .Where(d => d.DocumentTypeId == documentTypeId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Document>> GetByUploadedByAsync(Guid userId)
        {
            return await _dbSet
                .Where(d => d.UploadedById == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Document>> SearchAsync(string searchTerm, Guid? documentTypeId = null)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(d => 
                    d.DocumentName.Contains(searchTerm) || 
                    d.Description.Contains(searchTerm));
            }

            if (documentTypeId.HasValue)
            {
                query = query.Where(d => d.DocumentTypeId == documentTypeId.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Document>> GetRecentDocumentsAsync(int count)
        {
            return await _dbContext.Documents
                .Where(d => !d.IsDeleted)
                .OrderByDescending(d => d.UploadDate)
                .Take(count)
                .ToListAsync();
        }
    }
}