// -----------------------------------------------------------------------------
// <copyright file="NotFoundException.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 30, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        Exception for resource not found errors
// -----------------------------------------------------------------------------
using System;

namespace DocumentManagementML.Domain.Exceptions
{
    /// <summary>
    /// Exception for resource not found errors
    /// </summary>
    public class NotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the NotFoundException class
        /// </summary>
        public NotFoundException()
            : base("The requested resource was not found.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the NotFoundException class with a message
        /// </summary>
        /// <param name="message">Exception message</param>
        public NotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the NotFoundException class with a message and inner exception
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public NotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates a NotFoundException for a resource type and identifier
        /// </summary>
        /// <param name="resourceType">Resource type name</param>
        /// <param name="resourceId">Resource identifier</param>
        /// <returns>A new NotFoundException</returns>
        public static NotFoundException ForResource(string resourceType, object resourceId)
        {
            return new NotFoundException($"{resourceType} with ID {resourceId} was not found.");
        }
    }
}