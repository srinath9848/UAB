CREATE TABLE [dbo].[Claim] (
    [ClaimId]        INT IDENTITY (1, 1) NOT NULL,
    [ClinicalCaseId] INT NOT NULL,
    [VersionId]      INT NOT NULL
);

