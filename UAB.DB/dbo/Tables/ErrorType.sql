CREATE TABLE [dbo].[ErrorType] (
    [ErrorTypeId] INT          IDENTITY (1, 1) NOT NULL,
    [Name]        VARCHAR (50) NOT NULL,
    PRIMARY KEY CLUSTERED ([ErrorTypeId] ASC)
);

