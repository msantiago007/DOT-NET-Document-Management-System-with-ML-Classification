// -----------------------------------------------------------------------------
// <copyright file="DocumentsControllerTests.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            May 1, 2025
// Last Modified:      May 1, 2025
// Version:            0.9.0
// Description:        Integration tests for the Documents Controller
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
    public class DocumentsControllerTests : IClassFixture<ApiTestFixture>
    {
        private readonly ApiTestFixture _fixture;

        public DocumentsControllerTests(ApiTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task GetDocuments_ReturnsAllDocuments()
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var response = await client.GetAsync("/api/v1/documents");

            // Assert
            response.EnsureSuccessStatusCode();
            var documents = await TestHelper.DeserializeResponseAsync<List<DocumentDto>>(response);
            
            Assert.NotNull(documents);
            Assert.NotEmpty(documents);
            Assert.Contains(documents, d => d.DocumentName == "Invoice-2025-001");
            Assert.Contains(documents, d => d.DocumentName == "Receipt-2025-001");
        }

        [Fact]
        public async Task GetDocument_WithValidId_ReturnsDocument()
        {
            // Arrange
            var client = _fixture.CreateClient();
            
            // First get all documents to find a valid ID
            var allResponse = await client.GetAsync("/api/v1/documents");
            allResponse.EnsureSuccessStatusCode();
            var documents = await TestHelper.DeserializeResponseAsync<List<DocumentDto>>(allResponse);
            var firstDocument = documents[0];

            // Act
            var response = await client.GetAsync($"/api/v1/documents/{firstDocument.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var document = await TestHelper.DeserializeResponseAsync<DocumentDto>(response);
            
            Assert.NotNull(document);
            Assert.Equal(firstDocument.Id, document.Id);
            Assert.Equal(firstDocument.DocumentName, document.DocumentName);
        }

        [Fact]
        public async Task GetDocument_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var client = _fixture.CreateClient();
            var invalidId = Guid.NewGuid();

            // Act
            var response = await client.GetAsync($"/api/v1/documents/{invalidId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateDocument_WithValidData_ReturnsCreatedDocument()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();
            
            // First get all document types to find a valid document type ID
            var documentTypesResponse = await client.GetAsync("/api/v1/document-types");
            documentTypesResponse.EnsureSuccessStatusCode();
            var documentTypes = await TestHelper.DeserializeResponseAsync<List<DocumentTypeDto>>(documentTypesResponse);
            var documentType = documentTypes.Find(dt => dt.IsActive && dt.Name == "Invoice");
            
            var newDocument = new DocumentCreateDto
            {
                DocumentName = "Test Invoice",
                DocumentTypeId = documentType.Id
            };
            
            var fileContent = TestHelper.CreateSamplePdfDocument();
            var formContent = TestHelper.CreateDocumentFormContent(newDocument, fileContent, "test-invoice.pdf");

            // Act
            var response = await client.PostAsync("/api/v1/documents", formContent);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var createdDocument = await TestHelper.DeserializeResponseAsync<DocumentDto>(response);
            
            Assert.NotNull(createdDocument);
            Assert.Equal("Test Invoice", createdDocument.DocumentName);
            Assert.Equal(documentType.Id, createdDocument.DocumentTypeId);
            Assert.Equal("pdf", createdDocument.FileType);
            Assert.False(createdDocument.IsDeleted);
        }

        [Fact]
        public async Task UpdateDocument_WithValidData_ReturnsUpdatedDocument()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();
            
            // First get all documents to find a valid ID
            var allResponse = await client.GetAsync("/api/v1/documents");
            allResponse.EnsureSuccessStatusCode();
            var documents = await TestHelper.DeserializeResponseAsync<List<DocumentDto>>(allResponse);
            var documentToUpdate = documents[0];
            
            var updateDto = new DocumentUpdateDto
            {
                DocumentName = $"{documentToUpdate.DocumentName} Updated"
            };

            // Act
            var response = await client.PutAsync($"/api/v1/documents/{documentToUpdate.Id}", 
                TestHelper.CreateJsonContent(updateDto));

            // Assert
            response.EnsureSuccessStatusCode();
            var updatedDocument = await TestHelper.DeserializeResponseAsync<DocumentDto>(response);
            
            Assert.NotNull(updatedDocument);
            Assert.Equal(documentToUpdate.Id, updatedDocument.Id);
            Assert.Equal($"{documentToUpdate.DocumentName} Updated", updatedDocument.DocumentName);
        }

        [Fact]
        public async Task DeleteDocument_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();
            
            // First get all document types to find a valid document type ID
            var documentTypesResponse = await client.GetAsync("/api/v1/document-types");
            documentTypesResponse.EnsureSuccessStatusCode();
            var documentTypes = await TestHelper.DeserializeResponseAsync<List<DocumentTypeDto>>(documentTypesResponse);
            var documentType = documentTypes.Find(dt => dt.IsActive);
            
            // Create a document to delete
            var newDocument = new DocumentCreateDto
            {
                DocumentName = "Temporary Document",
                DocumentTypeId = documentType.Id
            };
            
            var fileContent = TestHelper.CreateSamplePdfDocument();
            var formContent = TestHelper.CreateDocumentFormContent(newDocument, fileContent, "temp-document.pdf");
            
            var createResponse = await client.PostAsync("/api/v1/documents", formContent);
            createResponse.EnsureSuccessStatusCode();
            var createdDocument = await TestHelper.DeserializeResponseAsync<DocumentDto>(createResponse);

            // Act
            var response = await client.DeleteAsync($"/api/v1/documents/{createdDocument.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            
            // Verify the document is now marked as deleted or not found
            var getResponse = await client.GetAsync($"/api/v1/documents/{createdDocument.Id}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task ClassifyDocument_WithValidId_ReturnsClassificationResult()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();
            
            // First get all documents to find a valid ID
            var allResponse = await client.GetAsync("/api/v1/documents");
            allResponse.EnsureSuccessStatusCode();
            var documents = await TestHelper.DeserializeResponseAsync<List<DocumentDto>>(allResponse);
            var document = documents[0];

            // Act
            var response = await client.PostAsync($"/api/v1/documents/{document.Id}/classify", null);

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await TestHelper.DeserializeResponseAsync<DocumentClassificationResultDto>(response);
            
            Assert.NotNull(result);
            Assert.True(result.IsSuccessful);
            Assert.NotNull(result.DocumentTypeScores);
            Assert.NotEmpty(result.DocumentTypeScores);
        }

        [Fact]
        public async Task ClassifyDocument_WithUploadedFile_ReturnsClassificationResult()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();
            var fileContent = TestHelper.CreateSamplePdfDocument();
            
            var formContent = new System.Net.Http.MultipartFormDataContent();
            var fileContentBytes = new System.Net.Http.ByteArrayContent(fileContent);
            fileContentBytes.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            formContent.Add(fileContentBytes, "File", "test-document.pdf");

            // Act
            var response = await client.PostAsync("/api/v1/documents/classify", formContent);

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await TestHelper.DeserializeResponseAsync<DocumentClassificationResultDto>(response);
            
            Assert.NotNull(result);
            Assert.True(result.IsSuccessful);
            Assert.NotNull(result.DocumentTypeScores);
            Assert.NotEmpty(result.DocumentTypeScores);
        }
    }
}