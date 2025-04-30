// -----------------------------------------------------------------------------
// <copyright file="ValidationExceptionMiddleware.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 30, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        Middleware for handling validation exceptions
// -----------------------------------------------------------------------------
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using DocumentManagementML.Application.Exceptions;
using DocumentManagementML.Application.DTOs;
using DomainNotFoundException = DocumentManagementML.Domain.Exceptions.NotFoundException;
using ApplicationNotFoundException = DocumentManagementML.Application.Exceptions.NotFoundException;

namespace DocumentManagementML.API.Middleware
{
    /// <summary>
    /// Middleware for handling validation exceptions
    /// </summary>
    public class ValidationExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ValidationExceptionMiddleware> _logger;
        
        /// <summary>
        /// Initializes a new instance of the ValidationExceptionMiddleware class
        /// </summary>
        /// <param name="next">Request delegate</param>
        /// <param name="logger">Logger</param>
        public ValidationExceptionMiddleware(RequestDelegate next, ILogger<ValidationExceptionMiddleware> logger)
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
            try
            {
                await _next(context);
            }
            catch (ValidationException ex)
            {
                await HandleValidationExceptionAsync(context, ex);
            }
            catch (ApplicationNotFoundException ex)
            {
                await HandleNotFoundExceptionAsync(context, ex);
            }
            catch (DomainNotFoundException ex)
            {
                await HandleNotFoundExceptionAsync(context, ex);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }
        
        private async Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception)
        {
            _logger.LogWarning("Validation error: {Message}", exception.Message);
            
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            
            var response = new ResponseDto<object>
            {
                Success = false,
                Message = "Validation error",
                Errors = exception.GetAllErrorMessages()
            };
            
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
        
        private async Task HandleNotFoundExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogWarning("Resource not found: {Message}", exception.Message);
            
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            
            var response = new ResponseDto<object>
            {
                Success = false,
                Message = exception.Message
            };
            
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
        
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred");
            
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            
            var response = new ResponseDto<object>
            {
                Success = false,
                Message = "An error occurred while processing your request."
            };
            
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
    }
}