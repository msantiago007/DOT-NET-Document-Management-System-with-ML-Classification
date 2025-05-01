// -----------------------------------------------------------------------------
// <copyright file="TestHelper.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            May 1, 2025
// Last Modified:      May 1, 2025
// Version:            0.9.0
// Description:        Helper methods for integration testing
// -----------------------------------------------------------------------------

using DocumentManagementML.Application.DTOs;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DocumentManagementML.IntegrationTests.TestHelpers
{
    /// <summary>
    /// Provides helper methods for integration testing
    /// </summary>
    public static class TestHelper
    {
        /// <summary>
        /// Creates a JsonContent object from the given object
        /// </summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="obj">The object to serialize</param>
        /// <returns>A StringContent object with JSON content</returns>
        public static StringContent CreateJsonContent<T>(T obj)
        {
            var content = JsonSerializer.Serialize(obj);
            var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
            return stringContent;
        }

        /// <summary>
        /// Deserializes the response content to the specified type
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="response">The HTTP response message</param>
        /// <returns>The deserialized object</returns>
        public static async Task<T> DeserializeResponseAsync<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            
            try
            {
                return JsonSerializer.Deserialize<T>(content, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Failed to deserialize response content: {content}", ex);
            }
        }
        
        /// <summary>
        /// Creates test form content for document upload
        /// </summary>
        /// <param name="document">Document metadata</param>
        /// <param name="fileContent">File content</param>
        /// <param name="fileName">File name</param>
        /// <returns>MultipartFormDataContent for the request</returns>
        public static MultipartFormDataContent CreateDocumentFormContent(DocumentCreateDto document, byte[] fileContent, string fileName)
        {
            var formContent = new MultipartFormDataContent();
            
            // Add the document metadata
            var documentJson = JsonSerializer.Serialize(document);
            var documentContent = new StringContent(documentJson, Encoding.UTF8, "application/json");
            formContent.Add(documentContent, "Document");
            
            // Add the file
            var fileContentBytes = new ByteArrayContent(fileContent);
            fileContentBytes.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            formContent.Add(fileContentBytes, "File", fileName);
            
            return formContent;
        }
        
        /// <summary>
        /// Creates a sample PDF document for testing
        /// </summary>
        /// <returns>A byte array containing a simple PDF document</returns>
        public static byte[] CreateSamplePdfDocument()
        {
            // This is a minimalist PDF document
            var samplePdf = "%PDF-1.4\n" +
                            "1 0 obj<</Type/Catalog/Pages 2 0 R>>endobj\n" +
                            "2 0 obj<</Type/Pages/Count 1/Kids[3 0 R]>>endobj\n" +
                            "3 0 obj<</Type/Page/MediaBox[0 0 612 792]/Contents 4 0 R/Resources<</Font<</F1<</Type/Font/Subtype/Type1/BaseFont/Helvetica>>>>>>>\n" +
                            "endobj\n" +
                            "4 0 obj<</Length 44>>stream\nBT /F1 12 Tf 100 700 Td (Test Document) Tj ET\nendstream\n" +
                            "endobj\n" +
                            "xref\n0 5\n0000000000 65535 f\n0000000010 00000 n\n0000000056 00000 n\n0000000111 00000 n\n0000000254 00000 n\n" +
                            "trailer<</Size 5/Root 1 0 R>>\n" +
                            "startxref\n345\n%%EOF";
            
            return Encoding.ASCII.GetBytes(samplePdf);
        }
    }
}