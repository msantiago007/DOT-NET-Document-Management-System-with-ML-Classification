// -----------------------------------------------------------------------------
// <copyright file="ControllerPaginationExtensions.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 30, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        Extensions for adding pagination to controllers
// -----------------------------------------------------------------------------
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DocumentManagementML.API.Extensions
{
    /// <summary>
    /// Extensions for adding pagination to controllers
    /// </summary>
    public static class ControllerPaginationExtensions
    {
        private const int DefaultPageSize = 10;
        private const int MaxPageSize = 50;
        private const string PaginationHeaderName = "X-Pagination";
        
        /// <summary>
        /// Creates a paginated result from a queryable collection
        /// </summary>
        /// <typeparam name="T">Type of items</typeparam>
        /// <param name="controller">Controller instance</param>
        /// <param name="query">Queryable collection</param>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Action result with pagination</returns>
        public static async Task<IActionResult> PaginatedResult<T>(
            this ControllerBase controller,
            IQueryable<T> query,
            int pageNumber = 1, 
            int pageSize = DefaultPageSize)
        {
            // Validate and normalize parameters
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? DefaultPageSize : (pageSize > MaxPageSize ? MaxPageSize : pageSize);
            
            // Get total count
            var totalCount = await query.CountAsync();
            
            // Calculate pagination values
            var totalPages = (int)System.Math.Ceiling(totalCount / (double)pageSize);
            var skip = (pageNumber - 1) * pageSize;
            
            // Get items for the current page
            var items = await query
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
                
            // Create pagination metadata
            var paginationMetadata = new 
            {
                TotalCount = totalCount,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalPages = totalPages,
                HasPrevious = pageNumber > 1,
                HasNext = pageNumber < totalPages
            };
            
            // Add pagination header
            controller.Response.Headers.Add(
                PaginationHeaderName,
                JsonSerializer.Serialize(paginationMetadata));
                
            // Enable CORS for the pagination header
            controller.Response.Headers.Add(
                "Access-Control-Expose-Headers",
                PaginationHeaderName);
                
            // Return paginated result
            return controller.Ok(items);
        }
    }
}