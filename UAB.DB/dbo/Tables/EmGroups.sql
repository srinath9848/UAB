CREATE TABLE [dbo].[EmGroups] (
    [EmgroupId] INT          IDENTITY (1, 1) NOT NULL,
    [Name]      VARCHAR (50) NULL,
    CONSTRAINT [PK_dbo.EmGroups] PRIMARY KEY CLUSTERED ([EmgroupId] ASC)
);

