-- SQL Script to seed initial data for the DocumentManagementML database
-- This script adds essential data including admin user and document types

USE [DocumentManagementML];
GO

-- Clear any existing seed data to prevent duplicates
DELETE FROM [dbo].[RefreshTokens];
DELETE FROM [dbo].[DocumentTypeScores];
DELETE FROM [dbo].[ClassificationResults];
DELETE FROM [dbo].[ModelMetrics];
DELETE FROM [dbo].[TrainingDocuments];
DELETE FROM [dbo].[DocumentRelationships];
DELETE FROM [dbo].[DocumentTopics];
DELETE FROM [dbo].[DocumentTags];
DELETE FROM [dbo].[DocumentMetadata];
DELETE FROM [dbo].[DocumentVersions];
DELETE FROM [dbo].[Documents];
DELETE FROM [dbo].[Tags];
DELETE FROM [dbo].[Topics];
DELETE FROM [dbo].[DocumentTypes];
DELETE FROM [dbo].[Users];
GO

-- Seed admin user
-- Password: Admin@123 (hashed with PBKDF2)
-- Note: In a real system, use proper password hashing with salt
DECLARE @AdminId UNIQUEIDENTIFIER = NEWID();
INSERT INTO [dbo].[Users] (
    [Id], 
    [Username], 
    [Email], 
    [PasswordHash], 
    [PasswordSalt], 
    [FirstName], 
    [LastName], 
    [IsActive], 
    [IsAdmin], 
    [CreatedDate], 
    [LastModifiedDate]
) 
VALUES (
    @AdminId, 
    'admin', 
    'admin@documentmanagement.com', 
    -- This is a dummy hash, the actual application will use proper PBKDF2 hashing
    'AQAAAAEAACcQAAAAEHxA9wOgANSWe4KK3fcabK5DZc3NC2vL0y0XRGc0a9KnH+VFb+XoLURkDmVHlRNrIA==', 
    'fdfj8nNBzS4GR5bCqEOQNu8a7is0siYw', 
    'System', 
    'Administrator', 
    1, -- IsActive
    1, -- IsAdmin
    GETUTCDATE(), 
    GETUTCDATE()
);
PRINT 'Admin user created with ID: ' + CAST(@AdminId AS NVARCHAR(50));

-- Seed standard user for testing
DECLARE @UserId UNIQUEIDENTIFIER = NEWID();
INSERT INTO [dbo].[Users] (
    [Id], 
    [Username], 
    [Email], 
    [PasswordHash], 
    [PasswordSalt], 
    [FirstName], 
    [LastName], 
    [IsActive], 
    [IsAdmin], 
    [CreatedDate], 
    [LastModifiedDate]
) 
VALUES (
    @UserId, 
    'user', 
    'user@documentmanagement.com', 
    -- This is a dummy hash, the actual application will use proper PBKDF2 hashing
    'AQAAAAEAACcQAAAAEHxA9wOgANSWe4KK3fcabK5DZc3NC2vL0y0XRGc0a9KnH+VFb+XoLURkDmVHlRNrIA==', 
    'fdfj8nNBzS4GR5bCqEOQNu8a7is0siYw', 
    'Standard', 
    'User', 
    1, -- IsActive
    0, -- IsAdmin
    GETUTCDATE(), 
    GETUTCDATE()
);
PRINT 'Standard user created with ID: ' + CAST(@UserId AS NVARCHAR(50));

-- Seed common document types
DECLARE @InvoiceTypeId UNIQUEIDENTIFIER = NEWID();
DECLARE @ContractTypeId UNIQUEIDENTIFIER = NEWID();
DECLARE @ReportTypeId UNIQUEIDENTIFIER = NEWID();
DECLARE @LetterTypeId UNIQUEIDENTIFIER = NEWID();
DECLARE @FormTypeId UNIQUEIDENTIFIER = NEWID();
DECLARE @PresentationTypeId UNIQUEIDENTIFIER = NEWID();
DECLARE @SpreadsheetTypeId UNIQUEIDENTIFIER = NEWID();

-- Insert document types
INSERT INTO [dbo].[DocumentTypes] ([DocumentTypeId], [TypeName], [Description], [CreatedDate], [LastModifiedDate], [IsActive])
VALUES 
    (@InvoiceTypeId, 'Invoice', 'Financial invoice documents', GETUTCDATE(), GETUTCDATE(), 1),
    (@ContractTypeId, 'Contract', 'Legal contract documents', GETUTCDATE(), GETUTCDATE(), 1),
    (@ReportTypeId, 'Report', 'Business and technical reports', GETUTCDATE(), GETUTCDATE(), 1),
    (@LetterTypeId, 'Letter', 'Correspondence and formal letters', GETUTCDATE(), GETUTCDATE(), 1),
    (@FormTypeId, 'Form', 'Standard and custom forms', GETUTCDATE(), GETUTCDATE(), 1),
    (@PresentationTypeId, 'Presentation', 'Slide decks and presentations', GETUTCDATE(), GETUTCDATE(), 1),
    (@SpreadsheetTypeId, 'Spreadsheet', 'Financial and data spreadsheets', GETUTCDATE(), GETUTCDATE(), 1);

PRINT 'Document types created successfully';

-- Seed common tags
INSERT INTO [dbo].[Tags] ([TagId], [TagName], [Description], [CreatedDate], [CreatedById])
VALUES 
    (NEWID(), 'Important', 'High priority documents', GETUTCDATE(), @AdminId),
    (NEWID(), 'Confidential', 'Documents with restricted access', GETUTCDATE(), @AdminId),
    (NEWID(), 'Draft', 'Documents in draft status', GETUTCDATE(), @AdminId),
    (NEWID(), 'Final', 'Finalized documents', GETUTCDATE(), @AdminId),
    (NEWID(), 'Archived', 'Documents moved to archive', GETUTCDATE(), @AdminId),
    (NEWID(), 'Financial', 'Finance-related documents', GETUTCDATE(), @AdminId),
    (NEWID(), 'Legal', 'Legal-related documents', GETUTCDATE(), @AdminId),
    (NEWID(), 'HR', 'Human resources documents', GETUTCDATE(), @AdminId),
    (NEWID(), 'Marketing', 'Marketing materials', GETUTCDATE(), @AdminId),
    (NEWID(), 'Technical', 'Technical documentation', GETUTCDATE(), @AdminId);

PRINT 'Tags created successfully';

-- Seed main topics
DECLARE @BusinessTopicId UNIQUEIDENTIFIER = NEWID();
DECLARE @LegalTopicId UNIQUEIDENTIFIER = NEWID();
DECLARE @FinanceTopicId UNIQUEIDENTIFIER = NEWID();
DECLARE @HRTopicId UNIQUEIDENTIFIER = NEWID();

INSERT INTO [dbo].[Topics] ([TopicId], [TopicName], [Description], [ParentTopicId], [CreatedDate], [CreatedById])
VALUES 
    (@BusinessTopicId, 'Business', 'Business documents and materials', NULL, GETUTCDATE(), @AdminId),
    (@LegalTopicId, 'Legal', 'Legal documents and contracts', NULL, GETUTCDATE(), @AdminId),
    (@FinanceTopicId, 'Finance', 'Financial reports and documents', NULL, GETUTCDATE(), @AdminId),
    (@HRTopicId, 'Human Resources', 'HR policies and employee documents', NULL, GETUTCDATE(), @AdminId);

-- Seed subtopics
INSERT INTO [dbo].[Topics] ([TopicId], [TopicName], [Description], [ParentTopicId], [CreatedDate], [CreatedById])
VALUES 
    (NEWID(), 'Marketing', 'Marketing materials and campaigns', @BusinessTopicId, GETUTCDATE(), @AdminId),
    (NEWID(), 'Sales', 'Sales reports and customer information', @BusinessTopicId, GETUTCDATE(), @AdminId),
    (NEWID(), 'Contracts', 'Legal contracts and agreements', @LegalTopicId, GETUTCDATE(), @AdminId),
    (NEWID(), 'Compliance', 'Regulatory compliance documents', @LegalTopicId, GETUTCDATE(), @AdminId),
    (NEWID(), 'Invoices', 'Customer and vendor invoices', @FinanceTopicId, GETUTCDATE(), @AdminId),
    (NEWID(), 'Reports', 'Financial reports and statements', @FinanceTopicId, GETUTCDATE(), @AdminId),
    (NEWID(), 'Policies', 'HR policies and procedures', @HRTopicId, GETUTCDATE(), @AdminId),
    (NEWID(), 'Employee Records', 'Employee information and records', @HRTopicId, GETUTCDATE(), @AdminId);

PRINT 'Topics created successfully';

-- Create dummy training documents for ML model
INSERT INTO [dbo].[TrainingDocuments] (
    [TrainingDocumentId], 
    [DocumentId], 
    [Text], 
    [DocumentType], 
    [IsTrainingSet], 
    [CreatedDate], 
    [CreatedById]
)
VALUES 
    (NEWID(), NULL, 'Invoice #12345 for services rendered. Total amount due: $1,250.00. Payment due within 30 days.', 'Invoice', 1, GETUTCDATE(), @AdminId),
    (NEWID(), NULL, 'This contract agreement is made between Company A and Company B for the provision of consulting services...', 'Contract', 1, GETUTCDATE(), @AdminId),
    (NEWID(), NULL, 'Quarterly Business Report Q2 2025. Executive Summary: Revenue increased by 15% compared to previous quarter...', 'Report', 1, GETUTCDATE(), @AdminId),
    (NEWID(), NULL, 'Dear Sir/Madam, I am writing to express my interest in the position advertised on your website...', 'Letter', 1, GETUTCDATE(), @AdminId),
    (NEWID(), NULL, 'Employee Information Form. Name: _______. Address: _______. Phone: _______. Emergency Contact: _______.', 'Form', 1, GETUTCDATE(), @AdminId),
    (NEWID(), NULL, 'Quarterly Sales Presentation. Slide 1: Introduction. Slide 2: Market Analysis. Slide 3: Sales Figures...', 'Presentation', 1, GETUTCDATE(), @AdminId),
    (NEWID(), NULL, 'Budget Forecast 2025. Q1: $125,000. Q2: $143,000. Q3: $156,000. Q4: $178,000. Total: $602,000.', 'Spreadsheet', 1, GETUTCDATE(), @AdminId);

-- Create initial model metrics record
INSERT INTO [dbo].[ModelMetrics] (
    [MetricsId], 
    [ModelVersion], 
    [MicroAccuracy], 
    [MacroAccuracy], 
    [LogLoss], 
    [LogLossReduction], 
    [TrainedDate], 
    [IsActive], 
    [TrainedById]
)
VALUES (
    NEWID(), 
    '0.1.0', 
    0.85, -- Initial accuracy estimate
    0.82, 
    0.42, 
    0.65, 
    GETUTCDATE(), 
    1, -- This is the active model
    @AdminId
);

PRINT 'Initial ML training data and metrics created successfully';
PRINT 'Seed data created successfully';
GO