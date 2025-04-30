// -----------------------------------------------------------------------------
// <copyright file="ValidationFilter.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 30, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        Action filter for model validation in APIs
// -----------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using DocumentManagementML.Application.DTOs;

namespace DocumentManagementML.API.Filters
{
    /// <summary>
    /// Action filter for automatic model validation in APIs
    /// </summary>
    public class ValidationFilter : IActionFilter
    {
        /// <summary>
        /// Executes before the action
        /// </summary>
        /// <param name="context">Action executing context</param>
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                
                var problemDetails = new ValidationProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    Title = "One or more validation errors occurred.",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = "See the errors property for details."
                };
                
                foreach (var error in errors)
                {
                    problemDetails.Errors.Add(error.Key, error.Value);
                }
                
                // For backward compatibility with ResponseDto pattern
                var responseDto = new ResponseDto<object>
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = errors.SelectMany(e => e.Value).ToList()
                };
                
                context.Result = new BadRequestObjectResult(responseDto);
            }
        }

        /// <summary>
        /// Executes after the action
        /// </summary>
        /// <param name="context">Action executed context</param>
        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Not needed for validation
        }
    }
}