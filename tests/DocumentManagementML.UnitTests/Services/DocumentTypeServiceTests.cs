using AutoMapper;
using DocumentManagementML.Application.DTOs;
using DocumentManagementML.Application.Services;
using DocumentManagementML.Domain.Entities;
using DocumentManagementML.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DocumentManagementML.UnitTests.Services
{
    public class DocumentTypeServiceTests
    {
        private readonly Mock<IDocumentTypeRepository> _mockDocumentTypeRepository;
        private readonly Mock<IDocumentRepository> _mockDocumentRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<DocumentTypeService>> _mockLogger;
        private readonly DocumentTypeService _service;

        public DocumentTypeServiceTests()
        {
            _mockDocumentTypeRepository = new Mock<IDocumentTypeRepository>();
            _mockDocumentRepository = new Mock<IDocumentRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<DocumentTypeService>>();
            
            _service = new DocumentTypeService(
                _mockDocumentTypeRepository.Object,
                _mockDocumentRepository.Object,
                _mockMapper.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task CreateDocumentTypeAsync_SetsTypeName_BasedOnName()
        {
            // Arrange
            var createDto = new DocumentTypeCreateDto
            {
                Name = "Invoice Document",
                Description = "Invoice documents for accounting"
            };
            
            var documentType = new DocumentType();
            var returnedDocumentType = new DocumentType
            {
                DocumentTypeId = Guid.NewGuid(),
                Name = "Invoice Document",
                TypeName = "invoicedocument",
                Description = "Invoice documents for accounting"
            };
            
            _mockDocumentTypeRepository.Setup(r => r.GetByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((DocumentType)null);
            
            _mockMapper.Setup(m => m.Map<DocumentType>(createDto))
                .Returns(documentType);
                
            _mockDocumentTypeRepository.Setup(r => r.AddAsync(It.IsAny<DocumentType>()))
                .ReturnsAsync(returnedDocumentType);
                
            _mockMapper.Setup(m => m.Map<DocumentTypeDto>(returnedDocumentType))
                .Returns(new DocumentTypeDto());
                
            // Act
            await _service.CreateDocumentTypeAsync(createDto);
            
            // Assert
            _mockDocumentTypeRepository.Verify(r => r.AddAsync(It.Is<DocumentType>(dt => 
                dt.TypeName == "invoicedocument")), Times.Once);
        }
        
        [Fact]
        public async Task UpdateDocumentTypeAsync_UpdatesTypeName_WhenNameChanges()
        {
            // Arrange
            var updateDto = new DocumentTypeUpdateDto
            {
                Name = "Updated Invoice",
                Description = "Updated description",
                IsActive = true
            };
            
            var existingDocumentType = new DocumentType
            {
                DocumentTypeId = Guid.NewGuid(),
                Name = "Invoice Document",
                TypeName = "invoicedocument",
                Description = "Original description",
                IsActive = true
            };
            
            _mockDocumentTypeRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(existingDocumentType);
                
            _mockDocumentTypeRepository.Setup(r => r.GetByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((DocumentType)null);
                
            _mockDocumentTypeRepository.Setup(r => r.BeginTransactionAsync())
                .ReturnsAsync(Mock.Of<ITransaction>());
                
            _mockDocumentTypeRepository.Setup(r => r.UpdateAsync(It.IsAny<DocumentType>()))
                .ReturnsAsync(existingDocumentType);
                
            _mockMapper.Setup(m => m.Map<DocumentTypeDto>(existingDocumentType))
                .Returns(new DocumentTypeDto());
                
            // Act
            await _service.UpdateDocumentTypeAsync(Guid.NewGuid(), updateDto);
            
            // Assert
            _mockDocumentTypeRepository.Verify(r => r.UpdateAsync(It.Is<DocumentType>(dt => 
                dt.Name == "Updated Invoice" && dt.TypeName == "updatedinvoice")), Times.Once);
        }
    }
}