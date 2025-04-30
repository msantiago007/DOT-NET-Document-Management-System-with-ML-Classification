// -----------------------------------------------------------------------------
// <copyright file="RequestThrottlingMiddleware.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 30, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        Middleware for throttling API requests
// -----------------------------------------------------------------------------
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;

namespace DocumentManagementML.API.Middleware
{
    /// <summary>
    /// Settings for request throttling
    /// </summary>
    public class RequestThrottlingSettings
    {
        /// <summary>
        /// Gets or sets the maximum number of requests allowed per time window
        /// </summary>
        public int MaxRequestsPerWindow { get; set; } = 100;
        
        /// <summary>
        /// Gets or sets the time window in minutes
        /// </summary>
        public int WindowMinutes { get; set; } = 1;
    }
    
    /// <summary>
    /// Middleware for throttling API requests to prevent abuse
    /// </summary>
    public class RequestThrottlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestThrottlingMiddleware> _logger;
        private readonly RequestThrottlingSettings _settings;
        private readonly IMemoryCache _cache;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks;
        
        /// <summary>
        /// Initializes a new instance of the RequestThrottlingMiddleware class
        /// </summary>
        /// <param name="next">Request delegate</param>
        /// <param name="logger">Logger</param>
        /// <param name="settings">Throttling settings</param>
        /// <param name="cache">Memory cache</param>
        public RequestThrottlingMiddleware(
            RequestDelegate next, 
            ILogger<RequestThrottlingMiddleware> logger,
            RequestThrottlingSettings settings,
            IMemoryCache cache)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _locks = new ConcurrentDictionary<string, SemaphoreSlim>();
        }
        
        /// <summary>
        /// Invokes the middleware
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>Task</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            // Get client IP
            var clientIp = GetClientIpAddress(context);
            
            // Create cache key
            var cacheKey = $"RequestThrottling_{clientIp}";
            
            // Get or create semaphore for this client
            var semaphore = _locks.GetOrAdd(clientIp, _ => new SemaphoreSlim(1, 1));
            
            await semaphore.WaitAsync();
            
            try
            {
                // Get or create request counter
                RequestCounter counter = _cache.GetOrCreate(cacheKey, entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_settings.WindowMinutes);
                    return new RequestCounter { Count = 0, LastReset = DateTime.UtcNow };
                }) ?? new RequestCounter { Count = 0, LastReset = DateTime.UtcNow };
                
                // Check if time window has elapsed and reset counter if needed
                if ((DateTime.UtcNow - counter.LastReset).TotalMinutes >= _settings.WindowMinutes)
                {
                    counter.Count = 0;
                    counter.LastReset = DateTime.UtcNow;
                }
                
                // Increment counter
                counter.Count++;
                
                // Update cache
                _cache.Set(cacheKey, counter, TimeSpan.FromMinutes(_settings.WindowMinutes));
                
                // Check if request limit is exceeded
                if (counter.Count > _settings.MaxRequestsPerWindow)
                {
                    _logger.LogWarning("Request throttling limit exceeded for IP {ClientIp}", clientIp);
                    
                    context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                    context.Response.ContentType = "application/problem+json";
                    context.Response.Headers.Add("Retry-After", _settings.WindowMinutes.ToString());
                    
                    var resetTime = counter.LastReset.AddMinutes(_settings.WindowMinutes);
                    var timeUntilReset = resetTime - DateTime.UtcNow;
                    
                    var problemDetails = new ProblemDetails
                    {
                        Status = StatusCodes.Status429TooManyRequests,
                        Type = "https://tools.ietf.org/html/rfc6585#section-4",
                        Title = "Too Many Requests",
                        Detail = $"Request limit of {_settings.MaxRequestsPerWindow} per {_settings.WindowMinutes} minute(s) exceeded. Please try again later.",
                        Instance = context.Request.Path
                    };
                    
                    problemDetails.Extensions["retryAfter"] = Math.Ceiling(timeUntilReset.TotalSeconds);
                    
                    var jsonOptions = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = true
                    };
                    
                    await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, jsonOptions));
                    return;
                }
            }
            finally
            {
                semaphore.Release();
            }
            
            // Continue processing the request
            await _next(context);
        }
        
        private string GetClientIpAddress(HttpContext context)
        {
            // Try to get IP from forwarded headers (for use behind proxies)
            string? ip = context.Request.Headers["X-Forwarded-For"].ToString();
            
            // If no forwarded header, use the remote IP address
            if (string.IsNullOrEmpty(ip))
            {
                ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            }
            else
            {
                // X-Forwarded-For can contain multiple IPs; get the first one
                ip = ip.Split(',')[0].Trim();
            }
            
            return ip;
        }
        
        private class RequestCounter
        {
            public int Count { get; set; }
            public DateTime LastReset { get; set; }
        }
    }
}