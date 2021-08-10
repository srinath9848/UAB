CREATE TYPE [dbo].[utAudit] AS TABLE (
    [FieldName]  VARCHAR (25)   NULL,
    [FieldValue] VARCHAR (1000) NULL,
    [Remark]     VARCHAR (2000) NULL,
    [ClaimId]    INT            NULL);

