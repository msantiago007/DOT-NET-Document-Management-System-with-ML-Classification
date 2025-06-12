// -----------------------------------------------------------------------------
// <copyright file="AuthControllerTests.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            May 1, 2025
// Last Modified:      May 1, 2025
// Version:            0.9.0
// Description:        Integration tests for the Auth Controller
// -----------------------------------------------------------------------------

using DocumentManagementML.IntegrationTests.TestFixtures;
using DocumentManagementML.IntegrationTests.TestHelpers;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace DocumentManagementML.IntegrationTests.Controllers
{
    public class AuthControllerTests : IClassFixture<ApiTestFixture>
    {
        private readonly ApiTestFixture _fixture;

        public AuthControllerTests(ApiTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsSuccessWithToken()
        {
            // Arrange
            var client = _fixture.CreateClient();
            
            // First, log the available routes
            var response = await client.GetAsync("/");
            Console.WriteLine($"Root response: {response.StatusCode}");
            
            // Try accessing Swagger to see if API is running
            var swaggerResponse = await client.GetAsync("/swagger");
            Console.WriteLine($"Swagger response: {swaggerResponse.StatusCode}");
            
            // Try with full API path
            var loginRequest = new
            {
                UsernameOrEmail = "testuser",
                Password = "Test123!"
            };

            // Act - Notice we're using the EnhancedAuth controller route
            var loginResponse = await client.PostAsync("/api/v1/EnhancedAuth/login", 
                TestHelper.CreateJsonContent(loginRequest));
            
            Console.WriteLine($"Login response: {loginResponse.StatusCode}");
            Console.WriteLine($"Login response content: {await loginResponse.Content.ReadAsStringAsync()}");

            // Assert
            loginResponse.EnsureSuccessStatusCode();
            var authResponse = await TestHelper.DeserializeResponseAsync<ApiTestFixture.LoginResponseDto>(loginResponse);
            
            Assert.NotNull(authResponse);
            Assert.Equal("testuser", authResponse.Username);
            Assert.NotEmpty(authResponse.AccessToken);
            Assert.NotEmpty(authResponse.RefreshToken);
            Assert.Contains("Admin", authResponse.Roles);
            Assert.True(authResponse.TokenExpiration > DateTime.UtcNow);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var client = _fixture.CreateClient();
            var loginRequest = new
            {
                UsernameOrEmail = "testuser",
                Password = "WrongPassword"
            };

            // Act
            var response = await client.PostAsync("/api/v1/auth/login", 
                TestHelper.CreateJsonContent(loginRequest));

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithMissingRequiredFields_ReturnsBadRequest()
        {
            // Arrange
            var client = _fixture.CreateClient();
            var loginRequest = new
            {
                UsernameOrEmail = ""
                // Missing password
            };

            // Act
            var response = await client.PostAsync("/api/v1/auth/login", 
                TestHelper.CreateJsonContent(loginRequest));

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Register_WithValidData_ReturnsSuccessWithToken()
        {
            // Arrange
            var client = _fixture.CreateClient();
            var registerRequest = new 
            {
                Username = "newuser",
                Password = "NewUser123!",
                ConfirmPassword = "NewUser123!",
                Email = "newuser@example.com",
                FirstName = "New",
                LastName = "User"
            };

            // Act
            var response = await client.PostAsync("/api/v1/auth/register", 
                TestHelper.CreateJsonContent(registerRequest));

            // Assert
            response.EnsureSuccessStatusCode();
            var authResponse = await TestHelper.DeserializeResponseAsync<ApiTestFixture.LoginResponseDto>(response);
            
            Assert.NotNull(authResponse);
            Assert.Equal("newuser", authResponse.Username);
            Assert.Equal("newuser@example.com", authResponse.Email);
            Assert.NotEmpty(authResponse.AccessToken);
            Assert.NotEmpty(authResponse.RefreshToken);
        }

        [Fact]
        public async Task Register_WithMismatchedPasswords_ReturnsBadRequest()
        {
            // Arrange
            var client = _fixture.CreateClient();
            var registerRequest = new 
            {
                Username = "invaliduser",
                Password = "Password123!",
                ConfirmPassword = "DifferentPassword123!",
                Email = "invalid@example.com",
                FirstName = "Invalid",
                LastName = "User"
            };

            // Act
            var response = await client.PostAsync("/api/v1/auth/register", 
                TestHelper.CreateJsonContent(registerRequest));

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Register_WithExistingUsername_ReturnsBadRequest()
        {
            // Arrange
            var client = _fixture.CreateClient();
            var registerRequest = new 
            {
                Username = "testuser", // Existing username
                Password = "Test123!",
                ConfirmPassword = "Test123!",
                Email = "unique@example.com",
                FirstName = "Test",
                LastName = "User"
            };

            // Act
            var response = await client.PostAsync("/api/v1/auth/register", 
                TestHelper.CreateJsonContent(registerRequest));

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task RefreshToken_WithValidToken_ReturnsNewTokens()
        {
            // Arrange
            // First login to get tokens
            var client = _fixture.CreateClient();
            var loginRequest = new
            {
                UsernameOrEmail = "testuser",
                Password = "Test123!"
            };

            var loginResponse = await client.PostAsync("/api/v1/auth/login", 
                TestHelper.CreateJsonContent(loginRequest));
            loginResponse.EnsureSuccessStatusCode();
            
            var authResponse = await TestHelper.DeserializeResponseAsync<ApiTestFixture.LoginResponseDto>(loginResponse);
            
            var refreshRequest = new
            {
                RefreshToken = authResponse.RefreshToken
            };

            // Act
            var response = await client.PostAsync("/api/v1/auth/refresh", 
                TestHelper.CreateJsonContent(refreshRequest));

            // Assert
            response.EnsureSuccessStatusCode();
            var refreshedTokens = await TestHelper.DeserializeResponseAsync<ApiTestFixture.LoginResponseDto>(response);
            
            Assert.NotNull(refreshedTokens);
            Assert.Equal(authResponse.Username, refreshedTokens.Username);
            Assert.NotEmpty(refreshedTokens.AccessToken);
            Assert.NotEmpty(refreshedTokens.RefreshToken);
            Assert.NotEqual(authResponse.AccessToken, refreshedTokens.AccessToken);
            Assert.NotEqual(authResponse.RefreshToken, refreshedTokens.RefreshToken);
        }

        [Fact]
        public async Task RefreshToken_WithInvalidToken_ReturnsUnauthorized()
        {
            // Arrange
            var client = _fixture.CreateClient();
            var refreshRequest = new
            {
                RefreshToken = "invalid-refresh-token"
            };

            // Act
            var response = await client.PostAsync("/api/v1/auth/refresh", 
                TestHelper.CreateJsonContent(refreshRequest));

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetCurrentUser_WithAuthenticatedUser_ReturnsUserData()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();

            // Act
            var response = await client.GetAsync("/api/v1/auth/me");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseDto = await TestHelper.DeserializeResponseAsync<ResponseDto<UserDto>>(response);
            
            Assert.NotNull(responseDto);
            Assert.True(responseDto.Success);
            Assert.NotNull(responseDto.Data);
            Assert.Equal("testuser", responseDto.Data.Username);
            Assert.Equal("test@example.com", responseDto.Data.Email);
        }

        [Fact]
        public async Task GetCurrentUser_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var response = await client.GetAsync("/api/v1/auth/me");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetCurrentUser_WithInvalidToken_ReturnsUnauthorized()
        {
            // Arrange
            var client = _fixture.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-token");

            // Act
            var response = await client.GetAsync("/api/v1/auth/me");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Logout_WithAuthenticatedUser_ReturnsSuccess()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();

            // Act
            var response = await client.PostAsync("/api/v1/auth/logout", null);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseDto = await TestHelper.DeserializeResponseAsync<ResponseDto>(response);
            
            Assert.NotNull(responseDto);
            Assert.True(responseDto.Success);
        }

        [Fact]
        public async Task Logout_ThenAccessProtectedEndpoint_ReturnsUnauthorized()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();
            
            // First logout
            var logoutResponse = await client.PostAsync("/api/v1/auth/logout", null);
            logoutResponse.EnsureSuccessStatusCode();

            // Act - try to access protected endpoint with the same client
            var response = await client.GetAsync("/api/v1/auth/me");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ChangePassword_WithValidCurrentPassword_ReturnsSuccess()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();
            var changePasswordRequest = new
            {
                CurrentPassword = "Test123!",
                NewPassword = "NewTest456!",
                ConfirmNewPassword = "NewTest456!"
            };

            // Act
            var response = await client.PostAsync("/api/v1/auth/change-password", 
                TestHelper.CreateJsonContent(changePasswordRequest));

            // Assert
            response.EnsureSuccessStatusCode();
            var responseDto = await TestHelper.DeserializeResponseAsync<ResponseDto>(response);
            
            Assert.NotNull(responseDto);
            Assert.True(responseDto.Success);
            
            // Verify we can login with the new password
            var loginRequest = new
            {
                UsernameOrEmail = "testuser",
                Password = "NewTest456!"
            };
            
            var loginResponse = await client.PostAsync("/api/v1/auth/login", 
                TestHelper.CreateJsonContent(loginRequest));
            
            loginResponse.EnsureSuccessStatusCode();
        }
    }
}