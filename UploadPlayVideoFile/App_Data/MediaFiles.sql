CREATE TABLE [dbo].[MediaFiles](
	[mediaid] [int] IDENTITY(1,1) NOT NULL,
	[filename] [nvarchar](max) NULL,
	[filesize] [int] NULL,
	[filepath] [nvarchar](max) NULL,
 CONSTRAINT [PK_MediaFiles] PRIMARY KEY CLUSTERED 
(
	[mediaid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

-- Stored procedure to get all media files
CREATE PROCEDURE [dbo].[GetMediaFiles]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT mediaid, filename, filesize, filepath FROM MediaFiles ORDER BY mediaid DESC;
END
GO
