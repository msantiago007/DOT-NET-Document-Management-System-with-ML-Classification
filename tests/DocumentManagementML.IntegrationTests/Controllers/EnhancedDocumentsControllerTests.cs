// -----------------------------------------------------------------------------
// <copyright file="EnhancedDocumentsControllerTests.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            May 1, 2025
// Last Modified:      May 1, 2025
// Version:            0.9.0
// Description:        Integration tests for the Enhanced Documents Controller
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
    public class EnhancedDocumentsControllerTests : IClassFixture<ApiTestFixture>
    {
        private readonly ApiTestFixture _fixture;

        public EnhancedDocumentsControllerTests(ApiTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task GetDocuments_ReturnsAllDocumentsWithStandardResponse()
        {
            // Arrange
            var client = _fixture.CreateClient();
            
            // First ensure the enhanced controllers are enabled
            var configResponse = await client.GetAsync("/api/v1/config/use-enhanced-controllers?enabled=true");
            configResponse.EnsureSuccessStatusCode();

            // Act
            var response = await client.GetAsync("/api/v1/enhanced/documents");

            // Assert
            response.EnsureSuccessStatusCode();
            var documentsResponse = await TestHelper.DeserializeResponseAsync<ResponseDto<List<DocumentDto>>>(response);
            
            Assert.NotNull(documentsResponse);
            Assert.True(documentsResponse.Success);
            Assert.NotNull(documentsResponse.Data);
            Assert.NotEmpty(documentsResponse.Data);
            Assert.Contains(documentsResponse.Data, d => d.DocumentName == "Invoice-2025-001");
            Assert.Contains(documentsResponse.Data, d => d.DocumentName == "Receipt-2025-001");
        }

        [Fact]
        public async Task GetDocument_WithValidId_ReturnsDocumentWithStandardResponse()
        {
            // Arrange
            var client = _fixture.CreateClient();
            
            // First ensure the enhanced controllers are enabled
            var configResponse = await client.GetAsync("/api/v1/config/use-enhanced-controllers?enabled=true");
            configResponse.EnsureSuccessStatusCode();
            
            // First get all documents to find a valid ID
            var allResponse = await client.GetAsync("/api/v1/enhanced/documents");
            allResponse.EnsureSuccessStatusCode();
            var documentsResponse = await TestHelper.DeserializeResponseAsync<ResponseDto<List<DocumentDto>>>(allResponse);
            var firstDocument = documentsResponse.Data[0];

            // Act
            var response = await client.GetAsync($"/api/v1/enhanced/documents/{firstDocument.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var documentResponse = await TestHelper.DeserializeResponseAsync<ResponseDto<DocumentDto>>(response);
            
            Assert.NotNull(documentResponse);
            Assert.True(documentResponse.Success);
            Assert.NotNull(documentResponse.Data);
            Assert.Equal(firstDocument.Id, documentResponse.Data.Id);
            Assert.Equal(firstDocument.DocumentName, documentResponse.Data.DocumentName);
        }

        [Fact]
        public async Task GetDocument_WithInvalidId_ReturnsNotFoundWithStandardResponse()
        {
            // Arrange
            var client = _fixture.CreateClient();
            
            // First ensure the enhanced controllers are enabled
            var configResponse = await client.GetAsync("/api/v1/config/use-enhanced-controllers?enabled=true");
            configResponse.EnsureSuccessStatusCode();
            
            var invalidId = Guid.NewGuid();

            // Act
            var response = await client.GetAsync($"/api/v1/enhanced/documents/{invalidId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var responseDto = await TestHelper.DeserializeResponseAsync<ResponseDto>(response);
            
            Assert.NotNull(responseDto);
            Assert.False(responseDto.Success);
            Assert.Contains("not found", responseDto.Message.ToLower());
        }

        [Fact]
        public async Task CreateDocument_WithValidData_ReturnsCreatedDocumentWithStandardResponse()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();
            
            // First ensure the enhanced controllers are enabled
            var configResponse = await client.GetAsync("/api/v1/config/use-enhanced-controllers?enabled=true");
            configResponse.EnsureSuccessStatusCode();
            
            // First get all document types to find a valid document type ID
            var documentTypesResponse = await client.GetAsync("/api/v1/enhanced/document-types");
            documentTypesResponse.EnsureSuccessStatusCode();
            var documentTypesDto = await TestHelper.DeserializeResponseAsync<ResponseDto<List<DocumentTypeDto>>>(documentTypesResponse);
            var documentType = documentTypesDto.Data.Find(dt => dt.IsActive && dt.Name == "Invoice");
            
            var newDocument = new DocumentCreateDto
            {
                DocumentName = "Enhanced Test Invoice",
                DocumentTypeId = documentType.Id
            };
            
            var fileContent = TestHelper.CreateSamplePdfDocument();
            var formContent = TestHelper.CreateDocumentFormContent(newDocument, fileContent, "enhanced-test-invoice.pdf");

            // Act
            var response = await client.PostAsync("/api/v1/enhanced/documents", formContent);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var createdDocumentResponse = await TestHelper.DeserializeResponseAsync<ResponseDto<DocumentDto>>(response);
            
            Assert.NotNull(createdDocumentResponse);
            Assert.True(createdDocumentResponse.Success);
            Assert.NotNull(createdDocumentResponse.Data);
            Assert.Equal("Enhanced Test Invoice", createdDocumentResponse.Data.DocumentName);
            Assert.Equal(documentType.Id, createdDocumentResponse.Data.DocumentTypeId);
            Assert.Equal("pdf", createdDocumentResponse.Data.FileType);
            Assert.False(createdDocumentResponse.Data.IsDeleted);
        }

        [Fact]
        public async Task UpdateDocument_WithValidData_ReturnsUpdatedDocumentWithStandardResponse()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();
            
            // First ensure the enhanced controllers are enabled
            var configResponse = await client.GetAsync("/api/v1/config/use-enhanced-controllers?enabled=true");
            configResponse.EnsureSuccessStatusCode();
            
            // First get all documents to find a valid ID
            var allResponse = await client.GetAsync("/api/v1/enhanced/documents");
            allResponse.EnsureSuccessStatusCode();
            var documentsResponse = await TestHelper.DeserializeResponseAsync<ResponseDto<List<DocumentDto>>>(allResponse);
            var documentToUpdate = documentsResponse.Data[0];
            
            var updateDto = new DocumentUpdateDto
            {
                DocumentName = $"{documentToUpdate.DocumentName} Enhanced"
            };

            // Act
            var response = await client.PutAsync($"/api/v1/enhanced/documents/{documentToUpdate.Id}", 
                TestHelper.CreateJsonContent(updateDto));

            // Assert
            response.EnsureSuccessStatusCode();
            var updatedDocumentResponse = await TestHelper.DeserializeResponseAsync<ResponseDto<DocumentDto>>(response);
            
            Assert.NotNull(updatedDocumentResponse);
            Assert.True(updatedDocumentResponse.Success);
            Assert.NotNull(updatedDocumentResponse.Data);
            Assert.Equal(documentToUpdate.Id, updatedDocumentResponse.Data.Id);
            Assert.Equal($"{documentToUpdate.DocumentName} Enhanced", updatedDocumentResponse.Data.DocumentName);
        }

        [Fact]
        public async Task DeleteDocument_WithValidId_ReturnsSuccessResponse()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();
            
            // First ensure the enhanced controllers are enabled
            var configResponse = await client.GetAsync("/api/v1/config/use-enhanced-controllers?enabled=true");
            configResponse.EnsureSuccessStatusCode();
            
            // First get all document types to find a valid document type ID
            var documentTypesResponse = await client.GetAsync("/api/v1/enhanced/document-types");
            documentTypesResponse.EnsureSuccessStatusCode();
            var documentTypesDto = await TestHelper.DeserializeResponseAsync<ResponseDto<List<DocumentTypeDto>>>(documentTypesResponse);
            var documentType = documentTypesDto.Data.Find(dt => dt.IsActive);
            
            // Create a document to delete
            var newDocument = new DocumentCreateDto
            {
                DocumentName = "Enhanced Temporary Document",
                DocumentTypeId = documentType.Id
            };
            
            var fileContent = TestHelper.CreateSamplePdfDocument();
            var formContent = TestHelper.CreateDocumentFormContent(newDocument, fileContent, "enhanced-temp-document.pdf");
            
            var createResponse = await client.PostAsync("/api/v1/enhanced/documents", formContent);
            createResponse.EnsureSuccessStatusCode();
            var createdDocumentResponse = await TestHelper.DeserializeResponseAsync<ResponseDto<DocumentDto>>(createResponse);

            // Act
            var response = await client.DeleteAsync($"/api/v1/enhanced/documents/{createdDocumentResponse.Data.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var deleteResponse = await TestHelper.DeserializeResponseAsync<ResponseDto>(response);
            
            Assert.NotNull(deleteResponse);
            Assert.True(deleteResponse.Success);
            
            // Verify the document is now marked as deleted or not found
            var getResponse = await client.GetAsync($"/api/v1/enhanced/documents/{createdDocumentResponse.Data.Id}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task ClassifyDocument_WithValidId_ReturnsClassificationResultWithStandardResponse()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();
            
            // First ensure the enhanced controllers are enabled
            var configResponse = await client.GetAsync("/api/v1/config/use-enhanced-controllers?enabled=true");
            configResponse.EnsureSuccessStatusCode();
            
            // First get all documents to find a valid ID
            var allResponse = await client.GetAsync("/api/v1/enhanced/documents");
            allResponse.EnsureSuccessStatusCode();
            var documentsResponse = await TestHelper.DeserializeResponseAsync<ResponseDto<List<DocumentDto>>>(allResponse);
            var document = documentsResponse.Data[0];

            // Act
            var response = await client.PostAsync($"/api/v1/enhanced/documents/{document.Id}/classify", null);

            // Assert
            response.EnsureSuccessStatusCode();
            var classificationResponse = await TestHelper.DeserializeResponseAsync<ResponseDto<DocumentClassificationResultDto>>(response);
            
            Assert.NotNull(classificationResponse);
            Assert.True(classificationResponse.Success);
            Assert.NotNull(classificationResponse.Data);
            Assert.True(classificationResponse.Data.IsSuccessful);
            Assert.NotNull(classificationResponse.Data.DocumentTypeScores);
            Assert.NotEmpty(classificationResponse.Data.DocumentTypeScores);
        }

        [Fact]
        public async Task ClassifyDocument_WithUploadedFile_ReturnsClassificationResultWithStandardResponse()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();
            
            // First ensure the enhanced controllers are enabled
            var configResponse = await client.GetAsync("/api/v1/config/use-enhanced-controllers?enabled=true");
            configResponse.EnsureSuccessStatusCode();
            
            var fileContent = TestHelper.CreateSamplePdfDocument();
            
            var formContent = new System.Net.Http.MultipartFormDataContent();
            var fileContentBytes = new System.Net.Http.ByteArrayContent(fileContent);
            fileContentBytes.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            formContent.Add(fileContentBytes, "File", "enhanced-test-document.pdf");

            // Act
            var response = await client.PostAsync("/api/v1/enhanced/documents/classify", formContent);

            // Assert
            response.EnsureSuccessStatusCode();
            var classificationResponse = await TestHelper.DeserializeResponseAsync<ResponseDto<DocumentClassificationResultDto>>(response);
            
            Assert.NotNull(classificationResponse);
            Assert.True(classificationResponse.Success);
            Assert.NotNull(classificationResponse.Data);
            Assert.True(classificationResponse.Data.IsSuccessful);
            Assert.NotNull(classificationResponse.Data.DocumentTypeScores);
            Assert.NotEmpty(classificationResponse.Data.DocumentTypeScores);
        }
        
        [Fact]
        public async Task GetDocumentsWithPagination_ReturnsPaginatedResults()
        {
            // Arrange
            var client = _fixture.CreateClient();
            
            // First ensure the enhanced controllers are enabled
            var configResponse = await client.GetAsync("/api/v1/config/use-enhanced-controllers?enabled=true");
            configResponse.EnsureSuccessStatusCode();

            // Act
            var response = await client.GetAsync("/api/v1/enhanced/documents?pageNumber=1&pageSize=1");

            // Assert
            response.EnsureSuccessStatusCode();
            var documentsResponse = await TestHelper.DeserializeResponseAsync<ResponseDto<List<DocumentDto>>>(response);
            
            Assert.NotNull(documentsResponse);
            Assert.True(documentsResponse.Success);
            Assert.NotNull(documentsResponse.Data);
            Assert.Single(documentsResponse.Data);
            
            // Check pagination headers
            Assert.True(response.Headers.Contains("X-Pagination"));
            var paginationHeader = response.Headers.GetValues("X-Pagination");
            Assert.NotNull(paginationHeader);
            Assert.Contains("TotalCount", paginationHeader.First());
            Assert.Contains("PageSize", paginationHeader.First());
            Assert.Contains("CurrentPage", paginationHeader.First());
            Assert.Contains("TotalPages", paginationHeader.First());
        }
    }
}