CREATE TABLE [dbo].[Version] (
    [VersionId]      INT            IDENTITY (1, 1) NOT NULL,
    [ClinicalCaseId] INT            NOT NULL,
    [VersionDate]    DATETIME2 (7)  NOT NULL,
    [UserId]         INT            NOT NULL,
    [StatusId]       INT            NOT NULL,
    [Remarks]        VARCHAR (2000) NULL,
    PRIMARY KEY CLUSTERED ([VersionId] ASC)
);

