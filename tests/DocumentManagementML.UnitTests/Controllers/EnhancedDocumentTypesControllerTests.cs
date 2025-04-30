// -----------------------------------------------------------------------------
// <copyright file="EnhancedDocumentTypesControllerTests.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 30, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        Tests for EnhancedDocumentTypesController
// -----------------------------------------------------------------------------
using DocumentManagementML.API.Controllers;
using DocumentManagementML.Application.DTOs;
using DocumentManagementML.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DocumentManagementML.UnitTests.Controllers
{
    public class EnhancedDocumentTypesControllerTests
    {
        private readonly Mock<ILogger<EnhancedDocumentTypesController>> _mockLogger;
        private readonly Mock<IDocumentTypeService> _mockDocumentTypeService;
        
        public EnhancedDocumentTypesControllerTests()
        {
            _mockLogger = new Mock<ILogger<EnhancedDocumentTypesController>>();
            _mockDocumentTypeService = new Mock<IDocumentTypeService>();
        }
        
        [Fact]
        public async Task GetAllDocumentTypes_ReturnsOkResponseWithDocumentTypes()
        {
            // Arrange
            var documentTypes = new List<DocumentTypeDto>
            {
                new DocumentTypeDto { Id = Guid.NewGuid(), Name = "Contract", IsActive = true },
                new DocumentTypeDto { Id = Guid.NewGuid(), Name = "Invoice", IsActive = true }
            };
            
            _mockDocumentTypeService
                .Setup(s => s.GetAllDocumentTypesAsync())
                .ReturnsAsync(documentTypes);
                
            var controller = new EnhancedDocumentTypesController(_mockDocumentTypeService.Object, _mockLogger.Object);
            
            // Act
            var result = await controller.GetAllDocumentTypes();
            
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseDto = Assert.IsType<ResponseDto<IEnumerable<DocumentTypeDto>>>(okResult.Value);
            
            Assert.True(responseDto.Success);
            Assert.Equal(documentTypes.Count, responseDto.Data.Count());
        }
        
        [Fact]
        public async Task GetDocumentType_WithValidId_ReturnsOkResponseWithDocumentType()
        {
            // Arrange
            var id = Guid.NewGuid();
            var documentType = new DocumentTypeDto { Id = id, Name = "Contract", IsActive = true };
            
            _mockDocumentTypeService
                .Setup(s => s.GetDocumentTypeByIdAsync(id))
                .ReturnsAsync(documentType);
                
            var controller = new EnhancedDocumentTypesController(_mockDocumentTypeService.Object, _mockLogger.Object);
            
            // Act
            var result = await controller.GetDocumentType(id);
            
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseDto = Assert.IsType<ResponseDto<DocumentTypeDto>>(okResult.Value);
            
            Assert.True(responseDto.Success);
            Assert.Equal(documentType.Id, responseDto.Data.Id);
            Assert.Equal(documentType.Name, responseDto.Data.Name);
        }
        
        [Fact]
        public async Task GetDocumentType_WithInvalidId_ReturnsNotFoundResponse()
        {
            // Arrange
            var id = Guid.NewGuid();
            
            _mockDocumentTypeService
                .Setup(s => s.GetDocumentTypeByIdAsync(id))
                .ReturnsAsync((DocumentTypeDto)null);
                
            var controller = new EnhancedDocumentTypesController(_mockDocumentTypeService.Object, _mockLogger.Object);
            
            // Act
            var result = await controller.GetDocumentType(id);
            
            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var responseDto = Assert.IsType<ResponseDto>(notFoundResult.Value);
            
            Assert.False(responseDto.Success);
        }
        
        [Fact]
        public async Task CreateDocumentType_WithValidDto_ReturnsCreatedResponse()
        {
            // Arrange
            var dto = new DocumentTypeCreateDto { Name = "Contract", Description = "Legal contracts" };
            var created = new DocumentTypeDto { Id = Guid.NewGuid(), Name = "Contract", Description = "Legal contracts", IsActive = true };
            
            _mockDocumentTypeService
                .Setup(s => s.CreateDocumentTypeAsync(dto))
                .ReturnsAsync(created);
                
            var controller = new EnhancedDocumentTypesController(_mockDocumentTypeService.Object, _mockLogger.Object);
            
            // Act
            var result = await controller.CreateDocumentType(dto);
            
            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var responseDto = Assert.IsType<ResponseDto<DocumentTypeDto>>(createdResult.Value);
            
            Assert.True(responseDto.Success);
            Assert.Equal(created.Id, responseDto.Data.Id);
            Assert.Equal("GetDocumentType", createdResult.ActionName);
        }
        
        [Fact]
        public async Task UpdateDocumentType_WithValidIdAndDto_ReturnsOkResponse()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new DocumentTypeUpdateDto { Name = "Updated Contract", Description = "Updated description" };
            var updated = new DocumentTypeDto { Id = id, Name = "Updated Contract", Description = "Updated description", IsActive = true };
            
            _mockDocumentTypeService
                .Setup(s => s.UpdateDocumentTypeAsync(id, dto))
                .ReturnsAsync(updated);
                
            var controller = new EnhancedDocumentTypesController(_mockDocumentTypeService.Object, _mockLogger.Object);
            
            // Act
            var result = await controller.UpdateDocumentType(id, dto);
            
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseDto = Assert.IsType<ResponseDto<DocumentTypeDto>>(okResult.Value);
            
            Assert.True(responseDto.Success);
            Assert.Equal(updated.Id, responseDto.Data.Id);
            Assert.Equal(dto.Name, responseDto.Data.Name);
        }
        
        [Fact]
        public async Task DeleteDocumentType_WithValidId_ReturnsOkResponse()
        {
            // Arrange
            var id = Guid.NewGuid();
            
            _mockDocumentTypeService
                .Setup(s => s.DeleteDocumentTypeAsync(id))
                .ReturnsAsync(true);
                
            var controller = new EnhancedDocumentTypesController(_mockDocumentTypeService.Object, _mockLogger.Object);
            
            // Act
            var result = await controller.DeleteDocumentType(id);
            
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseDto = Assert.IsType<ResponseDto>(okResult.Value);
            
            Assert.True(responseDto.Success);
        }
    }
}