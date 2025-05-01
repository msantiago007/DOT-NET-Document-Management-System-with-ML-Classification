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
using System.Net;
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
            var loginRequest = new
            {
                UsernameOrEmail = "testuser",
                Password = "Test123!"
            };

            // Act
            var response = await client.PostAsync("/api/v1/auth/login", 
                TestHelper.CreateJsonContent(loginRequest));

            // Assert
            response.EnsureSuccessStatusCode();
            var authResponse = await TestHelper.DeserializeResponseAsync<ApiTestFixture.LoginResponseDto>(response);
            
            Assert.NotNull(authResponse);
            Assert.Equal("testuser", authResponse.Username);
            Assert.NotEmpty(authResponse.AccessToken);
            Assert.NotEmpty(authResponse.RefreshToken);
            Assert.Contains("Admin", authResponse.Roles);
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
    }
}