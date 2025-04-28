using System.Reflection;
using AutoMapper;
using DocumentManagementML.Application.Mapping;
using DocumentManagementML.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentManagementML.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register AutoMapper
            services.AddAutoMapper(typeof(MappingProfile).Assembly);
            
            // Register application services
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<IDocumentTypeService, DocumentTypeService>();
            services.AddScoped<IDocumentClassificationService, DocumentClassificationService>();
            services.AddScoped<IUserService, UserService>();
            
            return services;
        }
    }
} 