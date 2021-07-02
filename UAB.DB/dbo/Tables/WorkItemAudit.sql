CREATE TABLE [dbo].[WorkItemAudit] (
    [WorkItemAuditId] INT            IDENTITY (1, 1) NOT NULL,
    [ClinicalCaseId]  INT            NOT NULL,
    [VersionId]       INT            NOT NULL,
    [FieldName]       VARCHAR (25)   NOT NULL,
    [FieldValue]      VARCHAR (1000) NULL,
    [Remark]          VARCHAR (2000) NOT NULL,
    [ErrorTypeId]     INT            NULL,
    [ClaimId]         INT            NULL,
    [IsAccepted]      BIT            DEFAULT ((0)) NULL,
    PRIMARY KEY CLUSTERED ([WorkItemAuditId] ASC)
);

