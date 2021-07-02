CREATE TABLE [dbo].[BlockResponse] (
    [BlockResponseId]  INT           IDENTITY (1, 1) NOT NULL,
    [ClinicalCaseId]   INT           NULL,
    [ResponseRemarks]  VARCHAR (100) NULL,
    [ResponseByUserId] INT           NULL,
    [ResponseDate]     DATETIME2 (7) NULL,
    CONSTRAINT [PK_dbo.BlockResponse] PRIMARY KEY CLUSTERED ([BlockResponseId] ASC)
);

