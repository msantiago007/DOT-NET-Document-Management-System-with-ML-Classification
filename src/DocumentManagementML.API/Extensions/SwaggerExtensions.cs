// -----------------------------------------------------------------------------
// <copyright file="SwaggerExtensions.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 30, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        Extensions for configuring Swagger documentation
// -----------------------------------------------------------------------------
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;

namespace DocumentManagementML.API.Extensions
{
    /// <summary>
    /// Configures versioned Swagger documentation
    /// </summary>
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;

        /// <summary>
        /// Initializes a new instance of the ConfigureSwaggerOptions class
        /// </summary>
        /// <param name="provider">API version description provider</param>
        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Configures the Swagger generator options
        /// </summary>
        /// <param name="options">Swagger generator options</param>
        public void Configure(SwaggerGenOptions options)
        {
            // Add a swagger document for each discovered API version
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(
                    description.GroupName,
                    CreateInfoForApiVersion(description));
            }
        }

        private static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
        {
            var info = new OpenApiInfo
            {
                Title = "Document Management ML API",
                Version = description.ApiVersion.ToString(),
                Description = "Document management system with ML classification capabilities",
                Contact = new OpenApiContact { Name = "Marco Santiago", Email = "marco@example.com" },
                License = new OpenApiLicense { Name = "Proprietary", Url = new Uri("https://example.com/license") }
            };

            if (description.IsDeprecated)
            {
                info.Description += " This API version has been deprecated.";
            }

            return info;
        }
    }

    /// <summary>
    /// Extensions for configuring Swagger
    /// </summary>
    public static class SwaggerExtensions
    {
        /// <summary>
        /// Configures Swagger for versioned APIs
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection ConfigureVersionedSwagger(this IServiceCollection services)
        {
            // Register the ConfigureSwaggerOptions as a singleton
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

            return services;
        }
    }
}