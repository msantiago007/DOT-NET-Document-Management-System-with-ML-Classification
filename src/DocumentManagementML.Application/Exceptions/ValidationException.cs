// -----------------------------------------------------------------------------
// <copyright file="ValidationException.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 29, 2025
// Last Modified:      April 29, 2025
// Version:            0.9.0
// Description:        Custom exception for handling validation errors
// -----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;

namespace DocumentManagementML.Application.Exceptions
{
    /// <summary>
    /// Exception for validation errors
    /// </summary>
    public class ValidationException : Exception
    {
        /// <summary>
        /// Gets the collection of validation errors
        /// </summary>
        public IDictionary<string, string[]> Errors { get; }

        /// <summary>
        /// Initializes a new instance of the ValidationException class
        /// </summary>
        public ValidationException()
            : base("One or more validation failures have occurred.")
        {
            Errors = new Dictionary<string, string[]>();
        }

        /// <summary>
        /// Initializes a new instance of the ValidationException class with specified errors
        /// </summary>
        /// <param name="errors">Dictionary of validation errors</param>
        public ValidationException(IDictionary<string, string[]> errors)
            : this()
        {
            Errors = errors;
        }

        /// <summary>
        /// Initializes a new instance of the ValidationException class with specified errors
        /// </summary>
        /// <param name="errors">Dictionary of validation errors with collections of error messages</param>
        public ValidationException(IDictionary<string, List<string>> errors)
            : this()
        {
            Errors = errors.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToArray());
        }

        /// <summary>
        /// Initializes a new instance of the ValidationException class with a specified property and error
        /// </summary>
        /// <param name="propertyName">Name of the property with the validation error</param>
        /// <param name="errorMessage">Error message</param>
        public ValidationException(string propertyName, string errorMessage)
            : this()
        {
            Errors = new Dictionary<string, string[]>
            {
                { propertyName, new[] { errorMessage } }
            };
        }

        /// <summary>
        /// Initializes a new instance of the ValidationException class with a single error message
        /// </summary>
        /// <param name="errorMessage">Error message</param>
        public ValidationException(string errorMessage)
            : base(errorMessage)
        {
            Errors = new Dictionary<string, string[]>
            {
                { "General", new[] { errorMessage } }
            };
        }

        /// <summary>
        /// Gets a flat list of all error messages
        /// </summary>
        /// <returns>List of all error messages</returns>
        public List<string> GetAllErrorMessages()
        {
            var messages = new List<string>();
            
            foreach (var error in Errors)
            {
                foreach (var message in error.Value)
                {
                    messages.Add($"{error.Key}: {message}");
                }
            }
            
            return messages;
        }

        /// <summary>
        /// Gets all error messages as a single string
        /// </summary>
        /// <param name="separator">Separator between messages</param>
        /// <returns>String containing all error messages</returns>
        public string GetErrorString(string separator = "; ")
        {
            return string.Join(separator, GetAllErrorMessages());
        }
    }
}