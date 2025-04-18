// DocumentMetadataRepository.cs
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
    /// Repository implementation for DocumentMetadata entity
    /// </summary>
    public class DocumentMetadataRepository : BaseRepository<DocumentMetadata>, IDocumentMetadataRepository
    {
        private new readonly DocumentManagementDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the DocumentMetadataRepository class
        /// </summary>
        /// <param name="context">Database context</param>
        public DocumentMetadataRepository(DocumentManagementDbContext context) : base(context)
        {
            _dbContext = context;
        }

        /// <summary>
        /// Gets metadata for a document
        /// </summary>
        /// <param name="documentId">Document identifier</param>
        /// <returns>Collection of metadata entries</returns>
        public async Task<IEnumerable<DocumentMetadata>> GetByDocumentIdAsync(Guid documentId)
        {
            return await _dbContext.DocumentMetadata
                .Where(dm => dm.DocumentId == documentId)
                .ToListAsync();
        }

        /// <summary>
        /// Gets a metadata entry by document and key
        /// </summary>
        /// <param name="key">Metadata key</param>
        /// <returns>Metadata entry if found, null otherwise</returns>
        public async Task<IEnumerable<DocumentMetadata>> GetByKeyAsync(string key)
        {
            return await _dbContext.DocumentMetadata
                .Where(dm => dm.MetadataKey == key)
                .ToListAsync();
        }

        /// <summary>
        /// Updates an existing metadata entry or creates a new one if it doesn't exist
        /// </summary>
        /// <param name="metadata">Metadata entry</param>
        /// <returns>Updated or created metadata entry</returns>
        public async Task<DocumentMetadata> UpsertAsync(DocumentMetadata metadata)
        {
            var existing = await _dbSet
                .Where(dm => dm.DocumentId == metadata.DocumentId && dm.MetadataKey == metadata.MetadataKey)
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                // Update existing metadata
                existing.MetadataValue = metadata.MetadataValue;
                existing.DataType = metadata.DataType;
                existing.LastModifiedDate = DateTime.UtcNow;
                
                await UpdateAsync(existing);
                return existing;
            }
            else
            {
                // Create new metadata
                metadata.CreatedDate = DateTime.UtcNow;
                metadata.LastModifiedDate = DateTime.UtcNow;
                
                await AddAsync(metadata);
                return metadata;
            }
        }

        public async Task<IEnumerable<DocumentMetadata>> SearchByValueAsync(string searchTerm)
        {
            return await _dbSet
                .Where(dm => dm.MetadataValue.Contains(searchTerm))
                .ToListAsync();
        }

        public async Task DeleteByDocumentIdAsync(Guid documentId)
        {
            var metadataItems = await _dbSet
                .Where(dm => dm.DocumentId == documentId)
                .ToListAsync();

            _dbSet.RemoveRange(metadataItems);
            await _dbContext.SaveChangesAsync();
        }
    }
}