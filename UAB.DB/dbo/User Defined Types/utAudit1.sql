CREATE TYPE [dbo].[utAudit1] AS TABLE (
    [FieldName]   VARCHAR (25)   NULL,
    [FieldValue]  VARCHAR (1000) NULL,
    [Remark]      VARCHAR (2000) NULL,
    [ErrorTypeId] INT            NULL,
    [ClaimId]     INT            NULL);

