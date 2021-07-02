CREATE TABLE [dbo].[WorkItemProvider] (
    [WorkItemProviderId] INT IDENTITY (1, 1) NOT NULL,
    [ClinicalCaseId]     INT NOT NULL,
    [VersionId]          INT NOT NULL,
    [ProviderId]         INT NOT NULL,
    [PayorId]            INT NOT NULL,
    [ProviderFeedbackId] INT NULL,
    [ClaimId]            INT NULL,
    CONSTRAINT [PK__WorkItem__203D6328CDBCB3BD] PRIMARY KEY CLUSTERED ([WorkItemProviderId] ASC)
);

