// -----------------------------------------------------------------------------
// <copyright file="RequestThrottlingExtensions.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 30, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        Extensions for request throttling
// -----------------------------------------------------------------------------
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DocumentManagementML.API.Middleware;

namespace DocumentManagementML.API.Extensions
{
    /// <summary>
    /// Extensions for request throttling
    /// </summary>
    public static class RequestThrottlingExtensions
    {
        /// <summary>
        /// Adds request throttling services to the service collection
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddRequestThrottling(this IServiceCollection services, IConfiguration configuration)
        {
            var section = configuration.GetSection("RequestThrottling");
            
            var settings = new RequestThrottlingSettings
            {
                MaxRequestsPerWindow = section.GetValue<int>("MaxRequestsPerWindow"),
                WindowMinutes = section.GetValue<int>("WindowMinutes")
            };
            
            // Set default values if configuration is missing
            if (settings.MaxRequestsPerWindow <= 0)
            {
                settings.MaxRequestsPerWindow = 100;
            }
            
            if (settings.WindowMinutes <= 0)
            {
                settings.WindowMinutes = 1;
            }
            
            services.AddSingleton(settings);
            services.AddMemoryCache();
            
            return services;
        }
        
        /// <summary>
        /// Adds request throttling middleware to the application
        /// </summary>
        /// <param name="app">Application builder</param>
        /// <returns>Application builder</returns>
        public static IApplicationBuilder UseRequestThrottling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RequestThrottlingMiddleware>();
        }
    }
}