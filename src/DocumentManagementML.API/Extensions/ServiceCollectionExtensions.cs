// ServiceCollectionExtensions.cs
using DocumentManagementML.Application.Interfaces;
using DocumentManagementML.Application.Mapping;
using DocumentManagementML.Application.Services;
using DocumentManagementML.Domain.Repositories;
using DocumentManagementML.Domain.Services;
using DocumentManagementML.Infrastructure.Data.Configurations;
using DocumentManagementML.Infrastructure.ML;
using DocumentManagementML.Infrastructure.Repositories;
using DocumentManagementML.Infrastructure.Settings;
using DocumentManagementML.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.InMemory;
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
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<IDocumentTypeService, DocumentTypeService>();
            services.AddScoped<IUserService, UserService>();
            // Use simple implementation for phase 1
            services.AddScoped<IDocumentClassificationService, SimpleDocumentClassificationService>();

            return services;
        }

        public static IServiceCollection AddMLServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure ML settings
            services.Configure<MLSettings>(configuration.GetSection("ML"));

            // Register ML services (simplified for phase 1)
            services.AddSingleton<ITextExtractor, SimpleTextExtractor>();
            // Document classification model registration commented out for phase 1
            // services.AddSingleton<IDocumentClassificationModel, DocumentClassificationModel>();

            return services;
        }

        public static IServiceCollection AddStorageServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure storage settings
            services.Configure<StorageSettings>(configuration.GetSection("Storage"));

            // Register storage services - using simple implementations for phase 1
            services.AddSingleton<DocumentManagementML.Domain.Services.IFileStorageService, SimpleFileStorageService>();
            services.AddSingleton<DocumentManagementML.Application.Interfaces.IFileStorageService, ApplicationFileStorageService>();

            return services;
        }

        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register database context
            var connectionString = configuration.GetConnectionString("DocumentManagementConnection");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                // Use in-memory database if connection string is not provided
                services.AddDbContext<DocumentManagementDbContext>(options =>
                    options.UseInMemoryDatabase("DocumentManagementInMemory"));
                    
                Console.WriteLine("Using in-memory database for development");
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

            return services;
        }
    }
}