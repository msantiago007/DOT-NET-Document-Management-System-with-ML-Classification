// ServiceCollectionExtensions.cs
using DocumentManagementML.Application.Interfaces;
using DocumentManagementML.Application.Mapping;
using DocumentManagementML.Application.Services;
using DocumentManagementML.Domain.Repositories;
using DocumentManagementML.Domain.Services;
using DocumentManagementML.Infrastructure.Data;
using DocumentManagementML.Infrastructure.ML;
using DocumentManagementML.Infrastructure.Repositories;
using DocumentManagementML.Infrastructure.Settings;
using DocumentManagementML.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
// Microsoft.EntityFrameworkCore.InMemory is imported through the package reference
// in the DocumentManagementML.API.csproj file
using System;

namespace DocumentManagementML.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register AutoMapper
            services.AddAutoMapper(typeof(MappingProfile).Assembly);

            // Register application services
            // Phase 2: Using enhanced services with Unit of Work pattern
            services.AddScoped<IDocumentService, EnhancedDocumentService>();
            services.AddScoped<IDocumentTypeService, EnhancedDocumentTypeService>();
            services.AddScoped<IUserService, EnhancedUserService>();
            // Use simple implementation for phase 1
            services.AddScoped<IDocumentClassificationService, SimpleDocumentClassificationService>();
            // Add password hasher
            services.AddSingleton<DocumentManagementML.Application.Interfaces.IPasswordHasher, DocumentManagementML.Infrastructure.Services.SimplePasswordHasher>();

            return services;
        }

        public static IServiceCollection AddMLServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure ML settings
            services.Configure<MLSettings>(configuration.GetSection("ML"));

            // Register enhanced text extraction services (Phase 4)
            services.AddSingleton<PdfTextExtractor>();
            services.AddSingleton<OfficeTextExtractor>();
            services.AddSingleton<OcrTextExtractor>();
            services.AddSingleton<TextPreprocessingPipeline>();
            
            // Register enhanced text extractor as the main implementation
            services.AddSingleton<ITextExtractor, EnhancedTextExtractor>();
            
            // Keep simple extractor available for fallback
            services.AddSingleton<SimpleTextExtractor>();
            
            // Document classification model registration
            services.AddSingleton<IDocumentClassificationModel, DocumentClassificationModel>();

            return services;
        }

        public static IServiceCollection AddStorageServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure storage settings
            services.Configure<StorageSettings>(configuration.GetSection("Storage"));

            // Register storage services with versioning support for Phase 2
            services.AddSingleton<DocumentManagementML.Domain.Services.IFileStorageService, VersionedFileStorageService>();
            services.AddSingleton<DocumentManagementML.Domain.Services.IVersionedFileStorageService, VersionedFileStorageService>();
            services.AddSingleton<DocumentManagementML.Application.Interfaces.IFileStorageService, VersionedApplicationFileStorageService>();
            services.AddSingleton<DocumentManagementML.Application.Interfaces.IVersionedFileStorageService, VersionedApplicationFileStorageService>();

            return services;
        }

        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register database context
            var connectionString = configuration.GetConnectionString("DocumentManagementConnection");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                // Use SQLite database as default (persistent but no server required)
                var sqliteConnectionString = "Data Source=./DocumentManagementML/Data/DocumentManagement.db";
                services.AddDbContext<DocumentManagementDbContext>(options =>
                    options.UseSqlite(sqliteConnectionString, 
                        b => b.MigrationsAssembly("DocumentManagementML.Infrastructure")));
                    
                Console.WriteLine("Using SQLite database for development: ./DocumentManagementML/Data/DocumentManagement.db");
            }
            else if (connectionString.Contains("Data Source=") || connectionString.EndsWith(".db"))
            {
                // Use SQLite with custom connection string
                services.AddDbContext<DocumentManagementDbContext>(options =>
                    options.UseSqlite(connectionString,
                        b => b.MigrationsAssembly("DocumentManagementML.Infrastructure")));
                        
                Console.WriteLine("Using SQLite database with connection string");
            }
            else
            {
                // Use SQL Server with the provided connection string
                services.AddDbContext<DocumentManagementDbContext>(options =>
                    options.UseSqlServer(
                        connectionString,
                        b => b.MigrationsAssembly("DocumentManagementML.Infrastructure")));
                        
                Console.WriteLine("Using SQL Server database with connection string");
            }

            // Register repositories
            services.AddScoped<IDocumentRepository, DocumentRepository>();
            services.AddScoped<IDocumentTypeRepository, DocumentTypeRepository>();
            services.AddScoped<IDocumentMetadataRepository, DocumentMetadataRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            
            // Register Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUnitOfWorkExtended, UnitOfWork>();

            return services;
        }
    }
}