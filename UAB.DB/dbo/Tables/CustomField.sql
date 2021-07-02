CREATE TABLE [dbo].[CustomField] (
    [CustomFieldId]  INT           IDENTITY (1, 1) NOT NULL,
    [ClinicalCaseId] INT           NOT NULL,
    [Name]           VARCHAR (25)  NOT NULL,
    [Value]          VARCHAR (255) NOT NULL,
    PRIMARY KEY CLUSTERED ([CustomFieldId] ASC),
    CONSTRAINT [FKCustomFiel283120] FOREIGN KEY ([ClinicalCaseId]) REFERENCES [dbo].[ClinicalCase] ([ClinicalCaseId])
);

