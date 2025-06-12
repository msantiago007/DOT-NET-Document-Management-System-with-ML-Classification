// -----------------------------------------------------------------------------
// <copyright file="AuthController.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 30, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        Controller for authentication operations
// -----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DocumentManagementML.API.Auth;
using DocumentManagementML.Application.DTOs;
using DocumentManagementML.Application.Interfaces;
using DocumentManagementML.Domain.Entities;
using DocumentManagementML.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DocumentManagementML.API.Controllers
{
    /// <summary>
    /// Controller for authentication operations
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuthController : BaseApiController
    {
        private readonly IUserService _userService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        
        /// <summary>
        /// Initializes a new instance of the AuthController class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="userService">User service</param>
        /// <param name="jwtTokenService">JWT token service</param>
        /// <param name="refreshTokenRepository">Refresh token repository</param>
        /// <param name="unitOfWork">Unit of work</param>
        public AuthController(
            ILogger<AuthController> logger,
            IUserService userService,
            IJwtTokenService jwtTokenService,
            IRefreshTokenRepository refreshTokenRepository,
            IUnitOfWork unitOfWork)
            : base(logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
            _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }
        
        /// <summary>
        /// Registers a new user account
        /// </summary>
        /// <param name="request">Registration details</param>
        /// <returns>Authentication response with token</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            return await ExecuteAsync<AuthResponse>(async () =>
            {
                // Create user
                var userDto = new UserDto
                {
                    Username = request.Username,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    IsActive = true,
                    IsAdmin = false
                };
                
                var createdUser = await _userService.CreateUserAsync(userDto, request.Password);
                
                // Generate tokens
                var accessToken = _jwtTokenService.GenerateJwtToken(createdUser);
                var refreshToken = _jwtTokenService.GenerateRefreshToken();
                var refreshTokenExpiry = _jwtTokenService.GetRefreshTokenExpiryTime();
                
                // Store refresh token
                var newRefreshToken = new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    UserId = createdUser.Id,
                    Token = refreshToken,
                    ExpiresAt = refreshTokenExpiry,
                    CreatedAt = DateTime.UtcNow
                };
                
                await _refreshTokenRepository.AddAsync(newRefreshToken);
                await _unitOfWork.CommitAsync();
                
                // Create response
                var roles = new List<string>();
                if (createdUser.IsAdmin)
                {
                    roles.Add("Admin");
                }
                
                return new AuthResponse
                {
                    UserId = createdUser.Id,
                    Username = createdUser.Username,
                    Email = createdUser.Email,
                    FirstName = createdUser.FirstName ?? string.Empty,
                    LastName = createdUser.LastName ?? string.Empty,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    TokenExpiration = DateTime.UtcNow.AddMinutes(60),
                    Roles = roles
                };
            }, "Error during user registration");
        }
        
        /// <summary>
        /// Authenticates a user and returns a token
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>Authentication response with token</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            return await ExecuteAsync<AuthResponse>(async () =>
            {
                // Validate credentials
                var user = await _userService.ValidateUserAsync(request.UsernameOrEmail, request.Password);
                if (user == null)
                {
                    throw new UnauthorizedAccessException("Invalid username or password");
                }
                
                // Update last login
                user.LastLoginDate = DateTime.UtcNow;
                await _userService.UpdateUserAsync(user.Id, new UserUpdateDto
                {
                    LastLoginDate = DateTime.UtcNow
                });
                
                // Generate tokens
                var accessToken = _jwtTokenService.GenerateJwtToken(user);
                var refreshToken = _jwtTokenService.GenerateRefreshToken();
                var refreshTokenExpiry = _jwtTokenService.GetRefreshTokenExpiryTime();
                
                // Store refresh token
                var newRefreshToken = new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Token = refreshToken,
                    ExpiresAt = refreshTokenExpiry,
                    CreatedAt = DateTime.UtcNow
                };
                
                await _refreshTokenRepository.AddAsync(newRefreshToken);
                await _unitOfWork.CommitAsync();
                
                // Create response
                var roles = new List<string>();
                if (user.IsAdmin)
                {
                    roles.Add("Admin");
                }
                
                return new AuthResponse
                {
                    UserId = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName ?? string.Empty,
                    LastName = user.LastName ?? string.Empty,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    TokenExpiration = DateTime.UtcNow.AddMinutes(60),
                    Roles = roles
                };
            }, "Error during user login");
        }
        
        /// <summary>
        /// Refreshes an access token using a refresh token
        /// </summary>
        /// <param name="request">Refresh token request</param>
        /// <returns>New access token and refresh token</returns>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            return await ExecuteAsync<TokenResponse>(async () =>
            {
                // Validate refresh token
                var storedRefreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);
                if (storedRefreshToken == null)
                {
                    throw new UnauthorizedAccessException("Invalid refresh token");
                }
                
                // Check if token is valid
                if (storedRefreshToken.IsUsed || 
                    storedRefreshToken.IsRevoked || 
                    storedRefreshToken.ExpiresAt < DateTime.UtcNow)
                {
                    await _refreshTokenRepository.RevokeTokenAsync(storedRefreshToken.Id);
                    await _unitOfWork.CommitAsync();
                    throw new UnauthorizedAccessException("Refresh token has expired or been revoked");
                }
                
                // Validate access token
                var principal = _jwtTokenService.GetPrincipalFromToken(request.AccessToken);
                if (principal == null)
                {
                    throw new UnauthorizedAccessException("Invalid access token");
                }
                
                // Check if the access token matches the user associated with the refresh token
                var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var tokenUserId) || 
                    tokenUserId != storedRefreshToken.UserId)
                {
                    throw new UnauthorizedAccessException("Tokens do not match");
                }
                
                // Mark current refresh token as used
                await _refreshTokenRepository.MarkAsUsedAsync(storedRefreshToken.Id);
                
                // Get user
                var user = storedRefreshToken.User;
                if (user == null)
                {
                    throw new UnauthorizedAccessException("User not found");
                }
                
                // Generate new tokens
                var accessToken = _jwtTokenService.GenerateJwtToken(user);
                var refreshToken = _jwtTokenService.GenerateRefreshToken();
                var refreshTokenExpiry = _jwtTokenService.GetRefreshTokenExpiryTime();
                
                // Store new refresh token
                var newRefreshToken = new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Token = refreshToken,
                    ExpiresAt = refreshTokenExpiry,
                    CreatedAt = DateTime.UtcNow
                };
                
                await _refreshTokenRepository.AddAsync(newRefreshToken);
                await _unitOfWork.CommitAsync();
                
                // Create response
                return new TokenResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    TokenExpiration = DateTime.UtcNow.AddMinutes(60)
                };
            }, "Error refreshing token");
        }
        
        /// <summary>
        /// Logs out a user by revoking their refresh tokens
        /// </summary>
        /// <returns>Success message</returns>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout()
        {
            return await ExecuteVoidAsync(async () =>
            {
                // Get user ID from claims
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
                {
                    throw new UnauthorizedAccessException("Invalid user information");
                }
                
                // Revoke all refresh tokens for the user
                await _refreshTokenRepository.RevokeAllUserTokensAsync(userGuid);
                await _unitOfWork.CommitAsync();
            }, "Error during logout", "Successfully logged out");
        }
        
        /// <summary>
        /// Gets information about the current user
        /// </summary>
        /// <returns>User information</returns>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentUser()
        {
            return await ExecuteAsync<UserDto>(async () =>
            {
                // Get user ID from claims
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
                {
                    throw new UnauthorizedAccessException("Invalid user information");
                }
                
                // Get user details
                var user = await _userService.GetUserByIdAsync(userGuid);
                if (user == null)
                {
                    throw new UnauthorizedAccessException("User not found");
                }
                
                return user;
            }, "Error retrieving user information");
        }
    }
}