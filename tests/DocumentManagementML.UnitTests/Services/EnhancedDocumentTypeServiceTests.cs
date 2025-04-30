// -----------------------------------------------------------------------------
// <copyright file="EnhancedDocumentTypeServiceTests.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 29, 2025
// Last Modified:      April 29, 2025
// Version:            0.9.0
// Description:        Unit tests for EnhancedDocumentTypeService
// -----------------------------------------------------------------------------
using AutoMapper;
using DocumentManagementML.Application.DTOs;
using DocumentManagementML.Application.Exceptions;
using DocumentManagementML.Application.Services;
using DocumentManagementML.Domain.Entities;
using DocumentManagementML.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DocumentManagementML.UnitTests.Services
{
    public class EnhancedDocumentTypeServiceTests
    {
        private readonly Mock<IUnitOfWorkExtended> _mockUnitOfWork;
        private readonly Mock<IDocumentTypeRepository> _mockDocumentTypeRepository;
        private readonly Mock<IDocumentRepository> _mockDocumentRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<EnhancedDocumentTypeService>> _mockLogger;
        private readonly EnhancedDocumentTypeService _service;

        public EnhancedDocumentTypeServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWorkExtended>();
            _mockDocumentTypeRepository = new Mock<IDocumentTypeRepository>();
            _mockDocumentRepository = new Mock<IDocumentRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<EnhancedDocumentTypeService>>();

            // Set up the unit of work to return our repository mocks
            _mockUnitOfWork.Setup(u => u.DocumentTypeRepository).Returns(_mockDocumentTypeRepository.Object);
            _mockUnitOfWork.Setup(u => u.DocumentRepository).Returns(_mockDocumentRepository.Object);

            // Create the service with all mocked dependencies
            _service = new EnhancedDocumentTypeService(
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task GetDocumentTypeByIdAsync_ShouldReturnDocumentTypeDto_WhenDocumentTypeExists()
        {
            // Arrange
            var typeId = Guid.NewGuid();
            var documentType = new DocumentType { DocumentTypeId = typeId, Name = "Test Type" };
            var expectedDto = new DocumentTypeDto { Id = typeId, Name = "Test Type" };

            _mockDocumentTypeRepository.Setup(r => r.GetByIdAsync(typeId))
                .ReturnsAsync(documentType);
            _mockMapper.Setup(m => m.Map<DocumentTypeDto>(documentType))
                .Returns(expectedDto);

            // Act
            var result = await _service.GetDocumentTypeByIdAsync(typeId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDto.Id, result.Id);
            Assert.Equal(expectedDto.Name, result.Name);
        }

        [Fact]
        public async Task GetDocumentTypeByIdAsync_ShouldReturnNull_WhenDocumentTypeDoesNotExist()
        {
            // Arrange
            var typeId = Guid.NewGuid();
            
            _mockDocumentTypeRepository.Setup(r => r.GetByIdAsync(typeId))
                .ReturnsAsync((DocumentType)null);

            // Act
            var result = await _service.GetDocumentTypeByIdAsync(typeId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllDocumentTypesAsync_ShouldReturnAllDocumentTypes()
        {
            // Arrange
            var documentTypes = new List<DocumentType>
            {
                new DocumentType { DocumentTypeId = Guid.NewGuid(), Name = "Type 1" },
                new DocumentType { DocumentTypeId = Guid.NewGuid(), Name = "Type 2" }
            };
            
            var expectedDtos = documentTypes.Select(dt => new DocumentTypeDto 
            { 
                Id = dt.DocumentTypeId, 
                Name = dt.Name 
            }).ToList();

            _mockDocumentTypeRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(documentTypes);
            _mockMapper.Setup(m => m.Map<IEnumerable<DocumentTypeDto>>(documentTypes))
                .Returns(expectedDtos);

            // Act
            var result = await _service.GetAllDocumentTypesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task CreateDocumentTypeAsync_ShouldCreateDocumentType_WithValidInputs()
        {
            // Arrange
            var createDto = new DocumentTypeCreateDto
            {
                Name = "New Type",
                Description = "Test Description",
                IsActive = true
            };
            
            var documentType = new DocumentType
            {
                DocumentTypeId = Guid.NewGuid(),
                Name = createDto.Name,
                Description = createDto.Description,
                IsActive = createDto.IsActive
            };
            
            var expectedDto = new DocumentTypeDto
            {
                Id = documentType.DocumentTypeId,
                Name = documentType.Name,
                Description = documentType.Description,
                IsActive = documentType.IsActive
            };
            
            var mockTransaction = new Mock<ITransaction>();
            
            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync())
                .ReturnsAsync(mockTransaction.Object);
            
            _mockDocumentTypeRepository.Setup(r => r.GetByNameAsync(createDto.Name))
                .ReturnsAsync((DocumentType)null);
            
            _mockMapper.Setup(m => m.Map<DocumentType>(createDto))
                .Returns(documentType);
            
            _mockDocumentTypeRepository.Setup(r => r.AddAsync(It.IsAny<DocumentType>()))
                .ReturnsAsync(documentType);
            
            _mockMapper.Setup(m => m.Map<DocumentTypeDto>(documentType))
                .Returns(expectedDto);

            // Act
            var result = await _service.CreateDocumentTypeAsync(createDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDto.Id, result.Id);
            Assert.Equal(expectedDto.Name, result.Name);
            Assert.Equal(expectedDto.Description, result.Description);
            Assert.Equal(expectedDto.IsActive, result.IsActive);
            
            _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(mockTransaction.Object), Times.Once);
            _mockDocumentTypeRepository.Verify(r => r.AddAsync(It.IsAny<DocumentType>()), Times.Once);
        }

        [Fact]
        public async Task CreateDocumentTypeAsync_ShouldThrowValidationException_WhenNameAlreadyExists()
        {
            // Arrange
            var createDto = new DocumentTypeCreateDto
            {
                Name = "Existing Type",
                Description = "Test Description",
                IsActive = true
            };
            
            var existingType = new DocumentType
            {
                DocumentTypeId = Guid.NewGuid(),
                Name = createDto.Name
            };
            
            var mockTransaction = new Mock<ITransaction>();
            
            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync())
                .ReturnsAsync(mockTransaction.Object);
            
            _mockDocumentTypeRepository.Setup(r => r.GetByNameAsync(createDto.Name))
                .ReturnsAsync(existingType);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => 
                _service.CreateDocumentTypeAsync(createDto));
            
            Assert.Contains("already exists", exception.Message);
            _mockUnitOfWork.Verify(u => u.RollbackTransactionAsync(mockTransaction.Object), Times.Once);
        }

        [Fact]
        public async Task UpdateDocumentTypeAsync_ShouldUpdateDocumentType_WhenDocumentTypeExists()
        {
            // Arrange
            var typeId = Guid.NewGuid();
            var updateDto = new DocumentTypeUpdateDto
            {
                Name = "Updated Type",
                Description = "Updated Description",
                IsActive = true
            };
            
            var existingType = new DocumentType
            {
                DocumentTypeId = typeId,
                Name = "Original Type",
                Description = "Original Description",
                IsActive = false
            };
            
            var updatedType = new DocumentType
            {
                DocumentTypeId = typeId,
                Name = updateDto.Name,
                Description = updateDto.Description,
                IsActive = updateDto.IsActive
            };
            
            var expectedDto = new DocumentTypeDto
            {
                Id = typeId,
                Name = updateDto.Name,
                Description = updateDto.Description,
                IsActive = updateDto.IsActive
            };
            
            var mockTransaction = new Mock<ITransaction>();
            
            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync())
                .ReturnsAsync(mockTransaction.Object);
            
            _mockDocumentTypeRepository.Setup(r => r.GetByIdAsync(typeId))
                .ReturnsAsync(existingType);
            
            _mockDocumentTypeRepository.Setup(r => r.GetByNameAsync(updateDto.Name))
                .ReturnsAsync((DocumentType)null);
            
            _mockDocumentTypeRepository.Setup(r => r.UpdateAsync(It.IsAny<DocumentType>()))
                .ReturnsAsync(updatedType);
            
            _mockMapper.Setup(m => m.Map<DocumentTypeDto>(updatedType))
                .Returns(expectedDto);

            // Act
            var result = await _service.UpdateDocumentTypeAsync(typeId, updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDto.Id, result.Id);
            Assert.Equal(expectedDto.Name, result.Name);
            Assert.Equal(expectedDto.Description, result.Description);
            Assert.Equal(expectedDto.IsActive, result.IsActive);
            
            _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(mockTransaction.Object), Times.Once);
            _mockDocumentTypeRepository.Verify(r => r.UpdateAsync(It.IsAny<DocumentType>()), Times.Once);
        }

        [Fact]
        public async Task DeleteDocumentTypeAsync_ShouldThrowValidationException_WhenDocumentTypeInUse()
        {
            // Arrange
            var typeId = Guid.NewGuid();
            var documentType = new DocumentType { DocumentTypeId = typeId, Name = "Test Type" };
            var documents = new List<Document> { new Document { DocumentId = Guid.NewGuid() } };
            var mockTransaction = new Mock<ITransaction>();
            
            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync())
                .ReturnsAsync(mockTransaction.Object);
            
            _mockDocumentTypeRepository.Setup(r => r.GetByIdAsync(typeId))
                .ReturnsAsync(documentType);
            
            _mockDocumentRepository.Setup(r => r.GetByDocumentTypeAsync(typeId))
                .ReturnsAsync(documents);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => 
                _service.DeleteDocumentTypeAsync(typeId));
            
            Assert.Contains("in use by documents", exception.Message);
            _mockUnitOfWork.Verify(u => u.RollbackTransactionAsync(mockTransaction.Object), Times.Once);
        }

        [Fact]
        public async Task DeleteDocumentTypeAsync_ShouldDeleteDocumentType_WhenNotInUse()
        {
            // Arrange
            var typeId = Guid.NewGuid();
            var documentType = new DocumentType { DocumentTypeId = typeId, Name = "Test Type" };
            var mockTransaction = new Mock<ITransaction>();
            
            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync())
                .ReturnsAsync(mockTransaction.Object);
            
            _mockDocumentTypeRepository.Setup(r => r.GetByIdAsync(typeId))
                .ReturnsAsync(documentType);
            
            _mockDocumentRepository.Setup(r => r.GetByDocumentTypeAsync(typeId))
                .ReturnsAsync(new List<Document>());
            
            _mockDocumentTypeRepository.Setup(r => r.DeleteAsync(typeId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteDocumentTypeAsync(typeId);

            // Assert
            Assert.True(result);
            _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(mockTransaction.Object), Times.Once);
        }
    }
}