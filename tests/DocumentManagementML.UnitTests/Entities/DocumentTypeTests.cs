using DocumentManagementML.Domain.Entities;
using System;
using Xunit;

namespace DocumentManagementML.UnitTests.Entities
{
    public class DocumentTypeTests
    {
        [Fact]
        public void DocumentType_Constructor_SetsDefaultProperties()
        {
            // Arrange & Act
            var documentType = new DocumentType();
            
            // Assert
            // Initially the ID is empty, it gets set when saved to database
            Assert.Equal(Guid.Empty, documentType.DocumentTypeId);
            Assert.Equal(string.Empty, documentType.Name);
            Assert.Equal(string.Empty, documentType.TypeName);
            Assert.Equal(string.Empty, documentType.SchemaDefinition);
            Assert.True(documentType.IsActive);
            Assert.NotNull(documentType.Documents);
            Assert.Empty(documentType.Documents);
        }
        
        [Fact]
        public void DocumentType_SetProperties_PropertiesAreSet()
        {
            // Arrange
            var documentType = new DocumentType
            {
                DocumentTypeId = Guid.NewGuid(),
                Name = "Invoice Document",
                TypeName = "invoicedocument",
                Description = "Invoice documents for accounting",
                SchemaDefinition = "{\"type\":\"object\"}",
                IsActive = true
            };
            
            // Act & Assert
            Assert.Equal("Invoice Document", documentType.Name);
            Assert.Equal("invoicedocument", documentType.TypeName);
            Assert.Equal("Invoice documents for accounting", documentType.Description);
            Assert.Equal("{\"type\":\"object\"}", documentType.SchemaDefinition);
            Assert.True(documentType.IsActive);
        }
    }
}