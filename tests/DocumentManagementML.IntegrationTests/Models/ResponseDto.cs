// -----------------------------------------------------------------------------
// <copyright file="ResponseDto.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            May 1, 2025
// Last Modified:      May 1, 2025
// Version:            0.9.0
// Description:        Response DTO for integration tests
// -----------------------------------------------------------------------------

using System;

namespace DocumentManagementML.IntegrationTests
{
    /// <summary>
    /// Represents a standard API response
    /// </summary>
    public class ResponseDto
    {
        /// <summary>
        /// Gets or sets a value indicating whether the operation was successful
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Gets or sets the response message
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Gets or sets the timestamp of the response
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Creates a successful response
        /// </summary>
        /// <param name="message">The success message</param>
        /// <returns>A successful response</returns>
        public static ResponseDto Ok(string message)
        {
            return new ResponseDto
            {
                Success = true,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
        }
        
        /// <summary>
        /// Creates a failed response
        /// </summary>
        /// <param name="message">The error message</param>
        /// <returns>A failed response</returns>
        public static ResponseDto Fail(string message)
        {
            return new ResponseDto
            {
                Success = false,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
        }
    }
    
    /// <summary>
    /// Represents a standard API response with data
    /// </summary>
    /// <typeparam name="T">The type of the data</typeparam>
    public class ResponseDto<T> : ResponseDto
    {
        /// <summary>
        /// Gets or sets the response data
        /// </summary>
        public T Data { get; set; }
        
        /// <summary>
        /// Creates a successful response with data
        /// </summary>
        /// <param name="data">The response data</param>
        /// <param name="message">The success message</param>
        /// <returns>A successful response with data</returns>
        public static ResponseDto<T> Ok(T data, string message = "Operation completed successfully")
        {
            return new ResponseDto<T>
            {
                Success = true,
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow
            };
        }
        
        /// <summary>
        /// Creates a failed response with data
        /// </summary>
        /// <param name="message">The error message</param>
        /// <returns>A failed response with default data</returns>
        public new static ResponseDto<T> Fail(string message)
        {
            return new ResponseDto<T>
            {
                Success = false,
                Message = message,
                Data = default,
                Timestamp = DateTime.UtcNow
            };
        }
    }
    
    /// <summary>
    /// Represents a user DTO for authentication tests
    /// </summary>
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsActive { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
    }
}