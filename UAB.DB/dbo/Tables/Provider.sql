CREATE TABLE [dbo].[Provider] (
    [ProviderID] INT           IDENTITY (1, 1) NOT NULL,
    [Name]       VARCHAR (100) NULL,
    [ProjectID]  INT           NULL,
    PRIMARY KEY CLUSTERED ([ProviderID] ASC),
    FOREIGN KEY ([ProjectID]) REFERENCES [dbo].[Project] ([ProjectId])
);

