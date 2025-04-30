# DocumentManagementML

A .NET Core document management system with intelligent document classification capabilities using ML.NET.

## Project Overview

DocumentManagementML is an on-premises document management system with the following features:

- Secure document storage and retrieval
- Document versioning
- Metadata management 
- Automated document classification through ML.NET
- Clean Architecture implementation

## Current Status

Version: 0.9.0 (Beta)

The project is in Phase 1 (Core Domain Layer) which is approximately 70% complete. We have successfully implemented:

- Core domain entities and repositories
- Basic service implementations
- Unit tests for key components
- Placeholder ML implementation

## Key Features

- **Clean Architecture**: The project follows a clean architecture pattern with clear separation of concerns.
- **Document Classification**: Uses ML.NET to classify documents based on their content.
- **Versioning**: Tracks document changes with full version history.
- **Metadata Management**: Flexible metadata system for document attributes.
- **Secure Storage**: Ensures documents are stored securely with access controls.
- **API-First Design**: RESTful API for all operations, supporting client applications.

## Architecture

The project is organized into several layers following Clean Architecture principles:

- **Domain Layer**: Core business entities and interfaces
- **Application Layer**: Business logic and services
- **Infrastructure Layer**: Technical implementations (DB, file storage, ML)
- **API Layer**: RESTful API endpoints
- **Shared Layer**: Common utilities and constants

## Setup Instructions

### Prerequisites

- .NET 9.0 SDK (Preview)
- SQL Server (for production) or SQLite (for development)
- Visual Studio 2025 or Visual Studio Code with C# extensions

### Installation

1. **Clone the repository**

```bash
git clone https://github.com/your-repository/DocumentManagementML.git
cd DocumentManagementML
```

2. **Install .NET 9.0 SDK**

On Linux:
```bash
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 9.0
```

On Windows:
```powershell
# Run PowerShell as Administrator
Invoke-WebRequest -Uri https://dot.net/v1/dotnet-install.ps1 -OutFile dotnet-install.ps1
./dotnet-install.ps1 -Channel 9.0
```

3. **Restore dependencies**

```bash
dotnet restore
```

4. **Configure the application**

Update the connection string in `src/DocumentManagementML.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DocumentManagementML;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

5. **Build the application**

```bash
dotnet build
```

6. **Run the application**

```bash
cd src/DocumentManagementML.API
dotnet run
```

7. **Verify the installation**

Navigate to `https://localhost:5001/swagger` to access the Swagger UI for API documentation.

### Development Setup

1. **Setup the database**

```bash
cd src/DocumentManagementML.API
dotnet ef database update
```

2. **Run the tests**

```bash
dotnet test
```

## Project Structure

```
DocumentManagementML/
├── src/
│   ├── DocumentManagementML.Domain/          # Core domain entities and interfaces
│   ├── DocumentManagementML.Application/     # Application services and DTOs
│   ├── DocumentManagementML.Infrastructure/  # Implementation of repositories and external services
│   ├── DocumentManagementML.API/             # API controllers and configuration
│   └── DocumentManagementML.Shared/          # Shared utilities and constants
├── tests/
│   ├── DocumentManagementML.UnitTests/       # Unit tests for the application
│   └── DocumentManagementML.IntegrationTests/ # Integration tests
├── Project Documentation/                    # Design documents and project plans
└── README.md                                 # This file
```

## Development Roadmap

1. **Phase 1**: Core Domain Layer (70% Complete)
   - Domain entities
   - Repository interfaces
   - Basic file storage
   - Unit tests

2. **Phase 2**: Application Services Layer (5% Complete)
   - CRUD operations
   - Exception handling
   - Service implementations
   - Transaction management

3. **Phase 3**: API Layer (10% Complete)
   - REST API endpoints
   - Validation
   - Error handling
   - Swagger documentation

4. **Phase 4**: ML Implementation (5% Complete)
   - Document classification
   - Text extraction
   - Model training and evaluation
   - Integration with document services

5. **Phase 5**: Deployment and Production (Planned)
   - Containerization
   - CI/CD pipeline
   - Monitoring and logging
   - Performance optimization

6. **Phase 6**: User Experience (Planned)
   - Web UI
   - Reporting
   - Workflow automation

## Contributing

The project is currently in a private development phase. If you're interested in contributing, please contact the project maintainer.

## License

This software is proprietary and confidential. Unauthorized copying or use is prohibited.

## Copyright Notice

Copyright (c) 2025 Marco Santiago
All Rights Reserved

This code is proprietary and confidential.
Unauthorized copying, distribution, or use of this code, in any form, is strictly prohibited.