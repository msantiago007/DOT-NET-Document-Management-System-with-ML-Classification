// -----------------------------------------------------------------------------
// <copyright file="DocumentTypesControllerTests.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            May 1, 2025
// Last Modified:      May 1, 2025
// Version:            0.9.0
// Description:        Integration tests for the Document Types Controller
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
    public class DocumentTypesControllerTests : IClassFixture<ApiTestFixture>
    {
        private readonly ApiTestFixture _fixture;

        public DocumentTypesControllerTests(ApiTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task GetDocumentTypes_ReturnsAllDocumentTypes()
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var response = await client.GetAsync("/api/v1/document-types");

            // Assert
            response.EnsureSuccessStatusCode();
            var documentTypes = await TestHelper.DeserializeResponseAsync<List<DocumentTypeDto>>(response);
            
            Assert.NotNull(documentTypes);
            Assert.NotEmpty(documentTypes);
            Assert.Contains(documentTypes, dt => dt.Name == "Invoice");
            Assert.Contains(documentTypes, dt => dt.Name == "Receipt");
        }

        [Fact]
        public async Task GetDocumentType_WithValidId_ReturnsDocumentType()
        {
            // Arrange
            var client = _fixture.CreateClient();
            
            // First get all document types to find a valid ID
            var allResponse = await client.GetAsync("/api/v1/document-types");
            allResponse.EnsureSuccessStatusCode();
            var documentTypes = await TestHelper.DeserializeResponseAsync<List<DocumentTypeDto>>(allResponse);
            var firstDocumentType = documentTypes[0];

            // Act
            var response = await client.GetAsync($"/api/v1/document-types/{firstDocumentType.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var documentType = await TestHelper.DeserializeResponseAsync<DocumentTypeDto>(response);
            
            Assert.NotNull(documentType);
            Assert.Equal(firstDocumentType.Id, documentType.Id);
            Assert.Equal(firstDocumentType.Name, documentType.Name);
        }

        [Fact]
        public async Task GetDocumentType_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var client = _fixture.CreateClient();
            var invalidId = Guid.NewGuid();

            // Act
            var response = await client.GetAsync($"/api/v1/document-types/{invalidId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateDocumentType_WithValidData_ReturnsCreatedDocumentType()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();
            var newDocumentType = new DocumentTypeCreateDto
            {
                Name = "Report",
                TypeName = "report",
                Description = "Report documents"
            };

            // Act
            var response = await client.PostAsync("/api/v1/document-types", 
                TestHelper.CreateJsonContent(newDocumentType));

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var createdDocumentType = await TestHelper.DeserializeResponseAsync<DocumentTypeDto>(response);
            
            Assert.NotNull(createdDocumentType);
            Assert.Equal("Report", createdDocumentType.Name);
            Assert.Equal("report", createdDocumentType.TypeName);
            Assert.True(createdDocumentType.IsActive);
        }

        [Fact]
        public async Task UpdateDocumentType_WithValidData_ReturnsUpdatedDocumentType()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();
            
            // First get all document types to find a valid ID
            var allResponse = await client.GetAsync("/api/v1/document-types");
            allResponse.EnsureSuccessStatusCode();
            var documentTypes = await TestHelper.DeserializeResponseAsync<List<DocumentTypeDto>>(allResponse);
            var documentTypeToUpdate = documentTypes[0];
            
            var updateDto = new DocumentTypeUpdateDto
            {
                Name = $"{documentTypeToUpdate.Name} Updated",
                Description = "Updated description"
            };

            // Act
            var response = await client.PutAsync($"/api/v1/document-types/{documentTypeToUpdate.Id}", 
                TestHelper.CreateJsonContent(updateDto));

            // Assert
            response.EnsureSuccessStatusCode();
            var updatedDocumentType = await TestHelper.DeserializeResponseAsync<DocumentTypeDto>(response);
            
            Assert.NotNull(updatedDocumentType);
            Assert.Equal(documentTypeToUpdate.Id, updatedDocumentType.Id);
            Assert.Equal($"{documentTypeToUpdate.Name} Updated", updatedDocumentType.Name);
            Assert.Equal("Updated description", updatedDocumentType.Description);
        }

        [Fact]
        public async Task DeactivateDocumentType_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();
            
            // First create a document type to deactivate
            var newDocumentType = new DocumentTypeCreateDto
            {
                Name = "Temporary Type",
                TypeName = "temporary",
                Description = "Temporary document type for testing"
            };
            
            var createResponse = await client.PostAsync("/api/v1/document-types", 
                TestHelper.CreateJsonContent(newDocumentType));
            createResponse.EnsureSuccessStatusCode();
            var createdDocumentType = await TestHelper.DeserializeResponseAsync<DocumentTypeDto>(createResponse);

            // Act
            var response = await client.DeleteAsync($"/api/v1/document-types/{createdDocumentType.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            
            // Verify the document type is now inactive
            var getResponse = await client.GetAsync($"/api/v1/document-types/{createdDocumentType.Id}");
            getResponse.EnsureSuccessStatusCode();
            var documentType = await TestHelper.DeserializeResponseAsync<DocumentTypeDto>(getResponse);
            
            Assert.False(documentType.IsActive);
        }
    }
}