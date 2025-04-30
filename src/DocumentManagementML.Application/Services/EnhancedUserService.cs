// -----------------------------------------------------------------------------
// <copyright file="EnhancedUserService.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 30, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        Enhanced service for user management with improved
//                     transaction handling and validation using the Unit of Work pattern.
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
using System.Threading.Tasks;

namespace DocumentManagementML.Application.Services
{
    /// <summary>
    /// Enhanced service for user management with transaction support
    /// </summary>
    public class EnhancedUserService : BaseApplicationService, IUserService
    {
        private readonly ILogger<EnhancedUserService> _logger;
        private readonly IPasswordHasher _passwordHasher;

        /// <summary>
        /// Initializes a new instance of the EnhancedUserService class
        /// </summary>
        /// <param name="unitOfWork">Unit of work for coordinating multiple repositories</param>
        /// <param name="mapper">AutoMapper instance</param>
        /// <param name="logger">Logger</param>
        /// <param name="passwordHasher">Password hasher service</param>
        public EnhancedUserService(
            IUnitOfWorkExtended unitOfWork,
            IMapper mapper,
            ILogger<EnhancedUserService> logger,
            IPasswordHasher passwordHasher)
            : base(unitOfWork, mapper, logger)
        {
            _logger = logger;
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        }

        /// <summary>
        /// Gets a user by its identifier
        /// </summary>
        /// <param name="id">User identifier</param>
        /// <returns>User DTO</returns>
        public async Task<UserDto> GetUserByIdAsync(Guid id)
        {
            try
            {
                var user = await UnitOfWork.UserRepository.GetByIdAsync(id);
                if (user == null || !user.IsActive)
                {
                    throw new NotFoundException($"User with ID {id} not found");
                }

                return Mapper.Map<UserDto>(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with ID {UserId}", id);
                throw;
            }
        }

        /// <summary>
        /// Gets all users with pagination
        /// </summary>
        /// <param name="skip">Number of users to skip</param>
        /// <param name="limit">Maximum number of users to return</param>
        /// <returns>Collection of user DTOs</returns>
        public async Task<IEnumerable<UserDto>> GetUsersAsync(int skip = 0, int limit = 100)
        {
            try
            {
                var users = await UnitOfWork.UserRepository.GetActiveUsersAsync(skip, limit);
                return Mapper.Map<IEnumerable<UserDto>>(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users with pagination (skip: {Skip}, limit: {Limit})", skip, limit);
                throw;
            }
        }

        /// <summary>
        /// Creates a new user
        /// </summary>
        /// <param name="userDto">User creation DTO</param>
        /// <param name="password">Password for the user</param>
        /// <returns>Created user entity</returns>
        public async Task<User> CreateUserAsync(UserDto userDto, string password)
        {
            return await ExecuteInTransactionAsync(async transaction =>
            {
                // Check if username is already taken
                var existingUsername = await UnitOfWork.UserRepository.GetByUsernameAsync(userDto.Username);
                if (existingUsername != null)
                {
                    throw new ValidationException("Username", "Username is already taken");
                }

                // Check if email is already taken
                var existingEmail = await UnitOfWork.UserRepository.GetByEmailAsync(userDto.Email);
                if (existingEmail != null)
                {
                    throw new ValidationException("Email", "Email is already taken");
                }

                _logger.LogInformation("Creating user: {Username}", userDto.Username);
                
                // Hash the password
                var passwordHash = _passwordHasher.HashPassword(password);
                
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = userDto.Username,
                    Email = userDto.Email,
                    PasswordHash = passwordHash,
                    FirstName = userDto.FirstName,
                    LastName = userDto.LastName,
                    IsActive = true,
                    IsAdmin = userDto.IsAdmin,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow
                };
                
                var createdUser = await UnitOfWork.UserRepository.AddAsync(user);
                
                _logger.LogInformation("User created with ID: {UserId}", createdUser.Id);
                return createdUser;
            }, $"Error creating user: {userDto.Username}");
        }

        /// <summary>
        /// Creates a new user
        /// </summary>
        /// <param name="userDto">User creation DTO</param>
        /// <returns>Created user DTO</returns>
        public async Task<UserDto> CreateUserAsync(UserCreateDto userDto)
        {
            return await ExecuteInTransactionAsync(async transaction =>
            {
                // Check if username is already taken
                var existingUsername = await UnitOfWork.UserRepository.GetByUsernameAsync(userDto.Username);
                if (existingUsername != null)
                {
                    throw new ValidationException("Username", "Username is already taken");
                }

                // Check if email is already taken
                var existingEmail = await UnitOfWork.UserRepository.GetByEmailAsync(userDto.Email);
                if (existingEmail != null)
                {
                    throw new ValidationException("Email", "Email is already taken");
                }

                _logger.LogInformation("Creating user: {Username}", userDto.Username);
                
                // Hash the password
                var passwordHash = _passwordHasher.HashPassword(userDto.Password);
                
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = userDto.Username,
                    Email = userDto.Email,
                    PasswordHash = passwordHash,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow
                };
                
                var createdUser = await UnitOfWork.UserRepository.AddAsync(user);
                
                _logger.LogInformation("User created with ID: {UserId}", createdUser.Id);
                return Mapper.Map<UserDto>(createdUser);
            }, $"Error creating user: {userDto.Username}");
        }

        /// <summary>
        /// Updates an existing user
        /// </summary>
        /// <param name="id">User identifier</param>
        /// <param name="userDto">User update DTO</param>
        /// <returns>Updated user DTO</returns>
        public async Task<UserDto> UpdateUserAsync(Guid id, UserUpdateDto userDto)
        {
            return await ExecuteInTransactionAsync(async transaction =>
            {
                var user = await UnitOfWork.UserRepository.GetByIdAsync(id);
                if (user == null || !user.IsActive)
                {
                    throw new NotFoundException($"User with ID {id} not found");
                }
                
                // Update username if provided and check for uniqueness
                if (!string.IsNullOrEmpty(userDto.Username) && userDto.Username != user.Username)
                {
                    var existingUser = await UnitOfWork.UserRepository.GetByUsernameAsync(userDto.Username);
                    if (existingUser != null && existingUser.Id != id)
                    {
                        throw new ValidationException("Username", "Username is already taken");
                    }
                    
                    user.Username = userDto.Username;
                }
                
                // Update email if provided and check for uniqueness
                if (!string.IsNullOrEmpty(userDto.Email) && userDto.Email != user.Email)
                {
                    var existingUser = await UnitOfWork.UserRepository.GetByEmailAsync(userDto.Email);
                    if (existingUser != null && existingUser.Id != id)
                    {
                        throw new ValidationException("Email", "Email is already taken");
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
                
                // Update admin status if provided
                if (userDto.IsAdmin.HasValue)
                {
                    user.IsAdmin = userDto.IsAdmin.Value;
                }
                
                // Update first name if provided
                if (userDto.FirstName != null)
                {
                    user.FirstName = userDto.FirstName;
                }
                
                // Update last name if provided
                if (userDto.LastName != null)
                {
                    user.LastName = userDto.LastName;
                }
                
                // Update last login date if provided
                if (userDto.LastLoginDate.HasValue)
                {
                    user.LastLoginDate = userDto.LastLoginDate;
                }
                
                user.LastModifiedDate = DateTime.UtcNow;
                var updatedUser = await UnitOfWork.UserRepository.UpdateAsync(user);
                
                _logger.LogInformation("User updated: {UserId}", id);
                return Mapper.Map<UserDto>(updatedUser);
            }, $"Error updating user with ID {id}");
        }

        /// <summary>
        /// Deactivates a user
        /// </summary>
        /// <param name="id">User identifier</param>
        public async Task DeactivateUserAsync(Guid id)
        {
            await ExecuteInTransactionAsync(async transaction =>
            {
                var user = await UnitOfWork.UserRepository.GetByIdAsync(id);
                if (user == null || !user.IsActive)
                {
                    throw new NotFoundException($"User with ID {id} not found");
                }
                
                await UnitOfWork.UserRepository.DeactivateAsync(id);
                
                _logger.LogInformation("User deactivated: {UserId}", id);
            }, $"Error deactivating user with ID {id}");
        }

        /// <summary>
        /// Validates a user's credentials
        /// </summary>
        /// <param name="usernameOrEmail">Username or email</param>
        /// <param name="password">Password</param>
        /// <returns>User entity if validation succeeds, null otherwise</returns>
        public async Task<User?> ValidateUserAsync(string usernameOrEmail, string password)
        {
            try
            {
                // Try to get user by username
                var user = await UnitOfWork.UserRepository.GetByUsernameAsync(usernameOrEmail);
                
                // If not found by username, try by email
                if (user == null)
                {
                    user = await UnitOfWork.UserRepository.GetByEmailAsync(usernameOrEmail);
                }
                
                if (user == null || !user.IsActive)
                {
                    _logger.LogWarning("Authentication failed: User {UsernameOrEmail} not found or inactive", usernameOrEmail);
                    return null;
                }
                
                if (!_passwordHasher.VerifyPassword(password, user.PasswordHash))
                {
                    _logger.LogWarning("Authentication failed: Invalid password for user {UsernameOrEmail}", usernameOrEmail);
                    return null;
                }
                
                _logger.LogInformation("User {Username} validated successfully", user.Username);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during validation for user {UsernameOrEmail}", usernameOrEmail);
                return null;
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
                var user = await UnitOfWork.UserRepository.GetByUsernameAsync(username);
                if (user == null || !user.IsActive)
                {
                    _logger.LogWarning("Authentication failed: User {Username} not found or inactive", username);
                    return null;
                }
                
                if (!_passwordHasher.VerifyPassword(password, user.PasswordHash))
                {
                    _logger.LogWarning("Authentication failed: Invalid password for user {Username}", username);
                    return null;
                }
                
                // Use a transaction to update the last login date
                return await ExecuteInTransactionAsync(async transaction =>
                {
                    // Update last login date
                    user.LastLoginDate = DateTime.UtcNow;
                    user.LastModifiedDate = DateTime.UtcNow;
                    await UnitOfWork.UserRepository.UpdateAsync(user);
                    
                    _logger.LogInformation("User {Username} authenticated successfully", username);
                    return Mapper.Map<UserDto>(user);
                }, $"Error updating last login date for user {username}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during authentication for user {Username}", username);
                return null;
            }
        }

        /// <summary>
        /// Checks if a user exists
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <returns>True if the user exists, false otherwise</returns>
        public async Task<bool> UserExistsAsync(Guid userId)
        {
            try
            {
                return await UnitOfWork.UserRepository.ExistsAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user with ID {UserId} exists", userId);
                return false;
            }
        }

        /// <summary>
        /// Gets a user's name
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <returns>User's name if found, null otherwise</returns>
        public async Task<string?> GetUserNameAsync(Guid userId)
        {
            try
            {
                var user = await UnitOfWork.UserRepository.GetByIdAsync(userId);
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
                var user = await UnitOfWork.UserRepository.GetByUsernameAsync(username);
                if (user == null || !user.IsActive)
                {
                    return null;
                }
                return Mapper.Map<UserDto>(user);
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
                var user = await UnitOfWork.UserRepository.GetByEmailAsync(email);
                if (user == null || !user.IsActive)
                {
                    return null;
                }
                return Mapper.Map<UserDto>(user);
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
                return await UnitOfWork.UserRepository.GetActiveUsersCountAsync();
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
                var user = await UnitOfWork.UserRepository.GetByIdAsync(userId);
                if (user == null || !user.IsActive)
                {
                    return false;
                }
                
                // Verify current password
                if (!_passwordHasher.VerifyPassword(currentPassword, user.PasswordHash))
                {
                    return false;
                }
                
                // Use a transaction to update the password
                return await ExecuteInTransactionAsync(async transaction =>
                {
                    // Update password
                    user.PasswordHash = _passwordHasher.HashPassword(newPassword);
                    user.LastModifiedDate = DateTime.UtcNow;
                    
                    await UnitOfWork.UserRepository.UpdateAsync(user);
                    
                    _logger.LogInformation("Password changed for user with ID {UserId}", userId);
                    return true;
                }, $"Error changing password for user with ID {userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user with ID {UserId}", userId);
                return false;
            }
        }
    }
}