CREATE TABLE [dbo].[ClinicalCaseDuplicate] (
    [ClinicalCaseDuplicateId] INT           IDENTITY (1, 1) NOT NULL,
    [ProjectId]               INT           NOT NULL,
    [ListId]                  INT           NULL,
    [PatientMRN]              VARCHAR (50)  NOT NULL,
    [PatientLastName]         VARCHAR (255) NOT NULL,
    [PatientFirstName]        VARCHAR (25)  NOT NULL,
    [DateOfService]           DATE          NOT NULL,
    [EncounterNumber]         VARCHAR (25)  NOT NULL,
    [CreatedDate]             DATETIME      NOT NULL,
    [ProviderId]              INT           NULL,
    [FileName]                VARCHAR (50)  NULL,
    [Remarks]                 VARCHAR (50)  NULL,
    CONSTRAINT [PK_ClinicalCaseDuplicate] PRIMARY KEY CLUSTERED ([ClinicalCaseDuplicateId] ASC)
);

