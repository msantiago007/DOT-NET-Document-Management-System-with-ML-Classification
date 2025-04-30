// -----------------------------------------------------------------------------
// <copyright file="UnitOfWorkTests.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 29, 2025
// Last Modified:      April 29, 2025
// Version:            0.9.0
// Description:        Unit tests for UnitOfWork implementation
// -----------------------------------------------------------------------------
using DocumentManagementML.Domain.Repositories;
using DocumentManagementML.Infrastructure.Data;
using DocumentManagementML.Infrastructure.Repositories;
using DocumentManagementML.UnitTests.TestFixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DocumentManagementML.UnitTests.Repositories
{
    public class UnitOfWorkTests : IClassFixture<TestDbContextFixture>
    {
        private readonly TestDbContextFixture _fixture;

        public UnitOfWorkTests(TestDbContextFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void UnitOfWork_ShouldProvideRepositories()
        {
            // Arrange
            using var dbContext = _fixture.CreateContext();
            var unitOfWork = new UnitOfWork(dbContext);

            // Act & Assert
            Assert.NotNull(unitOfWork.DocumentRepository);
            Assert.NotNull(unitOfWork.DocumentTypeRepository);
            Assert.NotNull(unitOfWork.UserRepository);
            Assert.NotNull(unitOfWork.DocumentMetadataRepository);
        }

        [Fact]
        public async Task UnitOfWork_ShouldManageTransaction_Successfully()
        {
            // Arrange
            using var dbContext = _fixture.CreateContext();
            var unitOfWork = new UnitOfWork(dbContext);

            // Act
            var transaction = await unitOfWork.BeginTransactionAsync();
            
            // Assert
            Assert.NotNull(transaction);
            
            // Cleanup
            await unitOfWork.RollbackTransactionAsync(transaction);
        }

        [Fact]
        public async Task UnitOfWork_ShouldCommitTransaction_Successfully()
        {
            // Arrange
            using var dbContext = _fixture.CreateContext();
            var unitOfWork = new UnitOfWork(dbContext);

            // Act
            var transaction = await unitOfWork.BeginTransactionAsync();
            await unitOfWork.CommitTransactionAsync(transaction);
            
            // Assert - if no exception was thrown, the test passes
        }

        [Fact]
        public async Task UnitOfWork_ShouldRollbackTransaction_Successfully()
        {
            // Arrange
            using var dbContext = _fixture.CreateContext();
            var unitOfWork = new UnitOfWork(dbContext);

            // Act
            var transaction = await unitOfWork.BeginTransactionAsync();
            await unitOfWork.RollbackTransactionAsync(transaction);
            
            // Assert - if no exception was thrown, the test passes
        }

        [Fact]
        public async Task UnitOfWork_ShouldThrowException_WhenCommittingNullTransaction()
        {
            // Arrange
            using var dbContext = _fixture.CreateContext();
            var unitOfWork = new UnitOfWork(dbContext);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () => 
                await unitOfWork.CommitTransactionAsync(null!));
        }

        [Fact]
        public async Task UnitOfWork_ShouldThrowException_WhenRollingBackNullTransaction()
        {
            // Arrange
            using var dbContext = _fixture.CreateContext();
            var unitOfWork = new UnitOfWork(dbContext);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () => 
                await unitOfWork.RollbackTransactionAsync(null!));
        }
    }
}