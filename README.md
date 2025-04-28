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

The project is in the early development stages, with successful builds of the following components:

- Core Domain Layer
- Application Services Layer
- API Layer (initial implementation)
- Basic ML services with placeholder implementations

## Key Features

- **Clean Architecture**: The project follows a clean architecture pattern with clear separation of concerns.
- **Document Classification**: Uses ML.NET to classify documents based on their content.
- **Versioning**: Tracks document changes with full version history.
- **Metadata Management**: Flexible metadata system for document attributes.

## Architecture

The project is organized into several layers following Clean Architecture principles:

- **Domain Layer**: Core business entities and interfaces
- **Application Layer**: Business logic and services
- **Infrastructure Layer**: Technical implementations (DB, file storage, ML)
- **API Layer**: RESTful API endpoints

## Development Roadmap

1. **Phase 1**: Core Domain Layer (Complete)
   - Domain entities
   - Repository interfaces
   - Basic file storage

2. **Phase 2**: Application Services Layer (In Progress)
   - CRUD operations
   - Exception handling
   - Service implementations

3. **Phase 3**: API Layer (In Progress)
   - REST API endpoints
   - Validation
   - Error handling

4. **Phase 4**: ML Implementation (Planned)
   - Document classification
   - Text extraction
   - Model training and evaluation

## Copyright Notice

Copyright (c) 2025 Marco Santiago
All Rights Reserved

This code is proprietary and confidential.
Unauthorized copying, distribution, or use of this code, in any form, is strictly prohibited.