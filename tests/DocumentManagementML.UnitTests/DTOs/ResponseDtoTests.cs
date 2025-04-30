// -----------------------------------------------------------------------------
// <copyright file="ResponseDtoTests.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 29, 2025
// Last Modified:      April 29, 2025
// Version:            0.9.0
// Description:        Unit tests for response DTOs
// -----------------------------------------------------------------------------
using DocumentManagementML.Application.DTOs;
using System;
using System.Collections.Generic;
using Xunit;

namespace DocumentManagementML.UnitTests.DTOs
{
    public class ResponseDtoTests
    {
        [Fact]
        public void ResponseDto_Ok_ShouldCreate_SuccessfulResponse()
        {
            // Act
            var response = ResponseDto.Ok("Success message");
            
            // Assert
            Assert.True(response.Success);
            Assert.Equal("Success message", response.Message);
            Assert.Null(response.Errors);
        }
        
        [Fact]
        public void ResponseDto_Fail_ShouldCreate_FailedResponse()
        {
            // Arrange
            var errors = new List<string> { "Error 1", "Error 2" };
            
            // Act
            var response = ResponseDto.Fail("Error message", errors);
            
            // Assert
            Assert.False(response.Success);
            Assert.Equal("Error message", response.Message);
            Assert.Equal(errors, response.Errors);
        }
        
        [Fact]
        public void ResponseDto_FromException_ShouldCreate_FailedResponse()
        {
            // Arrange
            var exception = new InvalidOperationException("Test exception");
            
            // Act - Without exception details
            var responseWithoutDetails = ResponseDto.FromException(exception);
            
            // Assert
            Assert.False(responseWithoutDetails.Success);
            Assert.Equal("An error occurred while processing the request", responseWithoutDetails.Message);
            Assert.Null(responseWithoutDetails.Errors);
            
            // Act - With exception details
            var responseWithDetails = ResponseDto.FromException(exception, true);
            
            // Assert
            Assert.False(responseWithDetails.Success);
            Assert.Equal("An error occurred while processing the request", responseWithDetails.Message);
            Assert.NotNull(responseWithDetails.Errors);
            Assert.Equal(2, responseWithDetails.Errors!.Count);
            Assert.Contains("Test exception", responseWithDetails.Errors);
        }
        
        [Fact]
        public void ResponseDtoT_Ok_ShouldCreate_SuccessfulResponseWithData()
        {
            // Arrange
            var testData = "Test data";
            
            // Act
            var response = ResponseDto<string>.Ok(testData, "Success message");
            
            // Assert
            Assert.True(response.Success);
            Assert.Equal("Success message", response.Message);
            Assert.Equal(testData, response.Data);
            Assert.Null(response.Errors);
        }
        
        [Fact]
        public void PagedResponseDto_Ok_ShouldCreate_SuccessfulPagedResponse()
        {
            // Arrange
            var testData = new List<string> { "Item 1", "Item 2" };
            
            // Act
            var response = PagedResponseDto<string>.Ok(
                testData, 
                page: 1, 
                pageSize: 2, 
                totalCount: 5, 
                message: "Success message");
            
            // Assert
            Assert.True(response.Success);
            Assert.Equal("Success message", response.Message);
            Assert.Equal(testData, response.Data);
            Assert.Equal(1, response.Page);
            Assert.Equal(2, response.PageSize);
            Assert.Equal(5, response.TotalCount);
            Assert.Equal(3, response.TotalPages);
            Assert.False(response.HasPrevious);
            Assert.True(response.HasNext);
        }
    }
}