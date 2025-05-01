// -----------------------------------------------------------------------------
// <copyright file="EnhancedDocumentTypesControllerTests.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            May 1, 2025
// Last Modified:      May 1, 2025
// Version:            0.9.0
// Description:        Integration tests for the Enhanced Document Types Controller
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
    public class EnhancedDocumentTypesControllerTests : IClassFixture<ApiTestFixture>
    {
        private readonly ApiTestFixture _fixture;

        public EnhancedDocumentTypesControllerTests(ApiTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task GetDocumentTypes_ReturnsAllDocumentTypesWithStandardResponse()
        {
            // Arrange
            var client = _fixture.CreateClient();
            
            // First ensure the enhanced controllers are enabled
            var configResponse = await client.GetAsync("/api/v1/config/use-enhanced-controllers?enabled=true");
            configResponse.EnsureSuccessStatusCode();

            // Act
            var response = await client.GetAsync("/api/v1/enhanced/document-types");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseDto = await TestHelper.DeserializeResponseAsync<ResponseDto<List<DocumentTypeDto>>>(response);
            
            Assert.NotNull(responseDto);
            Assert.True(responseDto.Success);
            Assert.NotNull(responseDto.Data);
            Assert.NotEmpty(responseDto.Data);
            Assert.Contains(responseDto.Data, dt => dt.Name == "Invoice");
            Assert.Contains(responseDto.Data, dt => dt.Name == "Receipt");
        }

        [Fact]
        public async Task GetDocumentType_WithValidId_ReturnsDocumentTypeWithStandardResponse()
        {
            // Arrange
            var client = _fixture.CreateClient();
            
            // First ensure the enhanced controllers are enabled
            var configResponse = await client.GetAsync("/api/v1/config/use-enhanced-controllers?enabled=true");
            configResponse.EnsureSuccessStatusCode();
            
            // First get all document types to find a valid ID
            var allResponse = await client.GetAsync("/api/v1/enhanced/document-types");
            allResponse.EnsureSuccessStatusCode();
            var documentTypesResponse = await TestHelper.DeserializeResponseAsync<ResponseDto<List<DocumentTypeDto>>>(allResponse);
            var firstDocumentType = documentTypesResponse.Data[0];

            // Act
            var response = await client.GetAsync($"/api/v1/enhanced/document-types/{firstDocumentType.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var documentTypeResponse = await TestHelper.DeserializeResponseAsync<ResponseDto<DocumentTypeDto>>(response);
            
            Assert.NotNull(documentTypeResponse);
            Assert.True(documentTypeResponse.Success);
            Assert.NotNull(documentTypeResponse.Data);
            Assert.Equal(firstDocumentType.Id, documentTypeResponse.Data.Id);
            Assert.Equal(firstDocumentType.Name, documentTypeResponse.Data.Name);
        }

        [Fact]
        public async Task GetDocumentType_WithInvalidId_ReturnsNotFoundWithStandardResponse()
        {
            // Arrange
            var client = _fixture.CreateClient();
            
            // First ensure the enhanced controllers are enabled
            var configResponse = await client.GetAsync("/api/v1/config/use-enhanced-controllers?enabled=true");
            configResponse.EnsureSuccessStatusCode();
            
            var invalidId = Guid.NewGuid();

            // Act
            var response = await client.GetAsync($"/api/v1/enhanced/document-types/{invalidId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var responseDto = await TestHelper.DeserializeResponseAsync<ResponseDto>(response);
            
            Assert.NotNull(responseDto);
            Assert.False(responseDto.Success);
            Assert.Contains("not found", responseDto.Message.ToLower());
        }

        [Fact]
        public async Task CreateDocumentType_WithValidData_ReturnsCreatedDocumentTypeWithStandardResponse()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();
            
            // First ensure the enhanced controllers are enabled
            var configResponse = await client.GetAsync("/api/v1/config/use-enhanced-controllers?enabled=true");
            configResponse.EnsureSuccessStatusCode();
            
            var newDocumentType = new DocumentTypeCreateDto
            {
                Name = "EnhancedReport",
                TypeName = "enhanced-report",
                Description = "Enhanced report documents"
            };

            // Act
            var response = await client.PostAsync("/api/v1/enhanced/document-types", 
                TestHelper.CreateJsonContent(newDocumentType));

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var createdDocumentTypeResponse = await TestHelper.DeserializeResponseAsync<ResponseDto<DocumentTypeDto>>(response);
            
            Assert.NotNull(createdDocumentTypeResponse);
            Assert.True(createdDocumentTypeResponse.Success);
            Assert.NotNull(createdDocumentTypeResponse.Data);
            Assert.Equal("EnhancedReport", createdDocumentTypeResponse.Data.Name);
            Assert.Equal("enhanced-report", createdDocumentTypeResponse.Data.TypeName);
            Assert.True(createdDocumentTypeResponse.Data.IsActive);
        }

        [Fact]
        public async Task UpdateDocumentType_WithValidData_ReturnsUpdatedDocumentTypeWithStandardResponse()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();
            
            // First ensure the enhanced controllers are enabled
            var configResponse = await client.GetAsync("/api/v1/config/use-enhanced-controllers?enabled=true");
            configResponse.EnsureSuccessStatusCode();
            
            // First get all document types to find a valid ID
            var allResponse = await client.GetAsync("/api/v1/enhanced/document-types");
            allResponse.EnsureSuccessStatusCode();
            var documentTypesResponse = await TestHelper.DeserializeResponseAsync<ResponseDto<List<DocumentTypeDto>>>(allResponse);
            var documentTypeToUpdate = documentTypesResponse.Data[0];
            
            var updateDto = new DocumentTypeUpdateDto
            {
                Name = $"{documentTypeToUpdate.Name} Enhanced",
                Description = "Updated by enhanced controller"
            };

            // Act
            var response = await client.PutAsync($"/api/v1/enhanced/document-types/{documentTypeToUpdate.Id}", 
                TestHelper.CreateJsonContent(updateDto));

            // Assert
            response.EnsureSuccessStatusCode();
            var updatedDocumentTypeResponse = await TestHelper.DeserializeResponseAsync<ResponseDto<DocumentTypeDto>>(response);
            
            Assert.NotNull(updatedDocumentTypeResponse);
            Assert.True(updatedDocumentTypeResponse.Success);
            Assert.NotNull(updatedDocumentTypeResponse.Data);
            Assert.Equal(documentTypeToUpdate.Id, updatedDocumentTypeResponse.Data.Id);
            Assert.Equal($"{documentTypeToUpdate.Name} Enhanced", updatedDocumentTypeResponse.Data.Name);
            Assert.Equal("Updated by enhanced controller", updatedDocumentTypeResponse.Data.Description);
        }

        [Fact]
        public async Task DeactivateDocumentType_WithValidId_ReturnsSuccessResponse()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();
            
            // First ensure the enhanced controllers are enabled
            var configResponse = await client.GetAsync("/api/v1/config/use-enhanced-controllers?enabled=true");
            configResponse.EnsureSuccessStatusCode();
            
            // First create a document type to deactivate
            var newDocumentType = new DocumentTypeCreateDto
            {
                Name = "Enhanced Temporary Type",
                TypeName = "enhanced-temporary",
                Description = "Enhanced temporary document type for testing"
            };
            
            var createResponse = await client.PostAsync("/api/v1/enhanced/document-types", 
                TestHelper.CreateJsonContent(newDocumentType));
            createResponse.EnsureSuccessStatusCode();
            var createdDocumentTypeResponse = await TestHelper.DeserializeResponseAsync<ResponseDto<DocumentTypeDto>>(createResponse);

            // Act
            var response = await client.DeleteAsync($"/api/v1/enhanced/document-types/{createdDocumentTypeResponse.Data.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var deactivateResponse = await TestHelper.DeserializeResponseAsync<ResponseDto>(response);
            
            Assert.NotNull(deactivateResponse);
            Assert.True(deactivateResponse.Success);
            
            // Verify the document type is now inactive
            var getResponse = await client.GetAsync($"/api/v1/enhanced/document-types/{createdDocumentTypeResponse.Data.Id}");
            getResponse.EnsureSuccessStatusCode();
            var documentTypeResponse = await TestHelper.DeserializeResponseAsync<ResponseDto<DocumentTypeDto>>(getResponse);
            
            Assert.False(documentTypeResponse.Data.IsActive);
        }
        
        [Fact]
        public async Task GetActiveDocumentTypes_ReturnsOnlyActiveDocumentTypes()
        {
            // Arrange
            var client = _fixture.CreateClient();
            
            // First ensure the enhanced controllers are enabled
            var configResponse = await client.GetAsync("/api/v1/config/use-enhanced-controllers?enabled=true");
            configResponse.EnsureSuccessStatusCode();

            // Act
            var response = await client.GetAsync("/api/v1/enhanced/document-types/active");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseDto = await TestHelper.DeserializeResponseAsync<ResponseDto<List<DocumentTypeDto>>>(response);
            
            Assert.NotNull(responseDto);
            Assert.True(responseDto.Success);
            Assert.NotNull(responseDto.Data);
            Assert.NotEmpty(responseDto.Data);
            Assert.DoesNotContain(responseDto.Data, dt => !dt.IsActive);
            Assert.All(responseDto.Data, dt => Assert.True(dt.IsActive));
        }
    }
}