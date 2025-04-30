// -----------------------------------------------------------------------------
// <copyright file="EnhancedMLControllerTests.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 30, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        Tests for EnhancedMLController
// -----------------------------------------------------------------------------
using DocumentManagementML.API.Controllers;
using DocumentManagementML.Application.DTOs;
using DocumentManagementML.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DocumentManagementML.UnitTests.Controllers
{
    public class EnhancedMLControllerTests
    {
        private readonly Mock<ILogger<EnhancedMLController>> _mockLogger;
        private readonly Mock<IDocumentClassificationService> _mockClassificationService;
        
        public EnhancedMLControllerTests()
        {
            _mockLogger = new Mock<ILogger<EnhancedMLController>>();
            _mockClassificationService = new Mock<IDocumentClassificationService>();
        }
        
        [Fact]
        public async Task GetModelMetrics_ReturnsOkResponseWithMetrics()
        {
            // Arrange
            var metrics = new ModelMetricsDto
            {
                Accuracy = 0.95,
                Precision = 0.92,
                Recall = 0.93,
                F1Score = 0.925,
                LastTrainingDate = DateTime.UtcNow
            };
            
            _mockClassificationService
                .Setup(s => s.EvaluateModelAsync())
                .ReturnsAsync(metrics);
                
            var controller = new EnhancedMLController(_mockClassificationService.Object, _mockLogger.Object);
            
            // Act
            var result = await controller.GetModelMetrics();
            
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseDto = Assert.IsType<ResponseDto<ModelMetricsDto>>(okResult.Value);
            
            Assert.True(responseDto.Success);
            Assert.Equal(metrics, responseDto.Data);
        }
        
        [Fact]
        public void GetModelStatus_ReturnsOkResponseWithStatus()
        {
            // Arrange
            var controller = new EnhancedMLController(_mockClassificationService.Object, _mockLogger.Object);
            
            // Act
            var result = controller.GetModelStatus();
            
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseDto = Assert.IsType<ResponseDto<ModelStatusDto>>(okResult.Value);
            
            Assert.True(responseDto.Success);
            Assert.NotNull(responseDto.Data);
            Assert.NotNull(responseDto.Data.Status);
            Assert.InRange(responseDto.Data.Progress, 0, 100);
        }
        
        [Fact]
        public async Task TrainModel_ReturnsAcceptedResponse()
        {
            // Arrange
            var controller = new EnhancedMLController(_mockClassificationService.Object, _mockLogger.Object);
            
            // Act
            var result = await controller.TrainModel();
            
            // Assert
            var acceptedResult = Assert.IsType<AcceptedAtActionResult>(result);
            var responseDto = Assert.IsType<ResponseDto>(acceptedResult.Value);
            
            Assert.True(responseDto.Success);
            Assert.Equal("GetModelStatus", acceptedResult.ActionName);
        }
    }
}