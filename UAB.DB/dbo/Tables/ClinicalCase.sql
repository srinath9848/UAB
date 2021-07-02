CREATE TABLE [dbo].[ClinicalCase] (
    [ClinicalCaseId]   INT           IDENTITY (1, 1) NOT NULL,
    [ProjectId]        INT           NOT NULL,
    [ListId]           BIGINT        NULL,
    [PatientMRN]       VARCHAR (50)  NOT NULL,
    [PatientLastName]  VARCHAR (255) NOT NULL,
    [PatientFirstName] VARCHAR (25)  NOT NULL,
    [DateOfService]    DATE          NOT NULL,
    [EncounterNumber]  VARCHAR (25)  NOT NULL,
    [CreatedDate]      DATETIME      NOT NULL,
    [ProviderId]       INT           NULL,
    [FileName]         VARCHAR (50)  NULL,
    CONSTRAINT [PK__Clinical__84F8DED7862C1110] PRIMARY KEY CLUSTERED ([ClinicalCaseId] ASC),
    CONSTRAINT [FKClinicalCa234939] FOREIGN KEY ([ListId]) REFERENCES [dbo].[List] ([ListId]),
    CONSTRAINT [FKClinicalCa331303] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[Project] ([ProjectId])
);


GO
CREATE NONCLUSTERED INDEX [IX_ClinicalCase_DateOfService]
    ON [dbo].[ClinicalCase]([DateOfService] ASC);

