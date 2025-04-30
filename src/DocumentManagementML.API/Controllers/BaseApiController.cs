// -----------------------------------------------------------------------------
// <copyright file="BaseApiController.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 29, 2025
// Last Modified:      April 29, 2025
// Version:            0.9.0
// Description:        Base API controller providing common functionality for all controllers
// -----------------------------------------------------------------------------
using DocumentManagementML.Application.DTOs;
using DocumentManagementML.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace DocumentManagementML.API.Controllers
{
    /// <summary>
    /// Base API controller providing common functionality for all controllers
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
        protected readonly ILogger Logger;

        /// <summary>
        /// Initializes a new instance of the BaseApiController class
        /// </summary>
        /// <param name="logger">Logger instance</param>
        protected BaseApiController(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Creates an AcceptedAtAction result with standardized response
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        /// <param name="message">Success message</param>
        /// <returns>AcceptedAtAction result with standardized response</returns>
        protected AcceptedAtActionResult AcceptedAtAction(string actionName, string controllerName, object routeValues, string message)
        {
            return base.AcceptedAtAction(
                actionName, 
                controllerName, 
                routeValues, 
                ResponseDto.Ok(message));
        }

        /// <summary>
        /// Executes an API operation with standardized error handling
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="operation">The operation to execute</param>
        /// <param name="errorMessage">Error message for logging</param>
        /// <returns>Action result with standardized response</returns>
        protected async Task<IActionResult> ExecuteAsync<T>(
            Func<Task<T>> operation,
            string errorMessage)
        {
            try
            {
                var result = await operation();
                
                if (result == null)
                {
                    return NotFound(ResponseDto.Fail("Resource not found"));
                }
                
                return Ok(ResponseDto<T>.Ok(result));
            }
            catch (ValidationException ex)
            {
                Logger.LogWarning(ex, errorMessage);
                var problemDetails = new ValidationProblemDetails(ex.Errors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    Title = "Validation error",
                    Detail = ex.Message,
                    Instance = HttpContext.Request.Path
                };
                return BadRequest(problemDetails);
            }
            catch (NotFoundException ex)
            {
                Logger.LogWarning(ex, errorMessage);
                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                    Title = "Resource not found",
                    Detail = ex.Message,
                    Instance = HttpContext.Request.Path
                };
                return NotFound(problemDetails);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, errorMessage);
                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    Title = "An unexpected error occurred",
                    Detail = "An error occurred while processing your request. Please try again later.",
                    Instance = HttpContext.Request.Path
                };
                return StatusCode(500, problemDetails);
            }
        }
        
        /// <summary>
        /// Executes an API operation that returns a boolean result with standardized error handling
        /// </summary>
        /// <param name="operation">The operation to execute</param>
        /// <param name="errorMessage">Error message for logging</param>
        /// <param name="successMessage">Message to return on success</param>
        /// <returns>Action result with standardized response</returns>
        protected async Task<IActionResult> ExecuteBooleanAsync(
            Func<Task<bool>> operation,
            string errorMessage,
            string successMessage = "Operation completed successfully")
        {
            try
            {
                var result = await operation();
                
                if (!result)
                {
                    return NotFound(ResponseDto.Fail("Resource not found"));
                }
                
                return Ok(ResponseDto.Ok(successMessage));
            }
            catch (ValidationException ex)
            {
                Logger.LogWarning(ex, errorMessage);
                var problemDetails = new ValidationProblemDetails(ex.Errors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    Title = "Validation error",
                    Detail = ex.Message,
                    Instance = HttpContext.Request.Path
                };
                return BadRequest(problemDetails);
            }
            catch (NotFoundException ex)
            {
                Logger.LogWarning(ex, errorMessage);
                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                    Title = "Resource not found",
                    Detail = ex.Message,
                    Instance = HttpContext.Request.Path
                };
                return NotFound(problemDetails);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, errorMessage);
                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    Title = "An unexpected error occurred",
                    Detail = "An error occurred while processing your request. Please try again later.",
                    Instance = HttpContext.Request.Path
                };
                return StatusCode(500, problemDetails);
            }
        }
        
        /// <summary>
        /// Executes an API operation with no return value with standardized error handling
        /// </summary>
        /// <param name="operation">The operation to execute</param>
        /// <param name="errorMessage">Error message for logging</param>
        /// <param name="successMessage">Message to return on success</param>
        /// <returns>Action result with standardized response</returns>
        protected async Task<IActionResult> ExecuteVoidAsync(
            Func<Task> operation,
            string errorMessage,
            string successMessage = "Operation completed successfully")
        {
            try
            {
                await operation();
                return Ok(ResponseDto.Ok(successMessage));
            }
            catch (ValidationException ex)
            {
                Logger.LogWarning(ex, errorMessage);
                var problemDetails = new ValidationProblemDetails(ex.Errors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    Title = "Validation error",
                    Detail = ex.Message,
                    Instance = HttpContext.Request.Path
                };
                return BadRequest(problemDetails);
            }
            catch (NotFoundException ex)
            {
                Logger.LogWarning(ex, errorMessage);
                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                    Title = "Resource not found",
                    Detail = ex.Message,
                    Instance = HttpContext.Request.Path
                };
                return NotFound(problemDetails);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, errorMessage);
                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    Title = "An unexpected error occurred",
                    Detail = "An error occurred while processing your request. Please try again later.",
                    Instance = HttpContext.Request.Path
                };
                return StatusCode(500, problemDetails);
            }
        }
    }
}