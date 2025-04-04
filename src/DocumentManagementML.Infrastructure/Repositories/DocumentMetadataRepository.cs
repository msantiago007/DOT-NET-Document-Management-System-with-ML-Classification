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
        /// <summary>
        /// Initializes a new instance of the DocumentMetadataRepository class
        /// </summary>
        /// <param name="context">Database context</param>
        public DocumentMetadataRepository(DocumentManagementDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Gets metadata for a document
        /// </summary>
        /// <param name="documentId">Document identifier</param>
        /// <returns>Collection of metadata entries</returns>
        public async Task<IEnumerable<DocumentMetadata>> GetByDocumentIdAsync(int documentId)
        {
            return await _context.DocumentMetadata
                .Where(m => m.DocumentId == documentId)
                .OrderBy(m => m.MetadataKey)
                .ToListAsync();
        }

        /// <summary>
        /// Gets a metadata entry by document and key
        /// </summary>
        /// <param name="documentId">Document identifier</param>
        /// <param name="key">Metadata key</param>
        /// <returns>Metadata entry if found, null otherwise</returns>
        public async Task<DocumentMetadata?> GetByKeyAsync(int documentId, string key)
        {
            return await _context.DocumentMetadata
                .Where(m => m.DocumentId == documentId && m.MetadataKey == key)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Updates an existing metadata entry or creates a new one if it doesn't exist
        /// </summary>
        /// <param name="metadata">Metadata entry</param>
        /// <returns>Updated or created metadata entry</returns>
        public async Task<DocumentMetadata> UpsertAsync(DocumentMetadata metadata)
        {
            var existing = await GetByKeyAsync(metadata.DocumentId, metadata.MetadataKey);

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
    }
}