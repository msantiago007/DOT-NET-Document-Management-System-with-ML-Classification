// -----------------------------------------------------------------------------
// <copyright file="UserService.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            February 22, 2025
// Last Modified:      April 29, 2025
// Version:            0.9.0
// Description:        Service implementation for user-related operations including
//                     authentication, user management, and profile operations.
// -----------------------------------------------------------------------------
using AutoMapper;
using DocumentManagementML.Application.DTOs;
using DocumentManagementML.Application.Exceptions;
using DocumentManagementML.Application.Interfaces;
using DocumentManagementML.Domain.Entities;
using DocumentManagementML.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
// using System.ComponentModel.DataAnnotations; // Removed to avoid ambiguity with ValidationException
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentManagementML.Application.Services
{
    /// <summary>
    /// Service implementation for user operations
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;
        private readonly IPasswordHasher _passwordHasher;

        /// <summary>
        /// Initializes a new instance of the UserService class
        /// </summary>
        /// <param name="userRepository">User repository</param>
        /// <param name="mapper">AutoMapper instance</param>
        /// <param name="logger">Logger</param>
        /// <param name="passwordHasher">Password hasher</param>
        public UserService(
            IUserRepository userRepository,
            IMapper mapper,
            ILogger<UserService> logger,
            IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
            _passwordHasher = passwordHasher;
        }

        /// <summary>
        /// Gets a user by its identifier
        /// </summary>
        /// <param name="id">User identifier</param>
        /// <returns>User DTO</returns>
        public async Task<UserDto> GetUserByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null || !user.IsActive)
            {
                throw new NotFoundException($"User with ID {id} not found");
            }

            return _mapper.Map<UserDto>(user);
        }

        /// <summary>
        /// Gets all users with pagination
        /// </summary>
        /// <param name="skip">Number of users to skip</param>
        /// <param name="limit">Maximum number of users to return</param>
        /// <returns>Collection of user DTOs</returns>
        public async Task<IEnumerable<UserDto>> GetUsersAsync(int skip = 0, int limit = 100)
        {
            var users = await _userRepository.GetActiveUsersAsync(skip, limit);
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        /// <summary>
        /// Creates a new user
        /// </summary>
        /// <param name="userDto">User creation DTO</param>
        /// <returns>Created user DTO</returns>
        public async Task<UserDto> CreateUserAsync(UserCreateDto userDto)
        {
            try
            {
                // Check if username is already taken
                var existingUsername = await _userRepository.GetByUsernameAsync(userDto.Username);
                if (existingUsername != null)
                {
                    throw new Application.Exceptions.ValidationException("Username is already taken");
                }

                // Check if email is already taken
                var existingEmail = await _userRepository.GetByEmailAsync(userDto.Email);
                if (existingEmail != null)
                {
                    throw new Application.Exceptions.ValidationException("Email is already taken");
                }

                _logger.LogInformation($"Creating user: {userDto.Username}");
                
                // Hash the password
                var passwordHash = _passwordHasher.HashPassword(userDto.Password);
                
                var user = new User
                {
                    Username = userDto.Username,
                    Email = userDto.Email,
                    PasswordHash = passwordHash,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                
                var createdUser = await _userRepository.AddAsync(user);
                
                _logger.LogInformation($"User created with ID: {createdUser.UserId}");
                return _mapper.Map<UserDto>(createdUser);
            }
            catch (Exception ex) when (!(ex is Application.Exceptions.ValidationException))
            {
                _logger.LogError(ex, $"Error creating user: {ex.Message}");
                throw new ApplicationException($"Error creating user: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates an existing user
        /// </summary>
        /// <param name="id">User identifier</param>
        /// <param name="userDto">User update DTO</param>
        /// <returns>Updated user DTO</returns>
        public async Task<UserDto> UpdateUserAsync(Guid id, UserUpdateDto userDto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null || !user.IsActive)
            {
                throw new NotFoundException($"User with ID {id} not found");
            }
            
            try
            {
                // Update username if provided and check for uniqueness
                if (!string.IsNullOrEmpty(userDto.Username) && userDto.Username != user.Username)
                {
                    var existingUser = await _userRepository.GetByUsernameAsync(userDto.Username);
                    if (existingUser != null && existingUser.UserId.ToString() != id.ToString())
                    {
                        throw new Application.Exceptions.ValidationException("Username is already taken");
                    }
                    
                    user.Username = userDto.Username;
                }
                
                // Update email if provided and check for uniqueness
                if (!string.IsNullOrEmpty(userDto.Email) && userDto.Email != user.Email)
                {
                    var existingUser = await _userRepository.GetByEmailAsync(userDto.Email);
                    if (existingUser != null && existingUser.UserId.ToString() != id.ToString())
                    {
                        throw new Application.Exceptions.ValidationException("Email is already taken");
                    }
                    
                    user.Email = userDto.Email;
                }
                
                // Update password if provided
                if (!string.IsNullOrEmpty(userDto.Password))
                {
                    user.PasswordHash = _passwordHasher.HashPassword(userDto.Password);
                }
                
                // Update active status if provided
                if (userDto.IsActive.HasValue)
                {
                    user.IsActive = userDto.IsActive.Value;
                }
                
                var updatedUser = await _userRepository.UpdateAsync(user);
                
                _logger.LogInformation($"User updated: {id}");
                return _mapper.Map<UserDto>(updatedUser);
            }
            catch (Exception ex) when (!(ex is Application.Exceptions.ValidationException))
            {
                _logger.LogError(ex, $"Error updating user: {ex.Message}");
                throw new ApplicationException($"Error updating user: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Deactivates a user
        /// </summary>
        /// <param name="id">User identifier</param>
        public async Task DeactivateUserAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null || !user.IsActive)
            {
                throw new NotFoundException($"User with ID {id} not found");
            }
            
            try
            {
                await _userRepository.DeactivateAsync(id);
                
                _logger.LogInformation($"User deactivated: {id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deactivating user: {ex.Message}");
                throw new ApplicationException($"Error deactivating user: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Authenticates a user
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <returns>User DTO if authentication succeeds, null otherwise</returns>
        public async Task<UserDto?> AuthenticateAsync(string username, string password)
        {
            try
            {
                var user = await _userRepository.GetByUsernameAsync(username);
                if (user == null || !user.IsActive)
                {
                    _logger.LogWarning($"Authentication failed: User {username} not found or inactive");
                    return null;
                }
                
                if (!_passwordHasher.VerifyPassword(password, user.PasswordHash))
                {
                    _logger.LogWarning($"Authentication failed: Invalid password for user {username}");
                    return null;
                }
                
                // Update last login date
                user.LastLoginDate = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);
                
                _logger.LogInformation($"User {username} authenticated successfully");
                return _mapper.Map<UserDto>(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during authentication: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UserExistsAsync(Guid userId)
        {
            try
            {
                return await _userRepository.ExistsAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user with ID {UserId} exists", userId);
                return false;
            }
        }

        public async Task<string?> GetUserNameAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                return user?.Username;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user name for user with ID {UserId}", userId);
                return null;
            }
        }

        /// <summary>
        /// Gets a user by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>User DTO if found, null otherwise</returns>
        public async Task<UserDto?> GetUserByUsernameAsync(string username)
        {
            try
            {
                var user = await _userRepository.GetByUsernameAsync(username);
                if (user == null || !user.IsActive)
                {
                    return null;
                }
                return _mapper.Map<UserDto>(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by username {Username}", username);
                return null;
            }
        }
        
        /// <summary>
        /// Gets a user by email
        /// </summary>
        /// <param name="email">Email address</param>
        /// <returns>User DTO if found, null otherwise</returns>
        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(email);
                if (user == null || !user.IsActive)
                {
                    return null;
                }
                return _mapper.Map<UserDto>(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by email {Email}", email);
                return null;
            }
        }
        
        /// <summary>
        /// Gets the total count of active users
        /// </summary>
        /// <returns>Total count of active users</returns>
        public async Task<int> GetUserCountAsync()
        {
            try
            {
                return await _userRepository.GetActiveUsersCountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user count");
                return 0;
            }
        }
        
        /// <summary>
        /// Changes a user's password
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="currentPassword">Current password</param>
        /// <param name="newPassword">New password</param>
        /// <returns>True if password was changed successfully, false otherwise</returns>
        public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null || !user.IsActive)
                {
                    return false;
                }
                
                // Verify current password
                if (!_passwordHasher.VerifyPassword(currentPassword, user.PasswordHash))
                {
                    return false;
                }
                
                // Update password
                user.PasswordHash = _passwordHasher.HashPassword(newPassword);
                user.LastModifiedDate = DateTime.UtcNow;
                
                await _userRepository.UpdateAsync(user);
                
                _logger.LogInformation("Password changed for user with ID {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user with ID {UserId}", userId);
                return false;
            }
        }
    }

    /// <summary>
    /// Interface for password hashing operations
    /// </summary>
    public interface IPasswordHasher
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string passwordHash);
    }
}