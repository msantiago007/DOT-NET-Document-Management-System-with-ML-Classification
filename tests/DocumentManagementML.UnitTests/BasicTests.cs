// -----------------------------------------------------------------------------
// <copyright file="BasicTests.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 30, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        Basic unit tests to verify test framework functionality
// -----------------------------------------------------------------------------

using DocumentManagementML.Domain.Entities;
using System;
using Xunit;

namespace DocumentManagementML.UnitTests
{
    /// <summary>
    /// Contains basic tests to verify that the test framework is functioning correctly.
    /// </summary>
    public class BasicTests
    {
        [Fact]
        public void SimplePassingTest()
        {
            // This test should always pass
            Assert.True(true);
        }
        
        [Fact]
        public void DocumentType_Constructor_InitializesProperties()
        {
            // Arrange & Act
            var documentType = new DocumentType();
            
            // Assert
            Assert.Equal(Guid.Empty, documentType.DocumentTypeId);
            Assert.Equal(string.Empty, documentType.Name);
            Assert.Equal(string.Empty, documentType.TypeName);
            Assert.Equal(string.Empty, documentType.SchemaDefinition);
            Assert.True(documentType.IsActive);
        }
    }
}