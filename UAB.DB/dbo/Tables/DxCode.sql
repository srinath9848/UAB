CREATE TABLE [dbo].[DxCode] (
    [DxCodeId]       INT          IDENTITY (1, 1) NOT NULL,
    [ClinicalCaseId] INT          NOT NULL,
    [VersionId]      INT          NOT NULL,
    [DxCode]         VARCHAR (10) NOT NULL,
    [ClaimId]        INT          NULL,
    PRIMARY KEY CLUSTERED ([DxCodeId] ASC)
);

