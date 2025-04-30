// -----------------------------------------------------------------------------
// <copyright file="ProblemDetailsExtensions.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 30, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        Extensions for configuring Problem Details
// -----------------------------------------------------------------------------
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DocumentManagementML.API.Extensions
{
    /// <summary>
    /// Extensions for configuring Problem Details
    /// </summary>
    public static class ProblemDetailsExtensions
    {
        /// <summary>
        /// Adds and configures the Problem Details service
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The modified service collection</returns>
        public static IServiceCollection AddProblemDetails(this IServiceCollection services)
        {
            services.AddProblemDetails(options =>
            {
                // Customize problem details
                options.CustomizeProblemDetails = context =>
                {
                    // Add application-specific properties
                    context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
                    context.ProblemDetails.Extensions["serverId"] = Environment.MachineName;
                };
            });
            
            return services;
        }
    }
}