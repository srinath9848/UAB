CREATE TABLE [dbo].[ProviderPosted] (
    [ProviderPostedId] INT           IDENTITY (1, 1) NOT NULL,
    [PostingDate]      DATETIME2 (7) NULL,
    [CoderComment]     VARCHAR (500) NULL,
    [ClinicalCaseId]   INT           NULL,
    [ProviderId]       INT           NULL,
    PRIMARY KEY CLUSTERED ([ProviderPostedId] ASC),
    FOREIGN KEY ([ClinicalCaseId]) REFERENCES [dbo].[ClinicalCase] ([ClinicalCaseId])
);

