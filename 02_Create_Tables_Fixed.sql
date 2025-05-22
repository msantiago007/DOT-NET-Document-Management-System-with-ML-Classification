-- SQL Script to create all tables for the DocumentManagementML database
-- This script aligns with the current entity models and DbContext
-- Fixed version that handles existing tables and corrects syntax errors

USE [DocumentManagementML];
GO

-- Create Users table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[Users] (
        [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        [Username] NVARCHAR(50) NOT NULL,
        [Email] NVARCHAR(100) NOT NULL,
        [PasswordHash] NVARCHAR(MAX) NOT NULL,
        [PasswordSalt] NVARCHAR(MAX) NOT NULL,
        [FirstName] NVARCHAR(50) NULL,
        [LastName] NVARCHAR(50) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsAdmin] BIT NOT NULL DEFAULT 0,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [LastLoginDate] DATETIME2 NULL,
        [LastModifiedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );

    -- Create unique indices for Users
    CREATE UNIQUE INDEX [IX_Users_Username] ON [dbo].[Users]([Username]);
    CREATE UNIQUE INDEX [IX_Users_Email] ON [dbo].[Users]([Email]);
    
    PRINT 'Users table created';
END
ELSE
BEGIN
    PRINT 'Users table already exists';
END
GO

-- Create DocumentTypes table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DocumentTypes' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[DocumentTypes] (
        [DocumentTypeId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        [TypeName] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [LastModifiedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [IsActive] BIT NOT NULL DEFAULT 1
    );

    -- Create unique index for DocumentTypes
    CREATE UNIQUE INDEX [IX_DocumentTypes_TypeName] ON [dbo].[DocumentTypes]([TypeName]);
    
    PRINT 'DocumentTypes table created';
END
ELSE
BEGIN
    PRINT 'DocumentTypes table already exists';
END
GO

-- Create Documents table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Documents' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[Documents] (
        [DocumentId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        [DocumentName] NVARCHAR(255) NOT NULL,
        [FilePath] NVARCHAR(MAX) NOT NULL,
        [FileSize] BIGINT NOT NULL,
        [ContentType] NVARCHAR(100) NULL,
        [UploadDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [LastModifiedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [FileLocation] NVARCHAR(MAX) NOT NULL,
        [FileType] NVARCHAR(50) NOT NULL,
        [FileSizeBytes] BIGINT NOT NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedById] UNIQUEIDENTIFIER NOT NULL,
        [LastModifiedById] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [ContentHash] NVARCHAR(255) NOT NULL,
        [DocumentTypeId] UNIQUEIDENTIFIER NULL,
        [ClassificationConfidence] FLOAT NULL,
        [UploadedById] UNIQUEIDENTIFIER NULL,
        [Description] NVARCHAR(1000) NULL
    );

    -- Add foreign keys for Documents - replacing RESTRICT with NO ACTION
    ALTER TABLE [dbo].[Documents] 
    ADD CONSTRAINT [FK_Documents_DocumentTypes] 
    FOREIGN KEY ([DocumentTypeId]) REFERENCES [dbo].[DocumentTypes]([DocumentTypeId]) ON DELETE SET NULL;

    ALTER TABLE [dbo].[Documents] 
    ADD CONSTRAINT [FK_Documents_Users_UploadedBy] 
    FOREIGN KEY ([UploadedById]) REFERENCES [dbo].[Users]([Id]) ON DELETE NO ACTION;

    ALTER TABLE [dbo].[Documents] 
    ADD CONSTRAINT [FK_Documents_Users_CreatedBy] 
    FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]) ON DELETE NO ACTION;

    ALTER TABLE [dbo].[Documents] 
    ADD CONSTRAINT [FK_Documents_Users_ModifiedBy] 
    FOREIGN KEY ([LastModifiedById]) REFERENCES [dbo].[Users]([Id]) ON DELETE NO ACTION;
    
    PRINT 'Documents table created';
END
ELSE
BEGIN
    PRINT 'Documents table already exists';
END
GO

-- Create DocumentVersions table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DocumentVersions' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[DocumentVersions] (
        [VersionId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        [DocumentId] UNIQUEIDENTIFIER NOT NULL,
        [VersionNumber] INT NOT NULL,
        [FileLocation] NVARCHAR(1000) NOT NULL,
        [ContentHash] NVARCHAR(255) NOT NULL,
        [CreatedById] UNIQUEIDENTIFIER NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [IsDeleted] BIT NOT NULL DEFAULT 0
    );

    -- Add foreign keys for DocumentVersions
    ALTER TABLE [dbo].[DocumentVersions] 
    ADD CONSTRAINT [FK_DocumentVersions_Documents] 
    FOREIGN KEY ([DocumentId]) REFERENCES [dbo].[Documents]([DocumentId]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[DocumentVersions] 
    ADD CONSTRAINT [FK_DocumentVersions_Users] 
    FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);

    -- Create unique index for DocumentVersions
    CREATE UNIQUE INDEX [IX_DocumentVersions_DocumentId_VersionNumber] 
    ON [dbo].[DocumentVersions]([DocumentId], [VersionNumber]);
    
    PRINT 'DocumentVersions table created';
END
ELSE
BEGIN
    PRINT 'DocumentVersions table already exists';
END
GO

-- Create DocumentMetadata table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DocumentMetadata' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[DocumentMetadata] (
        [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        [DocumentId] UNIQUEIDENTIFIER NOT NULL,
        [MetadataKey] NVARCHAR(100) NOT NULL,
        [MetadataValue] NVARCHAR(MAX) NOT NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );

    -- Add foreign key for DocumentMetadata
    ALTER TABLE [dbo].[DocumentMetadata] 
    ADD CONSTRAINT [FK_DocumentMetadata_Documents] 
    FOREIGN KEY ([DocumentId]) REFERENCES [dbo].[Documents]([DocumentId]) ON DELETE CASCADE;

    -- Create index for faster metadata lookup
    CREATE INDEX [IX_DocumentMetadata_DocumentId] 
    ON [dbo].[DocumentMetadata]([DocumentId]);
    
    PRINT 'DocumentMetadata table created';
END
ELSE
BEGIN
    PRINT 'DocumentMetadata table already exists';
END
GO

-- Create Tags table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Tags' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[Tags] (
        [TagId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        [TagName] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedById] UNIQUEIDENTIFIER NULL
    );

    -- Create unique index for Tags
    CREATE UNIQUE INDEX [IX_Tags_TagName] 
    ON [dbo].[Tags]([TagName]);

    -- Add foreign key for Tags CreatedBy
    ALTER TABLE [dbo].[Tags] 
    ADD CONSTRAINT [FK_Tags_Users] 
    FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
    
    PRINT 'Tags table created';
END
ELSE
BEGIN
    PRINT 'Tags table already exists';
END
GO

-- Create DocumentTags join table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DocumentTags' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[DocumentTags] (
        [DocumentId] UNIQUEIDENTIFIER NOT NULL,
        [TagId] UNIQUEIDENTIFIER NOT NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedById] UNIQUEIDENTIFIER NULL,
        CONSTRAINT [PK_DocumentTags] PRIMARY KEY ([DocumentId], [TagId])
    );

    -- Add foreign keys for DocumentTags
    ALTER TABLE [dbo].[DocumentTags] 
    ADD CONSTRAINT [FK_DocumentTags_Documents] 
    FOREIGN KEY ([DocumentId]) REFERENCES [dbo].[Documents]([DocumentId]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[DocumentTags] 
    ADD CONSTRAINT [FK_DocumentTags_Tags] 
    FOREIGN KEY ([TagId]) REFERENCES [dbo].[Tags]([TagId]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[DocumentTags] 
    ADD CONSTRAINT [FK_DocumentTags_Users] 
    FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
    
    PRINT 'DocumentTags table created';
END
ELSE
BEGIN
    PRINT 'DocumentTags table already exists';
END
GO

-- Create Topics table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Topics' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[Topics] (
        [TopicId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        [TopicName] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [ParentTopicId] UNIQUEIDENTIFIER NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedById] UNIQUEIDENTIFIER NULL
    );

    -- Create unique index for Topics
    CREATE UNIQUE INDEX [IX_Topics_TopicName] 
    ON [dbo].[Topics]([TopicName]);

    -- Add foreign keys for Topics
    ALTER TABLE [dbo].[Topics] 
    ADD CONSTRAINT [FK_Topics_Topics] 
    FOREIGN KEY ([ParentTopicId]) REFERENCES [dbo].[Topics]([TopicId]);

    ALTER TABLE [dbo].[Topics] 
    ADD CONSTRAINT [FK_Topics_Users] 
    FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
    
    PRINT 'Topics table created';
END
ELSE
BEGIN
    PRINT 'Topics table already exists';
END
GO

-- Create DocumentTopics join table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DocumentTopics' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[DocumentTopics] (
        [DocumentId] UNIQUEIDENTIFIER NOT NULL,
        [TopicId] UNIQUEIDENTIFIER NOT NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedById] UNIQUEIDENTIFIER NULL,
        CONSTRAINT [PK_DocumentTopics] PRIMARY KEY ([DocumentId], [TopicId])
    );

    -- Add foreign keys for DocumentTopics
    ALTER TABLE [dbo].[DocumentTopics] 
    ADD CONSTRAINT [FK_DocumentTopics_Documents] 
    FOREIGN KEY ([DocumentId]) REFERENCES [dbo].[Documents]([DocumentId]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[DocumentTopics] 
    ADD CONSTRAINT [FK_DocumentTopics_Topics] 
    FOREIGN KEY ([TopicId]) REFERENCES [dbo].[Topics]([TopicId]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[DocumentTopics] 
    ADD CONSTRAINT [FK_DocumentTopics_Users] 
    FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
    
    PRINT 'DocumentTopics table created';
END
ELSE
BEGIN
    PRINT 'DocumentTopics table already exists';
END
GO

-- Create DocumentRelationships table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DocumentRelationships' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[DocumentRelationships] (
        [RelationshipId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        [SourceDocumentId] UNIQUEIDENTIFIER NOT NULL,
        [TargetDocumentId] UNIQUEIDENTIFIER NOT NULL,
        [RelationshipType] NVARCHAR(50) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedById] UNIQUEIDENTIFIER NOT NULL
    );

    -- Add foreign keys for DocumentRelationships
    ALTER TABLE [dbo].[DocumentRelationships] 
    ADD CONSTRAINT [FK_DocumentRelationships_SourceDocuments] 
    FOREIGN KEY ([SourceDocumentId]) REFERENCES [dbo].[Documents]([DocumentId]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[DocumentRelationships] 
    ADD CONSTRAINT [FK_DocumentRelationships_TargetDocuments] 
    FOREIGN KEY ([TargetDocumentId]) REFERENCES [dbo].[Documents]([DocumentId]);

    ALTER TABLE [dbo].[DocumentRelationships] 
    ADD CONSTRAINT [FK_DocumentRelationships_Users] 
    FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
    
    PRINT 'DocumentRelationships table created';
END
ELSE
BEGIN
    PRINT 'DocumentRelationships table already exists';
END
GO

-- Create RefreshTokens table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RefreshTokens' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[RefreshTokens] (
        [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        [UserId] UNIQUEIDENTIFIER NOT NULL,
        [Token] NVARCHAR(255) NOT NULL,
        [JwtId] NVARCHAR(255) NULL,
        [IsUsed] BIT NOT NULL DEFAULT 0,
        [IsRevoked] BIT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [ExpiresAt] DATETIME2 NOT NULL
    );

    -- Add foreign key for RefreshTokens
    ALTER TABLE [dbo].[RefreshTokens] 
    ADD CONSTRAINT [FK_RefreshTokens_Users] 
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE;

    -- Create indices for RefreshTokens
    CREATE UNIQUE INDEX [IX_RefreshTokens_Token] 
    ON [dbo].[RefreshTokens]([Token]);

    CREATE INDEX [IX_RefreshTokens_UserId] 
    ON [dbo].[RefreshTokens]([UserId]);
    
    PRINT 'RefreshTokens table created';
END
ELSE
BEGIN
    PRINT 'RefreshTokens table already exists';
END
GO

-- Create TrainingDocuments table for ML if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TrainingDocuments' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[TrainingDocuments] (
        [TrainingDocumentId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        [DocumentId] UNIQUEIDENTIFIER NULL,
        [Text] NVARCHAR(MAX) NOT NULL,
        [DocumentType] NVARCHAR(100) NOT NULL,
        [IsTrainingSet] BIT NOT NULL DEFAULT 1,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedById] UNIQUEIDENTIFIER NOT NULL
    );

    -- Add foreign keys for TrainingDocuments
    ALTER TABLE [dbo].[TrainingDocuments] 
    ADD CONSTRAINT [FK_TrainingDocuments_Documents] 
    FOREIGN KEY ([DocumentId]) REFERENCES [dbo].[Documents]([DocumentId]) ON DELETE SET NULL;

    ALTER TABLE [dbo].[TrainingDocuments] 
    ADD CONSTRAINT [FK_TrainingDocuments_Users] 
    FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
    
    PRINT 'TrainingDocuments table created';
END
ELSE
BEGIN
    PRINT 'TrainingDocuments table already exists';
END
GO

-- Create ModelMetrics table for ML model evaluation if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ModelMetrics' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[ModelMetrics] (
        [MetricsId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        [ModelVersion] NVARCHAR(50) NOT NULL,
        [MicroAccuracy] FLOAT NOT NULL,
        [MacroAccuracy] FLOAT NOT NULL,
        [LogLoss] FLOAT NOT NULL,
        [LogLossReduction] FLOAT NOT NULL,
        [TrainedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [IsActive] BIT NOT NULL DEFAULT 0,
        [TrainedById] UNIQUEIDENTIFIER NOT NULL
    );

    -- Add foreign key for ModelMetrics
    ALTER TABLE [dbo].[ModelMetrics] 
    ADD CONSTRAINT [FK_ModelMetrics_Users] 
    FOREIGN KEY ([TrainedById]) REFERENCES [dbo].[Users]([Id]);
    
    PRINT 'ModelMetrics table created';
END
ELSE
BEGIN
    PRINT 'ModelMetrics table already exists';
END
GO

-- Create ClassificationResults table for document classification history if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ClassificationResults' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[ClassificationResults] (
        [ClassificationId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        [DocumentId] UNIQUEIDENTIFIER NOT NULL,
        [DocumentTypeId] UNIQUEIDENTIFIER NOT NULL,
        [Confidence] FLOAT NOT NULL,
        [ModelVersion] NVARCHAR(50) NOT NULL,
        [ClassifiedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [IsAutomatic] BIT NOT NULL DEFAULT 1,
        [ClassifiedById] UNIQUEIDENTIFIER NULL
    );

    -- Add foreign keys for ClassificationResults
    ALTER TABLE [dbo].[ClassificationResults] 
    ADD CONSTRAINT [FK_ClassificationResults_Documents] 
    FOREIGN KEY ([DocumentId]) REFERENCES [dbo].[Documents]([DocumentId]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[ClassificationResults] 
    ADD CONSTRAINT [FK_ClassificationResults_DocumentTypes] 
    FOREIGN KEY ([DocumentTypeId]) REFERENCES [dbo].[DocumentTypes]([DocumentTypeId]);

    ALTER TABLE [dbo].[ClassificationResults] 
    ADD CONSTRAINT [FK_ClassificationResults_Users] 
    FOREIGN KEY ([ClassifiedById]) REFERENCES [dbo].[Users]([Id]);
    
    PRINT 'ClassificationResults table created';
END
ELSE
BEGIN
    PRINT 'ClassificationResults table already exists';
END
GO

-- Create DocumentTypeScores table for detailed classification scores if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DocumentTypeScores' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[DocumentTypeScores] (
        [ScoreId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        [ClassificationId] UNIQUEIDENTIFIER NOT NULL,
        [DocumentTypeId] UNIQUEIDENTIFIER NOT NULL,
        [Score] FLOAT NOT NULL
    );

    -- Add foreign keys for DocumentTypeScores
    ALTER TABLE [dbo].[DocumentTypeScores] 
    ADD CONSTRAINT [FK_DocumentTypeScores_ClassificationResults] 
    FOREIGN KEY ([ClassificationId]) REFERENCES [dbo].[ClassificationResults]([ClassificationId]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[DocumentTypeScores] 
    ADD CONSTRAINT [FK_DocumentTypeScores_DocumentTypes] 
    FOREIGN KEY ([DocumentTypeId]) REFERENCES [dbo].[DocumentTypes]([DocumentTypeId]);
    
    PRINT 'DocumentTypeScores table created';
END
ELSE
BEGIN
    PRINT 'DocumentTypeScores table already exists';
END
GO

PRINT 'All tables created or already exist';
GO