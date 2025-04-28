# Test Documentation for DocumentManagementML

This document outlines the unit tests implemented in the project, what functionality they test, and how to run them manually.

## Overview of Implemented Tests

The project contains the following unit tests:

### Entity Tests

1. **DocumentTypeTests**
   - Located in: `tests/DocumentManagementML.UnitTests/Entities/DocumentTypeTests.cs`
   - Tests the `DocumentType` entity class
   - Verifies proper initialization and property setting

2. **DocumentClassificationResultTests**
   - Located in: `tests/DocumentManagementML.UnitTests/Entities/DocumentClassificationResultTests.cs`
   - Tests the `DocumentClassificationResult` entity class, including the new IsSuccessful property
   - Verifies property assignment and default initialization

### Service Tests

1. **DocumentTypeServiceTests**
   - Located in: `tests/DocumentManagementML.UnitTests/Services/DocumentTypeServiceTests.cs`
   - Tests the `DocumentTypeService` implementation using mocks
   - Verifies TypeName property handling during creation and updates

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

## Adding New Tests

As you develop the system further, you should continue adding tests for new components:

1. For new entities, add tests that verify default initialization and property setting
2. For services, test both success scenarios and edge cases/error conditions
3. Use mocking with Moq to isolate the component being tested

This approach ensures that existing functionality remains working as you add new features and fix bugs.