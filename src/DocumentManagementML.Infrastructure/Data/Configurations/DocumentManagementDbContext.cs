// -----------------------------------------------------------------------------
// <copyright file="DocumentManagementDbContext.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            February 22, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        Entity Framework Core database context for Document Management
// -----------------------------------------------------------------------------
using DocumentManagementML.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace DocumentManagementML.Infrastructure.Data
{
    /// <summary>
    /// Entity Framework Core database context for the Document Management system
    /// </summary>
    public class DocumentManagementDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the DocumentManagementDbContext
        /// </summary>
        /// <param name="options">The database context options</param>
        public DocumentManagementDbContext(DbContextOptions<DocumentManagementDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the users DbSet
        /// </summary>
        public DbSet<User> Users { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the document types DbSet
        /// </summary>
        public DbSet<DocumentType> DocumentTypes { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the documents DbSet
        /// </summary>
        public DbSet<Document> Documents { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the document versions DbSet
        /// </summary>
        public DbSet<DocumentVersion> DocumentVersions { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the document metadata DbSet
        /// </summary>
        public DbSet<DocumentMetadata> DocumentMetadata { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the tags DbSet
        /// </summary>
        public DbSet<Tag> Tags { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the topics DbSet
        /// </summary>
        public DbSet<Topic> Topics { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the refresh tokens DbSet
        /// </summary>
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

        /// <summary>
        /// Configures the model and entity mappings
        /// </summary>
        /// <param name="modelBuilder">The model builder</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure schema
            modelBuilder.HasDefaultSchema("dbo");

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.FirstName).HasMaxLength(50);
                entity.Property(e => e.LastName).HasMaxLength(50);
                entity.Property(e => e.IsAdmin).HasDefaultValue(false);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                
                // Ignore properties marked with [NotMapped]
                entity.Ignore(u => u.CreatedDocuments);
                entity.Ignore(u => u.ModifiedDocuments);
                entity.Ignore(u => u.CreatedVersions);
                entity.Ignore(u => u.CreatedRelationships);
                
                // Configure the UploadedDocuments navigation
                entity.HasMany(u => u.UploadedDocuments)
                      .WithOne(d => d.UploadedBy)
                      .HasForeignKey(d => d.UploadedById)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure DocumentType entity
            modelBuilder.Entity<DocumentType>(entity =>
            {
                entity.ToTable("DocumentTypes");
                entity.HasKey(e => e.DocumentTypeId);
                entity.Property(e => e.TypeName).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.TypeName).IsUnique();
            });

            // Configure Document entity
            modelBuilder.Entity<Document>(entity =>
            {
                entity.ToTable("Documents");
                entity.HasKey(e => e.DocumentId);
                entity.Property(e => e.DocumentName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FilePath).IsRequired();
                entity.Property(e => e.FileSize).IsRequired();
                entity.Property(e => e.ContentType).HasMaxLength(100);
                entity.Property(e => e.UploadDate).IsRequired();
                entity.Property(e => e.LastModifiedDate).IsRequired();
                
                entity.HasOne(d => d.DocumentType)
                      .WithMany(t => t.Documents)
                      .HasForeignKey(d => d.DocumentTypeId)
                      .OnDelete(DeleteBehavior.SetNull);
                
                entity.HasOne(d => d.UploadedBy)
                      .WithMany(u => u.UploadedDocuments)
                      .HasForeignKey(d => d.UploadedById)
                      .OnDelete(DeleteBehavior.Restrict);
                
                // Note: CreatedBy and ModifiedBy navigation properties are marked with [NotMapped]
                // in the entity class to avoid circular references
                // Set up explicit foreign key relationships without navigation collections
                
                // Add foreign key relationships for CreatedById and LastModifiedById
                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(d => d.CreatedById)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(d => d.LastModifiedById)
                      .OnDelete(DeleteBehavior.Restrict);
                
                // Explicitly ignore navigation properties that are marked with [NotMapped]
                // EF Core will skip these during model building
                entity.Ignore(d => d.CreatedBy);
                entity.Ignore(d => d.ModifiedBy);
                entity.Ignore(d => d.SourceRelationships);
                entity.Ignore(d => d.TargetRelationships);
            });

            // Configure DocumentVersion entity
            modelBuilder.Entity<DocumentVersion>(entity =>
            {
                entity.ToTable("DocumentVersions");
                entity.HasKey(e => e.VersionId);
                entity.Property(e => e.FileLocation).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.ContentHash).IsRequired().HasMaxLength(255);
                
                entity.HasOne(v => v.Document)
                      .WithMany(d => d.Versions)
                      .HasForeignKey(v => v.DocumentId);
                
                entity.HasOne(v => v.CreatedBy)
                      .WithMany()
                      .HasForeignKey(v => v.CreatedById);
                
                entity.HasIndex(e => new { e.DocumentId, e.VersionNumber }).IsUnique();
            });

            // Configure DocumentMetadata entity
            modelBuilder.Entity<DocumentMetadata>(entity =>
            {
                entity.ToTable("DocumentMetadata");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MetadataKey).IsRequired().HasMaxLength(100);
                entity.Property(e => e.MetadataValue).IsRequired();
                
                entity.HasOne<Document>()
                      .WithMany(d => d.MetadataItems)
                      .HasForeignKey(m => m.DocumentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Tag entity
            modelBuilder.Entity<Tag>(entity =>
            {
                entity.ToTable("Tags");
                entity.HasKey(e => e.TagId);
                entity.Property(e => e.TagName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.HasIndex(e => e.TagName).IsUnique();
            });

            // Configure Topic entity
            modelBuilder.Entity<Topic>(entity =>
            {
                entity.ToTable("Topics");
                entity.HasKey(e => e.TopicId);
                entity.Property(e => e.TopicName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                
                entity.HasOne(t => t.ParentTopic)
                      .WithMany(t => t.ChildTopics)
                      .HasForeignKey(t => t.ParentTopicId);
                
                entity.HasIndex(e => e.TopicName).IsUnique();
            });
            
            // Configure RefreshToken entity
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("RefreshTokens");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Token).IsRequired().HasMaxLength(255);
                entity.Property(e => e.JwtId).HasMaxLength(255);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.ExpiresAt).IsRequired();
                
                entity.HasOne(r => r.User)
                      .WithMany(u => u.RefreshTokens)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(e => e.Token).IsUnique();
                entity.HasIndex(e => e.UserId);
            });
            
            // Configure DocumentRelationship entity
            modelBuilder.Entity<DocumentRelationship>(entity =>
            {
                entity.ToTable("DocumentRelationships");
                entity.HasKey(e => e.RelationshipId);
                entity.Property(e => e.RelationshipType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CreatedDate).IsRequired();
                
                // Configure the relationships with the Document entity
                // Use HasOne with WithMany to explicitly define the relationship
                // since the navigation properties in Document are [NotMapped]
                entity.HasOne(r => r.SourceDocument)
                      .WithMany()
                      .HasForeignKey(r => r.SourceDocumentId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(r => r.TargetDocument)
                      .WithMany()
                      .HasForeignKey(r => r.TargetDocumentId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(r => r.CreatedBy)
                      .WithMany()
                      .HasForeignKey(r => r.CreatedById)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasIndex(e => new { e.SourceDocumentId, e.TargetDocumentId, e.RelationshipType }).IsUnique();
            });
        }
    }
}