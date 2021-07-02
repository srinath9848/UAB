CREATE TABLE [dbo].[Status] (
    [StatusId]    INT           IDENTITY (1, 1) NOT NULL,
    [Name]        VARCHAR (25)  NOT NULL,
    [Description] VARCHAR (255) NOT NULL,
    PRIMARY KEY CLUSTERED ([StatusId] ASC)
);

