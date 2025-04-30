// -----------------------------------------------------------------------------
// <copyright file="BaseApplicationServiceTests.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 29, 2025
// Last Modified:      April 29, 2025
// Version:            0.9.0
// Description:        Unit tests for the BaseApplicationService
// -----------------------------------------------------------------------------
using AutoMapper;
using DocumentManagementML.Application.Services;
using DocumentManagementML.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DocumentManagementML.UnitTests.Services
{
    public class BaseApplicationServiceTests
    {
        // Test implementation of BaseApplicationService for testing
        private class TestApplicationService : BaseApplicationService
        {
            public TestApplicationService(IUnitOfWorkExtended unitOfWork, IMapper mapper, ILogger logger)
                : base(unitOfWork, mapper, logger)
            {
            }

            public Task<string> TestExecuteInTransactionAsync(string input, bool throwException = false)
            {
                return ExecuteInTransactionAsync<string>(async transaction =>
                {
                    if (throwException)
                    {
                        throw new InvalidOperationException("Test exception");
                    }

                    return await Task.FromResult(input + " processed");
                }, "Error processing input");
            }

            public Task TestExecuteInTransactionVoidAsync(bool throwException = false)
            {
                return ExecuteInTransactionAsync(async transaction =>
                {
                    if (throwException)
                    {
                        throw new InvalidOperationException("Test exception");
                    }

                    await Task.CompletedTask;
                }, "Error processing void operation");
            }
        }

        [Fact]
        public async Task ExecuteInTransaction_ShouldReturn_Result_WhenSuccessful()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWorkExtended>();
            var mockTransaction = new Mock<ITransaction>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger>();

            mockUnitOfWork.Setup(u => u.BeginTransactionAsync())
                .ReturnsAsync(mockTransaction.Object);

            var service = new TestApplicationService(
                mockUnitOfWork.Object,
                mockMapper.Object,
                mockLogger.Object);

            // Act
            var result = await service.TestExecuteInTransactionAsync("test");

            // Assert
            Assert.Equal("test processed", result);
            mockUnitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Once);
            mockUnitOfWork.Verify(u => u.CommitTransactionAsync(mockTransaction.Object), Times.Once);
            mockUnitOfWork.Verify(u => u.RollbackTransactionAsync(It.IsAny<ITransaction>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteInTransaction_ShouldRollback_WhenExceptionOccurs()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWorkExtended>();
            var mockTransaction = new Mock<ITransaction>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger>();

            mockUnitOfWork.Setup(u => u.BeginTransactionAsync())
                .ReturnsAsync(mockTransaction.Object);

            var service = new TestApplicationService(
                mockUnitOfWork.Object,
                mockMapper.Object,
                mockLogger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await service.TestExecuteInTransactionAsync("test", true));

            mockUnitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Once);
            mockUnitOfWork.Verify(u => u.CommitTransactionAsync(It.IsAny<ITransaction>()), Times.Never);
            mockUnitOfWork.Verify(u => u.RollbackTransactionAsync(mockTransaction.Object), Times.Once);
        }

        [Fact]
        public async Task ExecuteInTransactionVoid_ShouldComplete_WhenSuccessful()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWorkExtended>();
            var mockTransaction = new Mock<ITransaction>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger>();

            mockUnitOfWork.Setup(u => u.BeginTransactionAsync())
                .ReturnsAsync(mockTransaction.Object);

            var service = new TestApplicationService(
                mockUnitOfWork.Object,
                mockMapper.Object,
                mockLogger.Object);

            // Act
            await service.TestExecuteInTransactionVoidAsync();

            // Assert
            mockUnitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Once);
            mockUnitOfWork.Verify(u => u.CommitTransactionAsync(mockTransaction.Object), Times.Once);
            mockUnitOfWork.Verify(u => u.RollbackTransactionAsync(It.IsAny<ITransaction>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteInTransactionVoid_ShouldRollback_WhenExceptionOccurs()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWorkExtended>();
            var mockTransaction = new Mock<ITransaction>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger>();

            mockUnitOfWork.Setup(u => u.BeginTransactionAsync())
                .ReturnsAsync(mockTransaction.Object);

            var service = new TestApplicationService(
                mockUnitOfWork.Object,
                mockMapper.Object,
                mockLogger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await service.TestExecuteInTransactionVoidAsync(true));

            mockUnitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Once);
            mockUnitOfWork.Verify(u => u.CommitTransactionAsync(It.IsAny<ITransaction>()), Times.Never);
            mockUnitOfWork.Verify(u => u.RollbackTransactionAsync(mockTransaction.Object), Times.Once);
        }
    }
}