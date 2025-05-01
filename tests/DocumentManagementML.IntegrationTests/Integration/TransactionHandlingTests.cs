// -----------------------------------------------------------------------------
// <copyright file="TransactionHandlingTests.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            May 1, 2025
// Last Modified:      May 1, 2025
// Version:            0.9.0
// Description:        Integration tests for transaction handling across services
// -----------------------------------------------------------------------------

using DocumentManagementML.Application.DTOs;
using DocumentManagementML.IntegrationTests.TestFixtures;
using DocumentManagementML.IntegrationTests.TestHelpers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DocumentManagementML.IntegrationTests.Integration
{
    public class TransactionHandlingTests : IClassFixture<ApiTestFixture>
    {
        private readonly ApiTestFixture _fixture;

        public TransactionHandlingTests(ApiTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task DocumentUploadWithMetadata_CommitsTransaction()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();
            
            // First ensure the enhanced controllers are enabled
            var configResponse = await client.GetAsync("/api/v1/config/use-enhanced-controllers?enabled=true");
            configResponse.EnsureSuccessStatusCode();
            
            // Get all document types to find a valid document type ID
            var documentTypesResponse = await client.GetAsync("/api/v1/enhanced/document-types");
            documentTypesResponse.EnsureSuccessStatusCode();
            var documentTypesDto = await TestHelper.DeserializeResponseAsync<ResponseDto<System.Collections.Generic.List<DocumentTypeDto>>>(documentTypesResponse);
            var documentType = documentTypesDto.Data.Find(dt => dt.IsActive && dt.Name == "Invoice");
            
            // Create a unique document name for this test
            var uniqueDocName = $"Transaction-Test-Invoice-{Guid.NewGuid()}";
            
            var newDocument = new DocumentCreateDto
            {
                DocumentName = uniqueDocName,
                DocumentTypeId = documentType.Id,
                Metadata = new System.Collections.Generic.Dictionary<string, string>
                {
                    { "InvoiceNumber", "TRX-001" },
                    { "Amount", "500.00" },
                    { "Customer", "Test Customer" },
                    { "Date", DateTime.Now.ToString("yyyy-MM-dd") }
                }
            };
            
            var fileContent = TestHelper.CreateSamplePdfDocument();
            var formContent = TestHelper.CreateDocumentFormContent(newDocument, fileContent, "transaction-test-invoice.pdf");

            // Act
            var response = await client.PostAsync("/api/v1/enhanced/documents", formContent);

            // Assert
            response.EnsureSuccessStatusCode();
            var createdDocumentResponse = await TestHelper.DeserializeResponseAsync<ResponseDto<DocumentDto>>(response);
            
            Assert.NotNull(createdDocumentResponse);
            Assert.True(createdDocumentResponse.Success);
            
            // Now verify that the document and its metadata were saved correctly
            var documentId = createdDocumentResponse.Data.Id;
            var getDocumentResponse = await client.GetAsync($"/api/v1/enhanced/documents/{documentId}");
            getDocumentResponse.EnsureSuccessStatusCode();
            
            var documentDto = await TestHelper.DeserializeResponseAsync<ResponseDto<DocumentDto>>(getDocumentResponse);
            Assert.NotNull(documentDto.Data);
            Assert.Equal(uniqueDocName, documentDto.Data.DocumentName);
            Assert.NotNull(documentDto.Data.Metadata);
            Assert.Equal(4, documentDto.Data.Metadata.Count);
            Assert.Equal("TRX-001", documentDto.Data.Metadata["InvoiceNumber"]);
            Assert.Equal("500.00", documentDto.Data.Metadata["Amount"]);
        }
        
        [Fact]
        public async Task DocumentUpdate_WithInvalidData_RollsBackTransaction()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();
            
            // First ensure the enhanced controllers are enabled
            var configResponse = await client.GetAsync("/api/v1/config/use-enhanced-controllers?enabled=true");
            configResponse.EnsureSuccessStatusCode();
            
            // Step 1: First create a valid document
            var documentTypesResponse = await client.GetAsync("/api/v1/enhanced/document-types");
            documentTypesResponse.EnsureSuccessStatusCode();
            var documentTypesDto = await TestHelper.DeserializeResponseAsync<ResponseDto<System.Collections.Generic.List<DocumentTypeDto>>>(documentTypesResponse);
            var documentType = documentTypesDto.Data.Find(dt => dt.IsActive && dt.Name == "Invoice");
            
            var uniqueDocName = $"Transaction-Rollback-Test-{Guid.NewGuid()}";
            
            var newDocument = new DocumentCreateDto
            {
                DocumentName = uniqueDocName,
                DocumentTypeId = documentType.Id
            };
            
            var fileContent = TestHelper.CreateSamplePdfDocument();
            var formContent = TestHelper.CreateDocumentFormContent(newDocument, fileContent, "transaction-rollback-test.pdf");
            
            var createResponse = await client.PostAsync("/api/v1/enhanced/documents", formContent);
            createResponse.EnsureSuccessStatusCode();
            var createdDocumentResponse = await TestHelper.DeserializeResponseAsync<ResponseDto<DocumentDto>>(createResponse);
            
            // Step 2: Try to update with an invalid document type ID (should trigger a rollback)
            var invalidUpdateDto = new 
            {
                DocumentName = "Updated Name",
                DocumentTypeId = Guid.NewGuid() // Invalid document type ID
            };
            
            // Act - this should trigger a validation failure and rollback
            var updateResponse = await client.PutAsync($"/api/v1/enhanced/documents/{createdDocumentResponse.Data.Id}", 
                TestHelper.CreateJsonContent(invalidUpdateDto));
            
            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, updateResponse.StatusCode);
            
            // Verify that the document was not updated
            var getDocumentResponse = await client.GetAsync($"/api/v1/enhanced/documents/{createdDocumentResponse.Data.Id}");
            getDocumentResponse.EnsureSuccessStatusCode();
            
            var documentDto = await TestHelper.DeserializeResponseAsync<ResponseDto<DocumentDto>>(getDocumentResponse);
            Assert.Equal(uniqueDocName, documentDto.Data.DocumentName);
            Assert.Equal(documentType.Id, documentDto.Data.DocumentTypeId);
        }
        
        [Fact]
        public async Task CreateDocumentTypeWithDocuments_CommitsTransaction()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();
            
            // First ensure the enhanced controllers are enabled
            var configResponse = await client.GetAsync("/api/v1/config/use-enhanced-controllers?enabled=true");
            configResponse.EnsureSuccessStatusCode();
            
            // Step 1: Create a new document type
            var uniqueTypeName = $"TransactionTest-{Guid.NewGuid().ToString().Substring(0, 8)}";
            var newDocumentType = new DocumentTypeCreateDto
            {
                Name = uniqueTypeName,
                TypeName = uniqueTypeName.ToLower(),
                Description = "Test document type for transaction testing"
            };
            
            var createTypeResponse = await client.PostAsync("/api/v1/enhanced/document-types", 
                TestHelper.CreateJsonContent(newDocumentType));
            createTypeResponse.EnsureSuccessStatusCode();
            var createdTypeResponse = await TestHelper.DeserializeResponseAsync<ResponseDto<DocumentTypeDto>>(createTypeResponse);
            
            // Step 2: Create a document of this type
            var newDocument = new DocumentCreateDto
            {
                DocumentName = $"Doc-For-{uniqueTypeName}",
                DocumentTypeId = createdTypeResponse.Data.Id
            };
            
            var fileContent = TestHelper.CreateSamplePdfDocument();
            var formContent = TestHelper.CreateDocumentFormContent(newDocument, fileContent, "transaction-test-doc.pdf");
            
            // Act
            var createDocResponse = await client.PostAsync("/api/v1/enhanced/documents", formContent);
            
            // Assert
            createDocResponse.EnsureSuccessStatusCode();
            var createdDocResponse = await TestHelper.DeserializeResponseAsync<ResponseDto<DocumentDto>>(createDocResponse);
            
            // Verify that both the document type and document exist
            var getTypeResponse = await client.GetAsync($"/api/v1/enhanced/document-types/{createdTypeResponse.Data.Id}");
            getTypeResponse.EnsureSuccessStatusCode();
            
            var getDocResponse = await client.GetAsync($"/api/v1/enhanced/documents/{createdDocResponse.Data.Id}");
            getDocResponse.EnsureSuccessStatusCode();
            
            var docDto = await TestHelper.DeserializeResponseAsync<ResponseDto<DocumentDto>>(getDocResponse);
            Assert.Equal(createdTypeResponse.Data.Id, docDto.Data.DocumentTypeId);
        }
        
        [Fact]
        public async Task BulkDocumentOperations_HandlesTransactionsCorrectly()
        {
            // Arrange
            var client = await _fixture.CreateAuthenticatedClientAsync();
            
            // First ensure the enhanced controllers are enabled
            var configResponse = await client.GetAsync("/api/v1/config/use-enhanced-controllers?enabled=true");
            configResponse.EnsureSuccessStatusCode();
            
            // Get all document types to find a valid document type ID
            var documentTypesResponse = await client.GetAsync("/api/v1/enhanced/document-types");
            documentTypesResponse.EnsureSuccessStatusCode();
            var documentTypesDto = await TestHelper.DeserializeResponseAsync<ResponseDto<System.Collections.Generic.List<DocumentTypeDto>>>(documentTypesResponse);
            var documentType = documentTypesDto.Data.Find(dt => dt.IsActive && dt.Name == "Invoice");
            
            // Create a unique document batch name for this test
            var batchId = Guid.NewGuid().ToString().Substring(0, 8);
            
            // Create multiple documents in a batch
            var fileContent = TestHelper.CreateSamplePdfDocument();
            var tasks = Enumerable.Range(1, 3).Select(i => CreateDocumentAsync(
                client, 
                $"Batch-{batchId}-Doc-{i}", 
                documentType.Id,
                fileContent,
                $"batch-{batchId}-doc-{i}.pdf"
            )).ToArray();
            
            // Act
            await Task.WhenAll(tasks);
            
            // Assert
            // Verify that all documents were created
            var allResponse = await client.GetAsync("/api/v1/enhanced/documents");
            allResponse.EnsureSuccessStatusCode();
            var documentsResponse = await TestHelper.DeserializeResponseAsync<ResponseDto<System.Collections.Generic.List<DocumentDto>>>(allResponse);
            
            var batchDocs = documentsResponse.Data.Where(d => d.DocumentName.StartsWith($"Batch-{batchId}")).ToList();
            Assert.Equal(3, batchDocs.Count);
            
            // Now update all documents
            var updateTasks = batchDocs.Select(d => UpdateDocumentAsync(
                client,
                d.Id,
                $"{d.DocumentName}-Updated"
            )).ToArray();
            
            await Task.WhenAll(updateTasks);
            
            // Verify all documents were updated
            allResponse = await client.GetAsync("/api/v1/enhanced/documents");
            allResponse.EnsureSuccessStatusCode();
            documentsResponse = await TestHelper.DeserializeResponseAsync<ResponseDto<System.Collections.Generic.List<DocumentDto>>>(allResponse);
            
            batchDocs = documentsResponse.Data.Where(d => d.DocumentName.StartsWith($"Batch-{batchId}")).ToList();
            Assert.Equal(3, batchDocs.Count);
            Assert.All(batchDocs, d => Assert.Contains("-Updated", d.DocumentName));
        }
        
        // Helper method to create a document
        private async Task<DocumentDto> CreateDocumentAsync(System.Net.Http.HttpClient client, string documentName, Guid documentTypeId, byte[] fileContent, string fileName)
        {
            var newDocument = new DocumentCreateDto
            {
                DocumentName = documentName,
                DocumentTypeId = documentTypeId
            };
            
            var formContent = TestHelper.CreateDocumentFormContent(newDocument, fileContent, fileName);
            
            var response = await client.PostAsync("/api/v1/enhanced/documents", formContent);
            response.EnsureSuccessStatusCode();
            
            var createdDocumentResponse = await TestHelper.DeserializeResponseAsync<ResponseDto<DocumentDto>>(response);
            return createdDocumentResponse.Data;
        }
        
        // Helper method to update a document
        private async Task<DocumentDto> UpdateDocumentAsync(System.Net.Http.HttpClient client, Guid documentId, string newName)
        {
            var updateDto = new DocumentUpdateDto
            {
                DocumentName = newName
            };
            
            var response = await client.PutAsync($"/api/v1/enhanced/documents/{documentId}", 
                TestHelper.CreateJsonContent(updateDto));
                
            response.EnsureSuccessStatusCode();
            
            var updatedDocumentResponse = await TestHelper.DeserializeResponseAsync<ResponseDto<DocumentDto>>(response);
            return updatedDocumentResponse.Data;
        }
    }
}