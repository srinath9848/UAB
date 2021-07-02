CREATE TABLE [dbo].[BlockHistory] (
    [BlockHistoryId]     INT           IDENTITY (1, 1) NOT NULL,
    [BlockedByUserId]    INT           NULL,
    [BlockCategoryId]    INT           NULL,
    [Remarks]            VARCHAR (50)  NULL,
    [CreateDate]         DATETIME2 (7) NULL,
    [ClinicalCaseId]     INT           NULL,
    [BlockedInQueueKind] VARCHAR (50)  NULL,
    CONSTRAINT [PK_dbo.BlockHistory] PRIMARY KEY CLUSTERED ([BlockHistoryId] ASC)
);

