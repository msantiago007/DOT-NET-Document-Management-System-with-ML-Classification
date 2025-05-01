// -----------------------------------------------------------------------------
// <copyright file="ApiControllerTests.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            May 1, 2025
// Last Modified:      May 1, 2025
// Version:            0.9.0
// Description:        Integration tests for the API controllers
// -----------------------------------------------------------------------------

using DocumentManagementML.IntegrationTests.TestFixtures;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace DocumentManagementML.IntegrationTests
{
    public class ApiControllerTests : IClassFixture<ApiTestFixture>
    {
        private readonly ApiTestFixture _fixture;

        public ApiControllerTests(ApiTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task GetDocumentTypes_ReturnsSuccessStatusCode()
        {
            // Arrange
            var client = _fixture.CreateClient();
            
            // Act
            var response = await client.GetAsync("/api/v1/document-types");
            
            // Assert
            response.EnsureSuccessStatusCode();
        }
        
        [Fact]
        public async Task GetDocuments_ReturnsSuccessStatusCode()
        {
            // Arrange
            var client = _fixture.CreateClient();
            
            // Act
            var response = await client.GetAsync("/api/v1/documents");
            
            // Assert
            response.EnsureSuccessStatusCode();
        }
        
        [Fact]
        public async Task Login_WithTestCredentials_ReturnsSuccess()
        {
            // Arrange
            var client = _fixture.CreateClient();
            var loginRequest = new
            {
                UsernameOrEmail = "testuser",
                Password = "Test123!"
            };
            
            // Act
            var response = await client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
            
            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("token", content.ToLower());
        }
        
        [Fact]
        public async Task AuthenticatedEndpoints_WithoutToken_ReturnsUnauthorized()
        {
            // Arrange
            var client = _fixture.CreateClient();
            
            // Act
            var response = await client.GetAsync("/api/v1/auth/me");
            
            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        
        [Fact]
        public async Task AuthenticatedEndpoints_WithToken_ReturnsSuccess()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();
            
            // Act
            var response = await client.GetAsync("/api/v1/auth/me");
            
            // Assert
            response.EnsureSuccessStatusCode();
        }
    }
}