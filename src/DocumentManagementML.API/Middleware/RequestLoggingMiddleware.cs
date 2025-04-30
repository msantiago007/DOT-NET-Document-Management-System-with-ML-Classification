// -----------------------------------------------------------------------------
// <copyright file="RequestLoggingMiddleware.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 30, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        Middleware for logging HTTP requests
// -----------------------------------------------------------------------------
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DocumentManagementML.API.Middleware
{
    /// <summary>
    /// Middleware for logging HTTP requests
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;
        
        /// <summary>
        /// Initializes a new instance of the RequestLoggingMiddleware class
        /// </summary>
        /// <param name="next">Request delegate</param>
        /// <param name="logger">Logger</param>
        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        
        /// <summary>
        /// Invokes the middleware
        /// </summary>
        /// <param name="context">HTTP context</param>
        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            
            // Log the request
            _logger.LogInformation(
                "Request {Method} {Path} started at {Time}",
                context.Request.Method, 
                context.Request.Path,
                DateTime.UtcNow);
            
            try
            {
                // Continue processing
                await _next(context);
                
                stopwatch.Stop();
                
                // Log successful response
                _logger.LogInformation(
                    "Request {Method} {Path} completed with status code {StatusCode} in {ElapsedMs}ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                // Log exception
                _logger.LogError(
                    ex,
                    "Request {Method} {Path} failed after {ElapsedMs}ms: {ErrorMessage}",
                    context.Request.Method,
                    context.Request.Path,
                    stopwatch.ElapsedMilliseconds,
                    ex.Message);
                
                // Re-throw the exception to be handled by exception middleware
                throw;
            }
        }
    }
}