-- SQL Script to drop all existing tables in the DocumentManagementML database
-- This script ensures a clean slate before creating the new schema

USE [DocumentManagementML];
GO

-- Disable foreign key constraints to avoid dependency errors
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';
GO

-- Drop all foreign key constraints first
DECLARE @sql NVARCHAR(MAX) = N'';
SELECT @sql = @sql + N'
ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + '.' + QUOTENAME(OBJECT_NAME(parent_object_id)) + 
' DROP CONSTRAINT ' + QUOTENAME(name) + ';'
FROM sys.foreign_keys;

EXEC sp_executesql @sql;
GO

-- Drop all tables
DECLARE @sql NVARCHAR(MAX) = N'';
SELECT @sql = @sql + N'
DROP TABLE ' + QUOTENAME(SCHEMA_NAME(schema_id)) + '.' + QUOTENAME(name) + ';'
FROM sys.tables
WHERE type = 'U'; -- User-defined tables only

EXEC sp_executesql @sql;
GO

PRINT 'All tables dropped successfully';
GO