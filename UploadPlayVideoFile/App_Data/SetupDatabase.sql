-- Create FileUploadDB database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'FileUploadDB')
BEGIN
    CREATE DATABASE FileUploadDB;
END
GO

USE FileUploadDB;
GO

-- Create MediaFiles table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MediaFiles]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[MediaFiles](
        [mediaid] [int] IDENTITY(1,1) NOT NULL,
        [filename] [nvarchar](max) NULL,
        [filesize] [int] NULL,
        [filepath] [nvarchar](max) NULL,
        CONSTRAINT [PK_MediaFiles] PRIMARY KEY CLUSTERED ([mediaid] ASC)
    );
END
GO

-- Create or update GetMediaFiles stored procedure
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetMediaFiles]') AND type in (N'P'))
    DROP PROCEDURE [dbo].[GetMediaFiles];
GO

CREATE PROCEDURE [dbo].[GetMediaFiles]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT mediaid, filename, filesize, filepath FROM MediaFiles ORDER BY mediaid DESC;
END
GO

PRINT 'FileUploadDB setup completed successfully!';
GO
