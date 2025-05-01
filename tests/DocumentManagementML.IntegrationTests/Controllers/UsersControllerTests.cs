// -----------------------------------------------------------------------------
// <copyright file="UsersControllerTests.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            May 1, 2025
// Last Modified:      May 1, 2025
// Version:            0.9.0
// Description:        Integration tests for the Users Controller
// -----------------------------------------------------------------------------

using DocumentManagementML.Application.DTOs;
using DocumentManagementML.IntegrationTests.TestFixtures;
using DocumentManagementML.IntegrationTests.TestHelpers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace DocumentManagementML.IntegrationTests.Controllers
{
    public class UsersControllerTests : IClassFixture<ApiTestFixture>
    {
        private readonly ApiTestFixture _fixture;

        public UsersControllerTests(ApiTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task GetUsers_AsAdmin_ReturnsAllUsers()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync(); // Default is admin user

            // Act
            var response = await client.GetAsync("/api/v1/users");

            // Assert
            response.EnsureSuccessStatusCode();
            var users = await TestHelper.DeserializeResponseAsync<ResponseDto<List<UserDto>>>(response);
            
            Assert.NotNull(users);
            Assert.True(users.Success);
            Assert.NotNull(users.Data);
            Assert.NotEmpty(users.Data);
            Assert.Contains(users.Data, u => u.Username == "testuser");
            Assert.Contains(users.Data, u => u.Username == "regularuser");
        }

        [Fact]
        public async Task GetUsers_AsRegularUser_ReturnsForbidden()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync("regularuser", "Test123!");

            // Act
            var response = await client.GetAsync("/api/v1/users");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetUser_WithValidId_ReturnsUser()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();
            
            // First get all users to find a valid ID
            var allResponse = await client.GetAsync("/api/v1/users");
            allResponse.EnsureSuccessStatusCode();
            var usersResponse = await TestHelper.DeserializeResponseAsync<ResponseDto<List<UserDto>>>(allResponse);
            var firstUser = usersResponse.Data[0];

            // Act
            var response = await client.GetAsync($"/api/v1/users/{firstUser.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var user = await TestHelper.DeserializeResponseAsync<ResponseDto<UserDto>>(response);
            
            Assert.NotNull(user);
            Assert.True(user.Success);
            Assert.NotNull(user.Data);
            Assert.Equal(firstUser.Id, user.Data.Id);
            Assert.Equal(firstUser.Username, user.Data.Username);
        }

        [Fact]
        public async Task GetUser_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();
            var invalidId = Guid.NewGuid();

            // Act
            var response = await client.GetAsync($"/api/v1/users/{invalidId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateUser_WithValidData_ReturnsCreatedUser()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();
            var newUser = new
            {
                Username = "newusertest",
                Email = "newuser@example.com",
                Password = "NewUser123!",
                FirstName = "New",
                LastName = "User",
                IsAdmin = false
            };

            // Act
            var response = await client.PostAsync("/api/v1/users", 
                TestHelper.CreateJsonContent(newUser));

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var createdUserResponse = await TestHelper.DeserializeResponseAsync<ResponseDto<UserDto>>(response);
            
            Assert.NotNull(createdUserResponse);
            Assert.True(createdUserResponse.Success);
            Assert.NotNull(createdUserResponse.Data);
            Assert.Equal("newusertest", createdUserResponse.Data.Username);
            Assert.Equal("newuser@example.com", createdUserResponse.Data.Email);
            Assert.Equal("New", createdUserResponse.Data.FirstName);
            Assert.Equal("User", createdUserResponse.Data.LastName);
            Assert.False(createdUserResponse.Data.IsAdmin);
            Assert.True(createdUserResponse.Data.IsActive);
        }

        [Fact]
        public async Task UpdateUser_WithValidData_ReturnsUpdatedUser()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();
            
            // First create a user to update
            var newUser = new
            {
                Username = "updatetestuser",
                Email = "updatetest@example.com",
                Password = "Update123!",
                FirstName = "Update",
                LastName = "Test",
                IsAdmin = false
            };
            
            var createResponse = await client.PostAsync("/api/v1/users", 
                TestHelper.CreateJsonContent(newUser));
            createResponse.EnsureSuccessStatusCode();
            var createdUserResponse = await TestHelper.DeserializeResponseAsync<ResponseDto<UserDto>>(createResponse);
            
            var updateDto = new
            {
                FirstName = "Updated",
                LastName = "UserName",
                Email = "updated@example.com"
            };

            // Act
            var response = await client.PutAsync($"/api/v1/users/{createdUserResponse.Data.Id}", 
                TestHelper.CreateJsonContent(updateDto));

            // Assert
            response.EnsureSuccessStatusCode();
            var updatedUserResponse = await TestHelper.DeserializeResponseAsync<ResponseDto<UserDto>>(response);
            
            Assert.NotNull(updatedUserResponse);
            Assert.True(updatedUserResponse.Success);
            Assert.NotNull(updatedUserResponse.Data);
            Assert.Equal(createdUserResponse.Data.Id, updatedUserResponse.Data.Id);
            Assert.Equal("updatetestuser", updatedUserResponse.Data.Username); // Username shouldn't change
            Assert.Equal("updated@example.com", updatedUserResponse.Data.Email);
            Assert.Equal("Updated", updatedUserResponse.Data.FirstName);
            Assert.Equal("UserName", updatedUserResponse.Data.LastName);
        }

        [Fact]
        public async Task DeactivateUser_WithValidId_ReturnsSuccess()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();
            
            // First create a user to deactivate
            var newUser = new
            {
                Username = "deactivateuser",
                Email = "deactivate@example.com",
                Password = "Deactivate123!",
                FirstName = "Deactivate",
                LastName = "User",
                IsAdmin = false
            };
            
            var createResponse = await client.PostAsync("/api/v1/users", 
                TestHelper.CreateJsonContent(newUser));
            createResponse.EnsureSuccessStatusCode();
            var createdUserResponse = await TestHelper.DeserializeResponseAsync<ResponseDto<UserDto>>(createResponse);

            // Act
            var response = await client.DeleteAsync($"/api/v1/users/{createdUserResponse.Data.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var deactivateResponse = await TestHelper.DeserializeResponseAsync<ResponseDto>(response);
            Assert.True(deactivateResponse.Success);
            
            // Verify the user is now inactive
            var getResponse = await client.GetAsync($"/api/v1/users/{createdUserResponse.Data.Id}");
            getResponse.EnsureSuccessStatusCode();
            var user = await TestHelper.DeserializeResponseAsync<ResponseDto<UserDto>>(getResponse);
            
            Assert.False(user.Data.IsActive);
        }

        [Fact]
        public async Task GetActiveUsersCount_ReturnsCorrectCount()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();

            // Act
            var response = await client.GetAsync("/api/v1/users/active-count");

            // Assert
            response.EnsureSuccessStatusCode();
            var countResponse = await TestHelper.DeserializeResponseAsync<ResponseDto<int>>(response);
            
            Assert.NotNull(countResponse);
            Assert.True(countResponse.Success);
            Assert.True(countResponse.Data > 0, "Active users count should be greater than 0");
        }

        [Fact]
        public async Task ChangePassword_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();
            
            // First create a user to change password
            var newUser = new
            {
                Username = "passworduser",
                Email = "password@example.com",
                Password = "Password123!",
                FirstName = "Password",
                LastName = "User",
                IsAdmin = false
            };
            
            var createResponse = await client.PostAsync("/api/v1/users", 
                TestHelper.CreateJsonContent(newUser));
            createResponse.EnsureSuccessStatusCode();
            var createdUserResponse = await TestHelper.DeserializeResponseAsync<ResponseDto<UserDto>>(createResponse);
            
            var passwordRequest = new
            {
                CurrentPassword = "Password123!",
                NewPassword = "NewPassword456!"
            };

            // Act - using admin token to change another user's password
            var response = await client.PutAsync($"/api/v1/users/{createdUserResponse.Data.Id}/change-password", 
                TestHelper.CreateJsonContent(passwordRequest));

            // Assert
            response.EnsureSuccessStatusCode();
            var passwordResponse = await TestHelper.DeserializeResponseAsync<ResponseDto>(response);
            
            Assert.NotNull(passwordResponse);
            Assert.True(passwordResponse.Success);
            
            // Verify we can log in with the new password
            var loginClient = _fixture.CreateClient();
            var loginRequest = new
            {
                UsernameOrEmail = "passworduser",
                Password = "NewPassword456!"
            };
            
            var loginResponse = await loginClient.PostAsync("/api/v1/auth/login", 
                TestHelper.CreateJsonContent(loginRequest));
            
            loginResponse.EnsureSuccessStatusCode();
        }
    }
}