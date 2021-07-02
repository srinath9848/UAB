CREATE TABLE [dbo].[ProjectType] (
    [ProjectTypeId]   INT          IDENTITY (1, 1) NOT NULL,
    [ProjectTypeName] VARCHAR (50) NULL,
    [HasHeader]       BIT          NULL,
    [FileExtension]   VARCHAR (10) NULL,
    PRIMARY KEY CLUSTERED ([ProjectTypeId] ASC)
);

