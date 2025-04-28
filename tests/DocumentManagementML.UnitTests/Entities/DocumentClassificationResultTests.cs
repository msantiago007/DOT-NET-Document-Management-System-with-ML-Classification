using DocumentManagementML.Domain.Entities;
using System;
using System.Collections.Generic;
using Xunit;

namespace DocumentManagementML.UnitTests.Entities
{
    public class DocumentClassificationResultTests
    {
        [Fact]
        public void IsSuccessful_IsAliasForSuccess_ReturnsSameValue()
        {
            // Arrange
            var result = new DocumentClassificationResult
            {
                Success = true
            };
            
            // Act & Assert
            Assert.True(result.IsSuccessful);
            
            // Test setting the property
            result.IsSuccessful = false;
            Assert.False(result.Success);
            Assert.False(result.IsSuccessful);
        }
        
        [Fact]
        public void DocumentClassificationResult_DefaultProperties_AreInitializedCorrectly()
        {
            // Arrange & Act
            var result = new DocumentClassificationResult();
            
            // Assert
            Assert.False(result.Success);
            Assert.False(result.IsSuccessful);
            Assert.Null(result.ErrorMessage);
            Assert.Equal(string.Empty, result.DocumentType);
            Assert.Equal(0, result.Confidence);
            Assert.NotNull(result.AllPredictions);
            Assert.Empty(result.AllPredictions);
            Assert.Null(result.DocumentId);
            Assert.Equal(string.Empty, result.DocumentName);
        }
        
        [Fact]
        public void DocumentClassificationResult_WithValues_StoresCorrectly()
        {
            // Arrange
            var predictions = new Dictionary<string, float>
            {
                { "Invoice", 0.8f },
                { "Receipt", 0.1f },
                { "Contract", 0.1f }
            };
            
            var documentId = Guid.NewGuid();
            
            // Act
            var result = new DocumentClassificationResult
            {
                Success = true,
                ErrorMessage = null,
                DocumentType = "Invoice",
                Confidence = 0.8f,
                AllPredictions = predictions,
                DocumentId = documentId,
                DocumentName = "test-invoice.pdf"
            };
            
            // Assert
            Assert.True(result.Success);
            Assert.True(result.IsSuccessful);
            Assert.Null(result.ErrorMessage);
            Assert.Equal("Invoice", result.DocumentType);
            Assert.Equal(0.8f, result.Confidence);
            Assert.Same(predictions, result.AllPredictions);
            Assert.Equal(documentId, result.DocumentId);
            Assert.Equal("test-invoice.pdf", result.DocumentName);
        }
    }
}