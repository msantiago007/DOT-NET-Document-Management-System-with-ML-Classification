// -----------------------------------------------------------------------------
// <copyright file="IUserService.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 30, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        Service interface for user operations
// -----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentManagementML.Application.DTOs;
using DocumentManagementML.Domain.Entities;

namespace DocumentManagementML.Application.Interfaces
{
    /// <summary>
    /// Service interface for user operations
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Creates a new user with the given DTO and password
        /// </summary>
        /// <param name="userDto">User DTO</param>
        /// <param name="password">User password</param>
        /// <returns>Created user entity</returns>
        Task<User> CreateUserAsync(UserDto userDto, string password);
        
        /// <summary>
        /// Validates user credentials
        /// </summary>
        /// <param name="usernameOrEmail">Username or email</param>
        /// <param name="password">Password</param>
        /// <returns>User entity if validation succeeds, null otherwise</returns>
        Task<User?> ValidateUserAsync(string usernameOrEmail, string password);
        /// <summary>
        /// Gets a user by its identifier
        /// </summary>
        /// <param name="id">User identifier</param>
        /// <returns>User DTO if found, null otherwise</returns>
        Task<UserDto> GetUserByIdAsync(Guid id);
        
        /// <summary>
        /// Gets all users with pagination
        /// </summary>
        /// <param name="skip">Number of users to skip</param>
        /// <param name="limit">Maximum number of users to return</param>
        /// <returns>Paged collection of user DTOs</returns>
        Task<IEnumerable<UserDto>> GetUsersAsync(int skip = 0, int limit = 100);
        
        /// <summary>
        /// Creates a new user
        /// </summary>
        /// <param name="userDto">User creation DTO</param>
        /// <returns>Created user DTO</returns>
        Task<UserDto> CreateUserAsync(UserCreateDto userDto);
        
        /// <summary>
        /// Updates an existing user
        /// </summary>
        /// <param name="id">User identifier</param>
        /// <param name="userDto">User update DTO</param>
        /// <returns>Updated user DTO</returns>
        Task<UserDto> UpdateUserAsync(Guid id, UserUpdateDto userDto);
        
        /// <summary>
        /// Deactivates a user (soft delete)
        /// </summary>
        /// <param name="id">User identifier</param>
        Task DeactivateUserAsync(Guid id);
        
        /// <summary>
        /// Authenticates a user
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <returns>User DTO if authentication succeeds, null otherwise</returns>
        Task<UserDto?> AuthenticateAsync(string username, string password);
        
        /// <summary>
        /// Checks if a user exists
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <returns>True if the user exists, false otherwise</returns>
        Task<bool> UserExistsAsync(Guid userId);
        
        /// <summary>
        /// Gets a user's name
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <returns>User's name if found, null otherwise</returns>
        Task<string?> GetUserNameAsync(Guid userId);
        
        /// <summary>
        /// Gets a user by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>User DTO if found, null otherwise</returns>
        Task<UserDto?> GetUserByUsernameAsync(string username);
        
        /// <summary>
        /// Gets a user by email
        /// </summary>
        /// <param name="email">Email address</param>
        /// <returns>User DTO if found, null otherwise</returns>
        Task<UserDto?> GetUserByEmailAsync(string email);
        
        /// <summary>
        /// Gets the total count of active users
        /// </summary>
        /// <returns>Total count of active users</returns>
        Task<int> GetUserCountAsync();
        
        /// <summary>
        /// Changes a user's password
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="currentPassword">Current password</param>
        /// <param name="newPassword">New password</param>
        /// <returns>True if password was changed successfully, false otherwise</returns>
        Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
    }
}