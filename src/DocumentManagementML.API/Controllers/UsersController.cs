// -----------------------------------------------------------------------------
// <copyright file="UsersController.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 30, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        Controller for user management operations
// -----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using DocumentManagementML.Application.DTOs;
using DocumentManagementML.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DocumentManagementML.API.Extensions;

namespace DocumentManagementML.API.Controllers
{
    /// <summary>
    /// Controller for user management operations
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class UsersController : BaseApiController
    {
        private readonly IUserService _userService;
        
        /// <summary>
        /// Initializes a new instance of the UsersController class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="userService">User service</param>
        public UsersController(
            ILogger<UsersController> logger,
            IUserService userService)
            : base(logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }
        
        /// <summary>
        /// Gets a user by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User details</returns>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            return await ExecuteAsync<UserDto>(async () =>
            {
                var user = await _userService.GetUserByIdAsync(id);
                return user;
            }, $"Error retrieving user with ID {id}");
        }
        
        /// <summary>
        /// Gets all users with pagination
        /// </summary>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Collection of users</returns>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            // Validate and normalize pagination parameters
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 50);
            
            // Calculate skip value
            int skip = (page - 1) * pageSize;
            
            // Get total count for pagination metadata
            var totalCount = await _userService.GetUserCountAsync();
            
            // Get users for the current page
            var users = await _userService.GetUsersAsync(skip, pageSize);
            
            // Add pagination header
            Response.AddPaginationHeader(page, pageSize, totalCount);
            
            return Ok(users);
        }
        
        /// <summary>
        /// Creates a new user
        /// </summary>
        /// <param name="createDto">User creation data</param>
        /// <returns>Created user</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateDto createDto)
        {
            return await ExecuteAsync<UserDto>(async () =>
            {
                var user = await _userService.CreateUserAsync(createDto);
                
                // Create the Location header for the created resource
                // This will be used in the 201 Created response
                return user;
            }, "Error creating user");
        }
        
        /// <summary>
        /// Updates an existing user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="updateDto">User update data</param>
        /// <returns>Updated user</returns>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UserUpdateDto updateDto)
        {
            return await ExecuteAsync<UserDto>(async () =>
            {
                // Only admins can modify other users or change admin status
                if (!User.IsInRole("Admin") && User.GetUserId() != id.ToString())
                {
                    throw new UnauthorizedAccessException("You are not authorized to update this user");
                }
                
                // Regular users can't change admin status
                if (!User.IsInRole("Admin") && updateDto.IsAdmin.HasValue)
                {
                    throw new UnauthorizedAccessException("You are not authorized to change admin status");
                }
                
                var user = await _userService.UpdateUserAsync(id, updateDto);
                return user;
            }, $"Error updating user with ID {id}");
        }
        
        /// <summary>
        /// Deactivates a user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeactivateUser(Guid id)
        {
            return await ExecuteVoidAsync(async () =>
            {
                await _userService.DeactivateUserAsync(id);
            }, $"Error deactivating user with ID {id}", "User deactivated successfully");
        }
        
        /// <summary>
        /// Changes a user's password
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="model">Password change model</param>
        /// <returns>No content if successful</returns>
        [HttpPost("{id}/change-password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordRequest model)
        {
            return await ExecuteVoidAsync(async () =>
            {
                // Only admins or the user themselves can change their password
                if (!User.IsInRole("Admin") && User.GetUserId() != id.ToString())
                {
                    throw new UnauthorizedAccessException("You are not authorized to change this user's password");
                }
                
                var success = await _userService.ChangePasswordAsync(id, model.CurrentPassword, model.NewPassword);
                if (!success)
                {
                    throw new UnauthorizedAccessException("Current password is incorrect");
                }
            }, $"Error changing password for user with ID {id}", "Password changed successfully");
        }
    }
    
    /// <summary>
    /// Model for changing a user's password
    /// </summary>
    public class ChangePasswordRequest
    {
        /// <summary>
        /// Gets or sets the current password
        /// </summary>
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the new password
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$", 
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character")]
        public string NewPassword { get; set; } = string.Empty;
    }
}