// ServiceCollectionExtensions.cs
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
            services.AddScoped<IDocumentClassificationService, DocumentClassificationService>();

            return services;
        }

        public static IServiceCollection AddMLServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure ML settings
            services.Configure<MLSettings>(configuration.GetSection("ML"));

            // Register ML services
            services.AddSingleton<ITextExtractor, TextExtractor>();
            services.AddSingleton<IDocumentClassificationModel, DocumentClassificationModel>();

            return services;
        }

        public static IServiceCollection AddStorageServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure storage settings
            services.Configure<StorageSettings>(configuration.GetSection("Storage"));

            // Register storage service
            services.AddSingleton<IFileStorageService, LocalFileStorageService>();

            return services;
        }
    }
}