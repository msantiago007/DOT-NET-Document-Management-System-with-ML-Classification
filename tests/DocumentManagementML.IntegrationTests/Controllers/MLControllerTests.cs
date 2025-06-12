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
using System;
using System.Collections.Generic;
using System.Net;
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
        public async Task GetModelMetrics_AsUnauthenticatedUser_ReturnsUnauthorized()
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var response = await client.GetAsync("/api/v1/ml/metrics");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
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
        
        [Fact]
        public async Task ClassifyText_WithEmptyText_ReturnsBadRequest()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();
            var classificationRequest = new ClassificationRequestDto
            {
                Text = string.Empty
            };

            // Act
            var response = await client.PostAsync("/api/v1/ml/classify-text", 
                TestHelper.CreateJsonContent(classificationRequest));

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        
        [Fact]
        public async Task ClassifyText_WithTooLongText_ReturnsBadRequest()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();
            // Create a string that exceeds typical max length (100KB)
            var tooLongText = new string('x', 102400);
            var classificationRequest = new ClassificationRequestDto
            {
                Text = tooLongText
            };

            // Act
            var response = await client.PostAsync("/api/v1/ml/classify-text", 
                TestHelper.CreateJsonContent(classificationRequest));

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        
        [Fact]
        public async Task ClassifyText_AsUnauthenticatedUser_ReturnsUnauthorized()
        {
            // Arrange
            var client = _fixture.CreateClient();
            var classificationRequest = new ClassificationRequestDto
            {
                Text = "This is an invoice."
            };

            // Act
            var response = await client.PostAsync("/api/v1/ml/classify-text", 
                TestHelper.CreateJsonContent(classificationRequest));

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        
        [Fact]
        public async Task GetModelVersion_ReturnsModelVersion()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();

            // Act
            var response = await client.GetAsync("/api/v1/ml/version");

            // Assert
            response.EnsureSuccessStatusCode();
            var versionInfo = await response.Content.ReadAsStringAsync();
            
            Assert.NotNull(versionInfo);
            Assert.NotEmpty(versionInfo);
        }
        
        [Fact]
        public async Task GetModelTrainingStatus_ReturnsTrainingStatus()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();

            // Act
            var response = await client.GetAsync("/api/v1/ml/training-status");

            // Assert
            response.EnsureSuccessStatusCode();
            var status = await response.Content.ReadAsStringAsync();
            
            Assert.NotNull(status);
            Assert.NotEmpty(status);
        }
        
        [Fact]
        public async Task StartModelTraining_AsAdmin_ReturnsAccepted()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync(); // Default is admin user

            // Act
            var response = await client.PostAsync("/api/v1/ml/train", null);

            // Assert
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
            var jobId = await response.Content.ReadAsStringAsync();
            
            Assert.NotNull(jobId);
            Assert.NotEmpty(jobId);
        }
        
        [Fact]
        public async Task StartModelTraining_AsRegularUser_ReturnsForbidden()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync("regularuser", "Test123!");

            // Act
            var response = await client.PostAsync("/api/v1/ml/train", null);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
        
        [Fact]
        public async Task EvaluateModel_AsAdmin_ReturnsModelEvaluation()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync(); // Default is admin user

            // Act
            var response = await client.GetAsync("/api/v1/ml/evaluate");

            // Assert
            response.EnsureSuccessStatusCode();
            var evaluationResult = await TestHelper.DeserializeResponseAsync<ModelMetricsDto>(response);
            
            Assert.NotNull(evaluationResult);
            Assert.NotNull(evaluationResult.ModelName);
            Assert.NotNull(evaluationResult.DocumentTypes);
            Assert.InRange(evaluationResult.Accuracy, 0.0, 1.0);
        }
    }
}