// -----------------------------------------------------------------------------
// <copyright file="ResponseDto.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 29, 2025
// Last Modified:      April 29, 2025
// Version:            0.9.0
// Description:        Standardized response DTOs for API consistency
// -----------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace DocumentManagementML.Application.DTOs
{
    /// <summary>
    /// Base response class for API standardization
    /// </summary>
    public class ResponseDto
    {
        /// <summary>
        /// Gets or sets a value indicating whether the operation was successful
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Gets or sets the message associated with the response
        /// </summary>
        public string? Message { get; set; }
        
        /// <summary>
        /// Gets or sets the error details
        /// </summary>
        public List<string>? Errors { get; set; }
        
        /// <summary>
        /// Gets or sets the timestamp of the response
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Creates a successful response
        /// </summary>
        /// <param name="message">Optional success message</param>
        /// <returns>Response DTO</returns>
        public static ResponseDto Ok(string? message = null) => new()
        {
            Success = true,
            Message = message
        };
        
        /// <summary>
        /// Creates a failed response
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="errors">Optional detailed error list</param>
        /// <returns>Response DTO</returns>
        public static ResponseDto Fail(string message, List<string>? errors = null) => new()
        {
            Success = false,
            Message = message,
            Errors = errors
        };
        
        /// <summary>
        /// Creates a failed response from an exception
        /// </summary>
        /// <param name="ex">The exception</param>
        /// <param name="includeExceptionDetails">Whether to include exception details in the response</param>
        /// <returns>Response DTO</returns>
        public static ResponseDto FromException(Exception ex, bool includeExceptionDetails = false)
        {
            var errors = includeExceptionDetails 
                ? new List<string> { ex.Message, ex.StackTrace ?? "No stack trace available" }
                : null;
                
            return new ResponseDto
            {
                Success = false,
                Message = "An error occurred while processing the request",
                Errors = errors
            };
        }
    }
    
    /// <summary>
    /// Generic response class with data payload
    /// </summary>
    /// <typeparam name="T">Data type</typeparam>
    public class ResponseDto<T> : ResponseDto
    {
        /// <summary>
        /// Gets or sets the data payload
        /// </summary>
        public T? Data { get; set; }
        
        /// <summary>
        /// Creates a successful response with data
        /// </summary>
        /// <param name="data">Data payload</param>
        /// <param name="message">Optional success message</param>
        /// <returns>Response DTO with data</returns>
        public static ResponseDto<T> Ok(T data, string? message = null) => new()
        {
            Success = true,
            Message = message,
            Data = data
        };
        
        /// <summary>
        /// Creates a failed response
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="errors">Optional detailed error list</param>
        /// <returns>Response DTO</returns>
        public static new ResponseDto<T> Fail(string message, List<string>? errors = null) => new()
        {
            Success = false,
            Message = message,
            Errors = errors
        };
        
        /// <summary>
        /// Creates a failed response from an exception
        /// </summary>
        /// <param name="ex">The exception</param>
        /// <param name="includeExceptionDetails">Whether to include exception details in the response</param>
        /// <returns>Response DTO</returns>
        public static new ResponseDto<T> FromException(Exception ex, bool includeExceptionDetails = false)
        {
            var errors = includeExceptionDetails 
                ? new List<string> { ex.Message, ex.StackTrace ?? "No stack trace available" }
                : null;
                
            return new ResponseDto<T>
            {
                Success = false,
                Message = "An error occurred while processing the request",
                Errors = errors
            };
        }
    }
    
    /// <summary>
    /// Response class for paged data
    /// </summary>
    /// <typeparam name="T">Data item type</typeparam>
    public class PagedResponseDto<T> : ResponseDto<IEnumerable<T>>
    {
        /// <summary>
        /// Gets or sets the current page number
        /// </summary>
        public int Page { get; set; }
        
        /// <summary>
        /// Gets or sets the page size
        /// </summary>
        public int PageSize { get; set; }
        
        /// <summary>
        /// Gets or sets the total item count
        /// </summary>
        public int TotalCount { get; set; }
        
        /// <summary>
        /// Gets or sets the total page count
        /// </summary>
        public int TotalPages { get; set; }
        
        /// <summary>
        /// Gets a value indicating whether there is a previous page
        /// </summary>
        public bool HasPrevious => Page > 1;
        
        /// <summary>
        /// Gets a value indicating whether there is a next page
        /// </summary>
        public bool HasNext => Page < TotalPages;
        
        /// <summary>
        /// Creates a successful paged response
        /// </summary>
        /// <param name="data">Paged data items</param>
        /// <param name="page">Current page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="totalCount">Total item count</param>
        /// <param name="message">Optional success message</param>
        /// <returns>Paged response DTO</returns>
        public static PagedResponseDto<T> Ok(
            IEnumerable<T> data, 
            int page, 
            int pageSize, 
            int totalCount, 
            string? message = null) => new()
        {
            Success = true,
            Message = message,
            Data = data,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }
}