CREATE TABLE [dbo].[CptCode] (
    [CptCodeId]      INT          IDENTITY (1, 1) NOT NULL,
    [ClinicalCaseId] INT          NOT NULL,
    [VersionId]      INT          NOT NULL,
    [CPTCode]        VARCHAR (5)  NOT NULL,
    [Modifier]       VARCHAR (25) NULL,
    [Qty]            VARCHAR (25) NULL,
    [Links]          VARCHAR (25) NULL,
    [ClaimId]        INT          NULL,
    PRIMARY KEY CLUSTERED ([CptCodeId] ASC)
);

