# Test Documentation for DocumentManagementML

This document outlines the unit tests and integration tests implemented in the project, what functionality they test, and how to run them manually.

## Overview of Implemented Tests

The project contains the following tests:

### Unit Tests

#### Entity Tests

1. **DocumentTypeTests**
   - Located in: `tests/DocumentManagementML.UnitTests/Entities/DocumentTypeTests.cs`
   - Tests the `DocumentType` entity class
   - Verifies proper initialization and property setting

2. **DocumentClassificationResultTests**
   - Located in: `tests/DocumentManagementML.UnitTests/Entities/DocumentClassificationResultTests.cs`
   - Tests the `DocumentClassificationResult` entity class, including the new IsSuccessful property
   - Verifies property assignment and default initialization

#### Service Tests

1. **DocumentTypeServiceTests**
   - Located in: `tests/DocumentManagementML.UnitTests/Services/DocumentTypeServiceTests.cs`
   - Tests the `DocumentTypeService` implementation using mocks
   - Verifies TypeName property handling during creation and updates

2. **EnhancedDocumentTypeServiceTests**
   - Located in: `tests/DocumentManagementML.UnitTests/Services/EnhancedDocumentTypeServiceTests.cs`
   - Tests the enhanced document type service with transaction support
   - Verifies proper transaction handling and validation
   - Tests all CRUD operations with various scenarios

3. **EnhancedDocumentServiceTests**
   - Located in: `tests/DocumentManagementML.UnitTests/Services/EnhancedDocumentServiceTests.cs`
   - Tests the enhanced document service with transaction support
   - Verifies document management operations and validation
   - Tests document metadata and file handling

4. **EnhancedUserServiceTests**
   - Located in: `tests/DocumentManagementML.UnitTests/Services/EnhancedUserServiceTests.cs`
   - Tests the enhanced user service with comprehensive validation
   - Verifies authentication, password management, and CRUD operations
   - Tests transaction handling for critical operations

## Detailed Test Descriptions

### DocumentTypeTests

1. **DocumentType_Constructor_SetsDefaultProperties**
   - **Purpose**: Verifies that a new DocumentType instance has correct default values
   - **What it tests**: Initial property values after constructor runs
   - **Expected behavior**: Properties should be initialized to empty strings or default values

2. **DocumentType_SetProperties_PropertiesAreSet**
   - **Purpose**: Verifies that properties can be properly set on a DocumentType instance
   - **What it tests**: Ability to set and retrieve properties
   - **Expected behavior**: Properties should retain the values they were set to

### DocumentClassificationResultTests

1. **IsSuccessful_IsAliasForSuccess_ReturnsSameValue**
   - **Purpose**: Verifies that the new IsSuccessful property is an alias for the Success property
   - **What it tests**: The implementation of the IsSuccessful property as a get/set accessor for Success
   - **Expected behavior**: Both properties should always return the same value and changing one changes the other

2. **DocumentClassificationResult_DefaultProperties_AreInitializedCorrectly**
   - **Purpose**: Verifies that a new DocumentClassificationResult instance has correct default values
   - **What it tests**: Initial property values after constructor runs
   - **Expected behavior**: Properties should be initialized to their default values

3. **DocumentClassificationResult_WithValues_StoresCorrectly**
   - **Purpose**: Verifies that properties can be properly set on a DocumentClassificationResult instance
   - **What it tests**: Ability to set and retrieve properties
   - **Expected behavior**: Properties should retain the values they were set to

### DocumentTypeServiceTests

1. **CreateDocumentTypeAsync_SetsTypeName_BasedOnName**
   - **Purpose**: Verifies that the TypeName property is properly set during document type creation
   - **What it tests**: The service's logic for generating a TypeName from a Name value
   - **Expected behavior**: The service should convert spaces and set the TypeName to lowercase

2. **UpdateDocumentTypeAsync_UpdatesTypeName_WhenNameChanges**
   - **Purpose**: Verifies that the TypeName property is updated when the Name property changes
   - **What it tests**: The service's update logic for synchronizing TypeName with Name
   - **Expected behavior**: The TypeName should be updated to match the new Name (with spaces removed and lowercase)

## How to Run Tests Manually

You can run the tests manually using the .NET CLI. Here are the commands to run different sets of tests:

### Run All Tests

```bash
export PATH=$HOME/.dotnet:$PATH
cd /home/administrator/nodejs/DocumentManagementML
dotnet test
```

### Run Only Entity Tests

```bash
export PATH=$HOME/.dotnet:$PATH
cd /home/administrator/nodejs/DocumentManagementML
dotnet test --filter Entities
```

### Run Only Service Tests

```bash
export PATH=$HOME/.dotnet:$PATH
cd /home/administrator/nodejs/DocumentManagementML
dotnet test --filter Services
```

### Run a Specific Test Class

```bash
export PATH=$HOME/.dotnet:$PATH
cd /home/administrator/nodejs/DocumentManagementML
dotnet test --filter ClassName=DocumentManagementML.UnitTests.Entities.DocumentTypeTests
```

### Run a Specific Test Method

```bash
export PATH=$HOME/.dotnet:$PATH
cd /home/administrator/nodejs/DocumentManagementML
dotnet test --filter Name=DocumentManagementML.UnitTests.Entities.DocumentTypeTests.DocumentType_Constructor_SetsDefaultProperties
```

## Understanding Test Results

When you run tests, the output will show you:

1. Which tests passed/failed
2. For failed tests, details about the assertion that failed
3. The line number where the failure occurred

### Example Output for Passing Tests:

```
Test run for /home/administrator/nodejs/DocumentManagementML/tests/DocumentManagementML.UnitTests/bin/Debug/net9.0/DocumentManagementML.UnitTests.dll (.NETCoreApp,Version=v9.0)
Microsoft (R) Test Execution Command Line Tool Version 17.11.0 (x64)
Copyright (c) Microsoft Corporation.  All rights reserved.

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:     2, Skipped:     0, Total:     2, Duration: < 1 ms
```

### Example Output for Failing Tests:

```
Test run for /home/administrator/nodejs/DocumentManagementML/tests/DocumentManagementML.UnitTests/bin/Debug/net9.0/DocumentManagementML.UnitTests.dll (.NETCoreApp,Version=v9.0)
Microsoft (R) Test Execution Command Line Tool Version 17.11.0 (x64)
Copyright (c) Microsoft Corporation.  All rights reserved.

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

[xUnit.net 00:00:00.36]     DocumentManagementML.UnitTests.Entities.DocumentTypeTests.DocumentType_Constructor_SetsDefaultProperties [FAIL]
  Failed DocumentManagementML.UnitTests.Entities.DocumentTypeTests.DocumentType_Constructor_SetsDefaultProperties [< 1 ms]
  Error Message:
   Assert.Equal() Failure
          Expected: Empty
          Actual:   00000000-0000-0000-0000-000000000000

Failed!  - Failed:     1, Passed:     1, Skipped:     0, Total:     2, Duration: < 1 ms
```

### Integration Tests

1. **ApiIntegrationTests**
   - Located in: `tests/DocumentManagementML.IntegrationTests/UnitTest1.cs`
   - Tests basic API connectivity
   - Verifies that the API is accessible and returns successful responses

2. **ApiControllerTests**
   - Located in: `tests/DocumentManagementML.IntegrationTests/ApiControllerTests.cs`
   - Tests various API endpoints including authenticated and unauthenticated requests
   - Verifies authentication flow and basic controller functionality

3. **MLControllerTests**
   - Located in: `tests/DocumentManagementML.IntegrationTests/Controllers/MLControllerTests.cs`
   - Tests machine learning endpoints including model metrics, classification, training, and evaluation
   - Verifies authentication, authorization, and validation for ML operations
   - Tests different error scenarios and edge cases with invalid inputs
   - Validates role-based access for admin-only operations

4. **EnhancedMLControllerTests**
   - Located in: `tests/DocumentManagementML.IntegrationTests/Controllers/EnhancedMLControllerTests.cs`
   - Tests the enhanced ML controller with standardized response format
   - Validates proper response structure with consistent success/error patterns
   - Tests authorization with regular and admin users
   - Verifies error handling with invalid inputs

5. **AuthControllerTests**
   - Located in: `tests/DocumentManagementML.IntegrationTests/Controllers/AuthControllerTests.cs`
   - Comprehensive tests for authentication and authorization
   - Tests user registration, login, token refresh, and user information retrieval
   - Validates token management, including refresh token functionality
   - Tests password change operations and security enforcement
   - Verifies proper error handling for invalid credentials and security violations

6. **JwtAuthenticationTests**
   - Located in: `tests/DocumentManagementML.IntegrationTests/Controllers/JwtAuthenticationTests.cs`
   - Comprehensive tests specifically for JWT authentication flows
   - Tests token generation, validation, and refresh workflows
   - Verifies token expiration, invalidation, and security
   - Tests password change functionality and its effect on authentication
   - Validates refresh token functionality and single-use security

7. **RoleBasedAuthorizationTests**
   - Located in: `tests/DocumentManagementML.IntegrationTests/Controllers/RoleBasedAuthorizationTests.cs`
   - Tests role-based authorization across different API endpoints
   - Verifies that admin users can access admin-only resources
   - Tests that regular users are denied access to admin-only endpoints
   - Validates that unauthenticated users are rejected from protected endpoints
   - Tests that expired or invalid tokens are properly rejected
   - Verifies that token tampering is detected and rejected

8. **DocumentsControllerTests** and **DocumentTypesControllerTests**
   - Located in: `tests/DocumentManagementML.IntegrationTests/Controllers/DocumentsControllerTests.cs` and 
     `tests/DocumentManagementML.IntegrationTests/Controllers/DocumentTypesControllerTests.cs`
   - Tests CRUD operations for documents and document types
   - Validates document classification, upload, and metadata handling
   - Tests document type management and relationship with documents
   - Verifies proper error handling and validation for operations

## Running Integration Tests

You can run the integration tests using the .NET CLI:

### Run All Integration Tests

```bash
export PATH=$HOME/.dotnet:$PATH
cd /home/administrator/nodejs/DocumentManagementML
dotnet test tests/DocumentManagementML.IntegrationTests
```

### Run a Specific Integration Test

```bash
export PATH=$HOME/.dotnet:$PATH
cd /home/administrator/nodejs/DocumentManagementML
dotnet test tests/DocumentManagementML.IntegrationTests --filter "FullyQualifiedName=DocumentManagementML.IntegrationTests.ApiIntegrationTests.Api_IsAccessible"
```

## Transaction Testing

We've implemented comprehensive transaction testing to ensure data integrity across the application:

### Repository-Level Transaction Tests

Located in: `tests/DocumentManagementML.UnitTests/Integration/TransactionHandlingTests.cs`

These tests verify the basic functionality of transactions at the repository level:
- `Transaction_CommittedAcrossRepositories_ChangesArePersisted`: Tests that changes are saved when a transaction is committed
- `Transaction_RolledBackAcrossRepositories_ChangesAreDiscarded`: Tests that changes are discarded when a transaction is rolled back
- `Transaction_ThrowsExceptionDuringOperation_ChangesAreRolledBack`: Tests that changes are rolled back when an exception occurs

### UnitOfWork Transaction Tests

Located in: `tests/DocumentManagementML.UnitTests/Repositories/UnitOfWorkTests.cs`

These tests verify the UnitOfWork implementation handles transactions properly:
- `UnitOfWork_ShouldManageTransaction_Successfully`: Tests transaction creation
- `UnitOfWork_ShouldCommitTransaction_Successfully`: Tests transaction commit
- `UnitOfWork_ShouldRollbackTransaction_Successfully`: Tests transaction rollback

### Cross-Service Transaction Tests

Located in: `tests/DocumentManagementML.UnitTests/Integration/CrossServiceTransactionTests.cs`

These tests verify transaction handling across multiple services:
- `CrossService_CreateDocumentTypeAndDocument_CommitsSuccessfully`: Tests successful transactions spanning multiple services
- `CrossService_ServiceFailureAfterSuccess_RollsBackAllChanges`: Tests rollback when a service fails
- `CrossService_ExplicitTransaction_RollsBackAllChanges`: Tests explicit transaction management
- `CrossService_RelatedEntities_CreateAndDelete_MaintainsConsistency`: Tests referential integrity with transactions
- `UnitOfWork_NestedTransactions_HandledCorrectly`: Tests nested transaction handling

### API-Level Transaction Tests

Located in: `tests/DocumentManagementML.IntegrationTests/Integration/TransactionHandlingTests.cs`

These tests verify transaction handling at the API level:
- `DocumentUploadWithMetadata_CommitsTransaction`: Tests document creation with metadata in a transaction
- `DocumentUpdate_WithInvalidData_RollsBackTransaction`: Tests transaction rollback when validation fails
- `CreateDocumentTypeWithDocuments_CommitsTransaction`: Tests creating a document type and documents in a transaction
- `BulkDocumentOperations_HandlesTransactionsCorrectly`: Tests bulk operations with transaction handling

## Integration Test Environment

Our integration tests use:

- WebApplicationFactory<MyAppProgram> for hosting the API
- In-memory database for persistence
- Seeded test data (users, document types, documents)
- Authentication helpers for testing secured endpoints
- Transaction management for data integrity

### Known Issues with WebApplicationFactory

We are currently experiencing some challenges with the WebApplicationFactory setup in .NET 9.0:

1. **ASP.NET Core 9.0 Compatibility**: The current WebApplicationFactory approach is having difficulty with the minimal API approach used in ASP.NET Core 9.0. This results in errors like "The entry point exited without ever building an IHost."

2. **Workaround Attempts**:
   - Implemented a custom MyAppProgram class that mimics the API's Program.cs structure
   - Created a TestStartup class for configuring test services
   - Added explicit CreateHostBuilder method to facilitate WebApplicationFactory initialization
   - Tried various configuration approaches for the test host

3. **Current Status**: Test cases are implemented but not running due to WebApplicationFactory setup issues. These issues need to be resolved before the integration tests can run successfully.

4. **Next Steps**:
   - Research better approaches for WebApplicationFactory with ASP.NET Core 9.0
   - Consider using the actual Program class with direct configuration
   - Explore Microsoft's recommended approaches for testing minimal APIs in .NET 9.0

## Adding New Tests

As you develop the system further, you should continue adding tests for new components:

1. For new entities, add unit tests that verify default initialization and property setting
2. For services, add unit tests for both success scenarios and edge cases/error conditions using Moq
3. For API controllers, add integration tests that verify the entire request/response flow
4. For transaction-heavy operations, add integration tests that verify data consistency

This approach ensures that existing functionality remains working as you add new features and fix bugs.