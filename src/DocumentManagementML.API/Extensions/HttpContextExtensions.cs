// -----------------------------------------------------------------------------
// <copyright file="HttpContextExtensions.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 30, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        Extensions for HTTP context and response
// -----------------------------------------------------------------------------
using System;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace DocumentManagementML.API.Extensions
{
    /// <summary>
    /// Extensions for HTTP context and response
    /// </summary>
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Adds pagination header to response
        /// </summary>
        /// <param name="response">HTTP response</param>
        /// <param name="currentPage">Current page number</param>
        /// <param name="itemsPerPage">Items per page</param>
        /// <param name="totalItems">Total items count</param>
        public static void AddPaginationHeader(this HttpResponse response, int currentPage, int itemsPerPage, int totalItems)
        {
            // Calculate total pages
            var totalPages = (int)Math.Ceiling(totalItems / (double)itemsPerPage);
            
            // Create pagination metadata object
            var paginationMetadata = new
            {
                totalCount = totalItems,
                pageSize = itemsPerPage,
                currentPage,
                totalPages,
                hasPrevious = currentPage > 1,
                hasNext = currentPage < totalPages
            };
            
            // Add pagination header
            response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));
            
            // Expose the header to clients
            response.Headers.Add("Access-Control-Expose-Headers", "X-Pagination");
        }
    }
}