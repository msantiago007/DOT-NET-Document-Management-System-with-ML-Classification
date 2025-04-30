// -----------------------------------------------------------------------------
// <copyright file="RoleBasedAuthorizationMiddleware.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 30, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        Middleware for handling role-based authorization
// -----------------------------------------------------------------------------
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

namespace DocumentManagementML.API.Middleware
{
    /// <summary>
    /// Middleware for handling role-based authorization
    /// </summary>
    public class RoleBasedAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RoleBasedAuthorizationMiddleware> _logger;
        
        /// <summary>
        /// Initializes a new instance of the RoleBasedAuthorizationMiddleware class
        /// </summary>
        /// <param name="next">Request delegate</param>
        /// <param name="logger">Logger</param>
        public RoleBasedAuthorizationMiddleware(RequestDelegate next, ILogger<RoleBasedAuthorizationMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Invokes the middleware
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>Task</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            // Check if the user is authenticated
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var endpoint = context.GetEndpoint();
                
                if (endpoint != null)
                {
                    // Get the authorization attributes from the endpoint
                    var authorizeAttributes = endpoint.Metadata
                        .OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>()
                        .ToList();
                    
                    // Check if there are role requirements
                    var requiredRoles = authorizeAttributes
                        .Where(a => !string.IsNullOrEmpty(a.Roles))
                        .SelectMany(a => a.Roles.Split(","))
                        .Select(r => r.Trim())
                        .ToList();
                    
                    // If there are required roles, check if the user has any of them
                    if (requiredRoles.Any())
                    {
                        bool hasRequiredRole = false;
                        
                        // Get user's roles
                        var userRoles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value);
                        
                        // Check if the user has any of the required roles
                        foreach (var role in requiredRoles)
                        {
                            if (userRoles.Contains(role))
                            {
                                hasRequiredRole = true;
                                break;
                            }
                        }
                        
                        // If the user doesn't have any of the required roles, return forbidden
                        if (!hasRequiredRole)
                        {
                            _logger.LogWarning("User {Username} attempted to access {Path} without required role. Required roles: {Roles}, User roles: {UserRoles}",
                                context.User.Identity?.Name,
                                context.Request.Path,
                                string.Join(", ", requiredRoles),
                                string.Join(", ", userRoles));
                            
                            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                            context.Response.ContentType = "application/problem+json";
                            
                            var problemDetails = new ProblemDetails
                            {
                                Status = StatusCodes.Status403Forbidden,
                                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                                Title = "Forbidden",
                                Detail = "You do not have permission to access this resource",
                                Instance = context.Request.Path
                            };
                            
                            var jsonOptions = new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                                WriteIndented = true
                            };
                            
                            await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, jsonOptions));
                            return;
                        }
                    }
                }
            }
            
            // Continue processing the request
            await _next(context);
        }
    }
}