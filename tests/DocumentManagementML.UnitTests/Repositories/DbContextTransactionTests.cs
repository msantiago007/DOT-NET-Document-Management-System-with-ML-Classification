// -----------------------------------------------------------------------------
// <copyright file="DbContextTransactionTests.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 29, 2025
// Last Modified:      April 29, 2025
// Version:            0.9.0
// Description:        Unit tests for the DbContextTransaction class
// -----------------------------------------------------------------------------

using DocumentManagementML.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DocumentManagementML.UnitTests.Repositories
{
    public class DbContextTransactionTests
    {
        [Fact]
        public void Constructor_WithNullTransaction_ThrowsArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new DbContextTransaction(null));
        }
        
        [Fact]
        public async Task CommitAsync_CallsUnderlyingTransaction()
        {
            // Arrange
            var mockTransaction = new Mock<IDbContextTransaction>();
            mockTransaction.Setup(t => t.CommitAsync(default))
                .Returns(Task.CompletedTask);
                
            var transaction = new DbContextTransaction(mockTransaction.Object);
            
            // Act
            await transaction.CommitAsync();
            
            // Assert
            mockTransaction.Verify(t => t.CommitAsync(default), Times.Once);
        }
        
        [Fact]
        public async Task RollbackAsync_CallsUnderlyingTransaction()
        {
            // Arrange
            var mockTransaction = new Mock<IDbContextTransaction>();
            mockTransaction.Setup(t => t.RollbackAsync(default))
                .Returns(Task.CompletedTask);
                
            var transaction = new DbContextTransaction(mockTransaction.Object);
            
            // Act
            await transaction.RollbackAsync();
            
            // Assert
            mockTransaction.Verify(t => t.RollbackAsync(default), Times.Once);
        }
        
        [Fact]
        public void GetDbContextTransaction_ReturnsUnderlyingTransaction()
        {
            // Arrange
            var mockTransaction = new Mock<IDbContextTransaction>();
            var transaction = new DbContextTransaction(mockTransaction.Object);
            
            // Act
            var result = transaction.GetDbContextTransaction();
            
            // Assert
            Assert.Same(mockTransaction.Object, result);
        }
        
        [Fact]
        public void Dispose_CallsDisposeOnUnderlyingTransaction()
        {
            // Arrange
            var mockTransaction = new Mock<IDbContextTransaction>();
            var transaction = new DbContextTransaction(mockTransaction.Object);
            
            // Act
            transaction.Dispose();
            
            // Assert
            mockTransaction.Verify(t => t.Dispose(), Times.Once);
        }
        
        [Fact]
        public async Task DisposeAsync_CallsDisposeAsyncOnUnderlyingTransaction()
        {
            // Arrange
            var mockTransaction = new Mock<IDbContextTransaction>();
            mockTransaction.Setup(t => t.DisposeAsync())
                .Returns(ValueTask.CompletedTask);
                
            var transaction = new DbContextTransaction(mockTransaction.Object);
            
            // Act
            await transaction.DisposeAsync();
            
            // Assert
            mockTransaction.Verify(t => t.DisposeAsync(), Times.Once);
        }
        
        [Fact]
        public async Task AfterDispose_MethodsCantBeUsed()
        {
            // Arrange
            var mockTransaction = new Mock<IDbContextTransaction>();
            var transaction = new DbContextTransaction(mockTransaction.Object);
            
            // Act
            transaction.Dispose();
            
            // Assert
            Assert.Throws<ObjectDisposedException>(() => transaction.GetDbContextTransaction());
            await Assert.ThrowsAsync<ObjectDisposedException>(() => transaction.CommitAsync());
            await Assert.ThrowsAsync<ObjectDisposedException>(() => transaction.RollbackAsync());
        }
        
        [Fact]
        public async Task DoubleDispose_IsHandledGracefully()
        {
            // Arrange
            var mockTransaction = new Mock<IDbContextTransaction>();
            var transaction = new DbContextTransaction(mockTransaction.Object);
            
            // Act & Assert - should not throw
            transaction.Dispose();
            transaction.Dispose();
            
            await transaction.DisposeAsync();
            await transaction.DisposeAsync();
            
            // Verify Dispose only called once
            mockTransaction.Verify(t => t.Dispose(), Times.Once);
        }
    }
}