// -----------------------------------------------------------------------------
// <copyright file="ApiIntegrationTests.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            May 1, 2025
// Last Modified:      May 1, 2025
// Version:            0.9.0
// Description:        Basic integration test to verify the API is functioning
// -----------------------------------------------------------------------------

using DocumentManagementML.IntegrationTests.TestFixtures;
using System.Threading.Tasks;
using Xunit;

namespace DocumentManagementML.IntegrationTests
{
    public class ApiIntegrationTests : IClassFixture<ApiTestFixture>
    {
        private readonly ApiTestFixture _fixture;

        public ApiIntegrationTests(ApiTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Api_IsAccessible()
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var response = await client.GetAsync("/api/v1/document-types");

            // Assert
            response.EnsureSuccessStatusCode();
        }
    }
}
