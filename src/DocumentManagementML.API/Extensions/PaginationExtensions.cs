// -----------------------------------------------------------------------------
// <copyright file="PaginationExtensions.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 30, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        Extensions for handling pagination in API responses
// -----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DocumentManagementML.API.Extensions
{
    /// <summary>
    /// Represents pagination information
    /// </summary>
    /// <typeparam name="T">Type of the items</typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// Gets or sets the list of items
        /// </summary>
        public List<T> Items { get; set; } = new List<T>();
        
        /// <summary>
        /// Gets or sets the total count of items
        /// </summary>
        public int TotalCount { get; set; }
        
        /// <summary>
        /// Gets or sets the page number
        /// </summary>
        public int PageNumber { get; set; }
        
        /// <summary>
        /// Gets or sets the page size
        /// </summary>
        public int PageSize { get; set; }
        
        /// <summary>
        /// Gets the total pages
        /// </summary>
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        
        /// <summary>
        /// Gets a value indicating whether there is a previous page
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;
        
        /// <summary>
        /// Gets a value indicating whether there is a next page
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;
    }
    
    /// <summary>
    /// Extensions for pagination
    /// </summary>
    public static class PaginationExtensions
    {
        /// <summary>
        /// Converts a query to a paged result
        /// </summary>
        /// <typeparam name="T">Type of the items</typeparam>
        /// <param name="query">Query to paginate</param>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Paged result</returns>
        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
            this IQueryable<T> query,
            int pageNumber,
            int pageSize)
        {
            // Validate parameters
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Max(1, Math.Min(50, pageSize)); // Maximum page size of 50
            
            // Get total count
            var totalCount = await query.CountAsync();
            
            // Get items for current page
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            // Create paged result
            return new PagedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}