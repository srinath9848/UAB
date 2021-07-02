CREATE TABLE [dbo].[Project] (
    [ProjectId]         INT           IDENTITY (1, 1) NOT NULL,
    [ClientId]          INT           NOT NULL,
    [Name]              VARCHAR (255) NOT NULL,
    [IsActive]          BIT           DEFAULT ('1') NOT NULL,
    [CreatedDate]       DATETIME2 (7) NULL,
    [InputFileLocation] VARCHAR (255) NULL,
    [InputFileFormat]   VARCHAR (5)   NULL,
    [ProjectTypeId]     INT           NULL,
    [SLAInDays]         INT           DEFAULT ((0)) NOT NULL,
    [TPICProjectId]     INT           DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([ProjectId] ASC),
    FOREIGN KEY ([ProjectTypeId]) REFERENCES [dbo].[ProjectType] ([ProjectTypeId]),
    CONSTRAINT [FKProject603379] FOREIGN KEY ([ClientId]) REFERENCES [dbo].[Client] ([ClientId])
);

