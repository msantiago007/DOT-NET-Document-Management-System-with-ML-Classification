// DocumentManagementDbContext.cs
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
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.FirstName).HasMaxLength(50);
                entity.Property(e => e.LastName).HasMaxLength(50);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
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
                      .WithMany()
                      .HasForeignKey(d => d.UploadedById)
                      .OnDelete(DeleteBehavior.Restrict);
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
        }
    }
}