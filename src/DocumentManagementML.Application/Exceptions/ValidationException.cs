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
    }
}