// -----------------------------------------------------------------------------
// <copyright file="MLControllerTests.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            May 1, 2025
// Last Modified:      May 1, 2025
// Version:            0.9.0
// Description:        Integration tests for the ML Controller
// -----------------------------------------------------------------------------

using DocumentManagementML.Application.DTOs;
using DocumentManagementML.IntegrationTests.TestFixtures;
using DocumentManagementML.IntegrationTests.TestHelpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DocumentManagementML.IntegrationTests.Controllers
{
    public class MLControllerTests : IClassFixture<ApiTestFixture>
    {
        private readonly ApiTestFixture _fixture;

        public MLControllerTests(ApiTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task GetModelMetrics_ReturnsModelMetrics()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();

            // Act
            var response = await client.GetAsync("/api/v1/ml/metrics");

            // Assert
            response.EnsureSuccessStatusCode();
            var metrics = await TestHelper.DeserializeResponseAsync<ModelMetricsDto>(response);
            
            Assert.NotNull(metrics);
            Assert.NotNull(metrics.ModelName);
            Assert.NotNull(metrics.DocumentTypes);
        }

        [Fact]
        public async Task ClassifyText_WithValidText_ReturnsClassificationResult()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();
            var classificationRequest = new ClassificationRequestDto
            {
                Text = "This is an invoice for your recent purchase. Invoice number: INV-2025-001. Total amount: $1250.00"
            };

            // Act
            var response = await client.PostAsync("/api/v1/ml/classify-text", 
                TestHelper.CreateJsonContent(classificationRequest));

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await TestHelper.DeserializeResponseAsync<DocumentClassificationResultDto>(response);
            
            Assert.NotNull(result);
            Assert.True(result.IsSuccessful);
            Assert.NotNull(result.DocumentTypeScores);
            Assert.NotEmpty(result.DocumentTypeScores);
            
            // Check if the system recognized it as an invoice (most likely)
            var hasInvoiceType = false;
            foreach (var score in result.DocumentTypeScores)
            {
                if (score.TypeName.ToLower().Contains("invoice"))
                {
                    hasInvoiceType = true;
                    break;
                }
            }
            
            Assert.True(hasInvoiceType, "The classification should identify invoice-related document types");
        }
    }
}