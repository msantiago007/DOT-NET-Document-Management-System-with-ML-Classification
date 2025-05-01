// -----------------------------------------------------------------------------
// <copyright file="EnhancedMLControllerTests.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            May 1, 2025
// Last Modified:      May 1, 2025
// Version:            0.9.0
// Description:        Integration tests for the Enhanced ML Controller
// -----------------------------------------------------------------------------

using DocumentManagementML.Application.DTOs;
using DocumentManagementML.IntegrationTests.TestFixtures;
using DocumentManagementML.IntegrationTests.TestHelpers;
using System.Threading.Tasks;
using Xunit;

namespace DocumentManagementML.IntegrationTests.Controllers
{
    public class EnhancedMLControllerTests : IClassFixture<ApiTestFixture>
    {
        private readonly ApiTestFixture _fixture;

        public EnhancedMLControllerTests(ApiTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task GetModelMetrics_ReturnsModelMetricsWithStandardResponse()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();
            
            // First ensure the enhanced controllers are enabled
            var configResponse = await client.GetAsync("/api/v1/config/use-enhanced-controllers?enabled=true");
            configResponse.EnsureSuccessStatusCode();

            // Act
            var response = await client.GetAsync("/api/v1/enhanced/ml/metrics");

            // Assert
            response.EnsureSuccessStatusCode();
            var metricsResponse = await TestHelper.DeserializeResponseAsync<ResponseDto<ModelMetricsDto>>(response);
            
            Assert.NotNull(metricsResponse);
            Assert.True(metricsResponse.Success);
            Assert.NotNull(metricsResponse.Data);
            Assert.NotNull(metricsResponse.Data.ModelName);
            Assert.NotNull(metricsResponse.Data.DocumentTypes);
        }

        [Fact]
        public async Task ClassifyText_WithValidText_ReturnsClassificationResultWithStandardResponse()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();
            
            // First ensure the enhanced controllers are enabled
            var configResponse = await client.GetAsync("/api/v1/config/use-enhanced-controllers?enabled=true");
            configResponse.EnsureSuccessStatusCode();
            
            var classificationRequest = new ClassificationRequestDto
            {
                Text = "This is an invoice for your recent purchase. Invoice number: INV-2025-001. Total amount: $1250.00"
            };

            // Act
            var response = await client.PostAsync("/api/v1/enhanced/ml/classify-text", 
                TestHelper.CreateJsonContent(classificationRequest));

            // Assert
            response.EnsureSuccessStatusCode();
            var classificationResponse = await TestHelper.DeserializeResponseAsync<ResponseDto<DocumentClassificationResultDto>>(response);
            
            Assert.NotNull(classificationResponse);
            Assert.True(classificationResponse.Success);
            Assert.NotNull(classificationResponse.Data);
            Assert.True(classificationResponse.Data.IsSuccessful);
            Assert.NotNull(classificationResponse.Data.DocumentTypeScores);
            Assert.NotEmpty(classificationResponse.Data.DocumentTypeScores);
            
            // Check if the system recognized it as an invoice (most likely)
            var hasInvoiceType = false;
            foreach (var score in classificationResponse.Data.DocumentTypeScores)
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
        public async Task ClassifyText_WithEmptyText_ReturnsBadRequestWithErrorResponse()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();
            
            // First ensure the enhanced controllers are enabled
            var configResponse = await client.GetAsync("/api/v1/config/use-enhanced-controllers?enabled=true");
            configResponse.EnsureSuccessStatusCode();
            
            var classificationRequest = new ClassificationRequestDto
            {
                Text = ""
            };

            // Act
            var response = await client.PostAsync("/api/v1/enhanced/ml/classify-text", 
                TestHelper.CreateJsonContent(classificationRequest));

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
            var errorResponse = await TestHelper.DeserializeResponseAsync<ResponseDto>(response);
            
            Assert.NotNull(errorResponse);
            Assert.False(errorResponse.Success);
            Assert.Contains("text", errorResponse.Message.ToLower());
        }
        
        [Fact]
        public async Task GetModelTrainingStatus_ReturnsTrainingStatusWithStandardResponse()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();
            
            // First ensure the enhanced controllers are enabled
            var configResponse = await client.GetAsync("/api/v1/config/use-enhanced-controllers?enabled=true");
            configResponse.EnsureSuccessStatusCode();

            // Act
            var response = await client.GetAsync("/api/v1/enhanced/ml/training-status");

            // Assert
            response.EnsureSuccessStatusCode();
            var statusResponse = await TestHelper.DeserializeResponseAsync<ResponseDto<string>>(response);
            
            Assert.NotNull(statusResponse);
            Assert.True(statusResponse.Success);
            Assert.NotNull(statusResponse.Data);
            Assert.Contains("model", statusResponse.Data.ToLower());
        }
        
        [Fact]
        public async Task StartTraining_AsAdmin_ReturnsAcceptedWithJobId()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync(); // Default is admin user
            
            // First ensure the enhanced controllers are enabled
            var configResponse = await client.GetAsync("/api/v1/config/use-enhanced-controllers?enabled=true");
            configResponse.EnsureSuccessStatusCode();

            // Act
            var response = await client.PostAsync("/api/v1/enhanced/ml/start-training", null);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.Accepted, response.StatusCode);
            var trainingResponse = await TestHelper.DeserializeResponseAsync<ResponseDto<string>>(response);
            
            Assert.NotNull(trainingResponse);
            Assert.True(trainingResponse.Success);
            Assert.NotNull(trainingResponse.Data);
            Assert.Contains("job", trainingResponse.Message.ToLower());
        }
        
        [Fact]
        public async Task StartTraining_AsRegularUser_ReturnsForbidden()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync("regularuser", "Test123!");
            
            // First ensure the enhanced controllers are enabled
            var configResponse = await client.GetAsync("/api/v1/config/use-enhanced-controllers?enabled=true");
            configResponse.EnsureSuccessStatusCode();

            // Act
            var response = await client.PostAsync("/api/v1/enhanced/ml/start-training", null);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}