// -----------------------------------------------------------------------------
// <copyright file="EnhancedDocumentServiceTests.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 29, 2025
// Last Modified:      April 29, 2025
// Version:            0.9.0
// Description:        Unit tests for EnhancedDocumentService
// -----------------------------------------------------------------------------
using AutoMapper;
using DocumentManagementML.Application.DTOs;
using DocumentManagementML.Application.Exceptions;
using DocumentManagementML.Application.Interfaces;
using DocumentManagementML.Application.Services;
using DocumentManagementML.Domain.Entities;
using DocumentManagementML.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DocumentManagementML.UnitTests.Services
{
    public class EnhancedDocumentServiceTests
    {
        private readonly Mock<IUnitOfWorkExtended> _mockUnitOfWork;
        private readonly Mock<IDocumentRepository> _mockDocumentRepository;
        private readonly Mock<IDocumentTypeRepository> _mockDocumentTypeRepository;
        private readonly Mock<IDocumentMetadataRepository> _mockDocumentMetadataRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IDocumentClassificationService> _mockClassificationService;
        private readonly Mock<IFileStorageService> _mockFileStorageService;
        private readonly Mock<ILogger<EnhancedDocumentService>> _mockLogger;
        private readonly EnhancedDocumentService _service;

        public EnhancedDocumentServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWorkExtended>();
            _mockDocumentRepository = new Mock<IDocumentRepository>();
            _mockDocumentTypeRepository = new Mock<IDocumentTypeRepository>();
            _mockDocumentMetadataRepository = new Mock<IDocumentMetadataRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockClassificationService = new Mock<IDocumentClassificationService>();
            _mockFileStorageService = new Mock<IFileStorageService>();
            _mockLogger = new Mock<ILogger<EnhancedDocumentService>>();

            // Set up the unit of work to return our repository mocks
            _mockUnitOfWork.Setup(u => u.DocumentRepository).Returns(_mockDocumentRepository.Object);
            _mockUnitOfWork.Setup(u => u.DocumentTypeRepository).Returns(_mockDocumentTypeRepository.Object);
            _mockUnitOfWork.Setup(u => u.DocumentMetadataRepository).Returns(_mockDocumentMetadataRepository.Object);

            // Create the service with all mocked dependencies
            _service = new EnhancedDocumentService(
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockClassificationService.Object,
                _mockFileStorageService.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task GetDocumentByIdAsync_ShouldReturnDocumentDto_WhenDocumentExists()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var document = new Document { DocumentId = documentId, DocumentName = "Test Document" };
            var expectedDto = new DocumentDto { Id = documentId, Name = "Test Document" };

            _mockDocumentRepository.Setup(r => r.GetWithMetadataAsync(documentId))
                .ReturnsAsync(document);
            _mockMapper.Setup(m => m.Map<DocumentDto>(document))
                .Returns(expectedDto);

            // Act
            var result = await _service.GetDocumentByIdAsync(documentId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDto.Id, result.Id);
            Assert.Equal(expectedDto.Name, result.Name);
        }

        [Fact]
        public async Task GetDocumentByIdAsync_ShouldReturnNull_WhenDocumentDoesNotExist()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            
            _mockDocumentRepository.Setup(r => r.GetWithMetadataAsync(documentId))
                .ReturnsAsync((Document)null);

            // Act
            var result = await _service.GetDocumentByIdAsync(documentId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateDocumentAsync_ShouldCreateDocument_WithValidInputs()
        {
            // Arrange
            var documentDto = new DocumentCreateDto
            {
                Name = "Test Document",
                Description = "Test Description"
            };
            
            var document = new Document
            {
                DocumentId = Guid.NewGuid(),
                DocumentName = documentDto.Name,
                Description = documentDto.Description
            };
            
            var expectedDto = new DocumentDto
            {
                Id = document.DocumentId,
                Name = document.DocumentName,
                Description = document.Description
            };
            
            var mockTransaction = new Mock<ITransaction>();
            var mockFileStream = new MemoryStream();
            var fileName = "test.pdf";
            var storagePath = "storage/test.pdf";
            
            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync())
                .ReturnsAsync(mockTransaction.Object);
            
            _mockMapper.Setup(m => m.Map<Document>(documentDto))
                .Returns(document);
            
            _mockFileStorageService.Setup(f => f.StoreFileAsync(It.IsAny<Stream>(), fileName, It.IsAny<string>()))
                .ReturnsAsync(storagePath);
            
            _mockDocumentRepository.Setup(r => r.AddAsync(It.IsAny<Document>()))
                .ReturnsAsync(document);
            
            _mockMapper.Setup(m => m.Map<DocumentDto>(document))
                .Returns(expectedDto);
            
            _mockClassificationService.Setup(c => c.ClassifyDocumentAsync(It.IsAny<Stream>(), fileName))
                .ReturnsAsync(new DocumentClassificationResultDto { IsSuccessful = true });

            // Act
            var result = await _service.CreateDocumentAsync(documentDto, mockFileStream, fileName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDto.Id, result.Id);
            Assert.Equal(expectedDto.Name, result.Name);
            Assert.Equal(expectedDto.Description, result.Description);
            
            _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(mockTransaction.Object), Times.Once);
            _mockDocumentRepository.Verify(r => r.AddAsync(It.IsAny<Document>()), Times.Once);
        }

        [Fact]
        public async Task CreateDocumentAsync_ShouldValidateDocumentType_WhenProvided()
        {
            // Arrange
            var documentTypeId = Guid.NewGuid();
            var documentDto = new DocumentCreateDto
            {
                Name = "Test Document",
                Description = "Test Description",
                DocumentTypeId = documentTypeId
            };
            
            var mockTransaction = new Mock<ITransaction>();
            var mockFileStream = new MemoryStream();
            var fileName = "test.pdf";
            
            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync())
                .ReturnsAsync(mockTransaction.Object);
            
            _mockDocumentTypeRepository.Setup(r => r.GetByIdAsync(documentTypeId))
                .ReturnsAsync((DocumentType)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => 
                _service.CreateDocumentAsync(documentDto, mockFileStream, fileName));
            
            Assert.Contains("Document type with ID", exception.Message);
            _mockUnitOfWork.Verify(u => u.RollbackTransactionAsync(mockTransaction.Object), Times.Once);
        }

        [Fact]
        public async Task UpdateDocumentAsync_ShouldUpdateDocument_WhenDocumentExists()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var documentDto = new DocumentUpdateDto
            {
                Name = "Updated Document",
                Description = "Updated Description"
            };
            
            var existingDocument = new Document
            {
                DocumentId = documentId,
                DocumentName = "Original Document",
                Description = "Original Description"
            };
            
            var updatedDocument = new Document
            {
                DocumentId = documentId,
                DocumentName = documentDto.Name,
                Description = documentDto.Description
            };
            
            var expectedDto = new DocumentDto
            {
                Id = documentId,
                Name = documentDto.Name,
                Description = documentDto.Description
            };
            
            var mockTransaction = new Mock<ITransaction>();
            
            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync())
                .ReturnsAsync(mockTransaction.Object);
            
            _mockDocumentRepository.Setup(r => r.GetWithMetadataAsync(documentId))
                .ReturnsAsync(existingDocument);
            
            _mockDocumentRepository.Setup(r => r.UpdateAsync(It.IsAny<Document>()))
                .ReturnsAsync(updatedDocument);
            
            _mockMapper.Setup(m => m.Map<DocumentDto>(updatedDocument))
                .Returns(expectedDto);

            // Act
            var result = await _service.UpdateDocumentAsync(documentId, documentDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDto.Id, result.Id);
            Assert.Equal(expectedDto.Name, result.Name);
            Assert.Equal(expectedDto.Description, result.Description);
            
            _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(mockTransaction.Object), Times.Once);
            _mockDocumentRepository.Verify(r => r.UpdateAsync(It.IsAny<Document>()), Times.Once);
        }

        [Fact]
        public async Task UpdateDocumentAsync_ShouldReturnNull_WhenDocumentDoesNotExist()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var documentDto = new DocumentUpdateDto
            {
                Name = "Updated Document",
                Description = "Updated Description"
            };
            
            var mockTransaction = new Mock<ITransaction>();
            
            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync())
                .ReturnsAsync(mockTransaction.Object);
            
            _mockDocumentRepository.Setup(r => r.GetWithMetadataAsync(documentId))
                .ReturnsAsync((Document)null);

            // Act
            var result = await _service.UpdateDocumentAsync(documentId, documentDto);

            // Assert
            Assert.Null(result);
            _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(mockTransaction.Object), Times.Once);
        }

        [Fact]
        public async Task DeleteDocumentAsync_ShouldReturnTrue_WhenDocumentExists()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var document = new Document { DocumentId = documentId };
            var mockTransaction = new Mock<ITransaction>();
            
            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync())
                .ReturnsAsync(mockTransaction.Object);
            
            _mockDocumentRepository.Setup(r => r.GetByIdAsync(documentId))
                .ReturnsAsync(document);
            
            _mockDocumentRepository.Setup(r => r.SoftDeleteAsync(documentId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.DeleteDocumentAsync(documentId);

            // Assert
            Assert.True(result);
            _mockDocumentRepository.Verify(r => r.SoftDeleteAsync(documentId), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(mockTransaction.Object), Times.Once);
        }

        [Fact]
        public async Task DeleteDocumentAsync_ShouldReturnFalse_WhenDocumentDoesNotExist()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var mockTransaction = new Mock<ITransaction>();
            
            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync())
                .ReturnsAsync(mockTransaction.Object);
            
            _mockDocumentRepository.Setup(r => r.GetByIdAsync(documentId))
                .ReturnsAsync((Document)null);

            // Act
            var result = await _service.DeleteDocumentAsync(documentId);

            // Assert
            Assert.False(result);
            _mockDocumentRepository.Verify(r => r.SoftDeleteAsync(documentId), Times.Never);
            _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(mockTransaction.Object), Times.Once);
        }
    }
}