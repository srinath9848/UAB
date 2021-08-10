CREATE TYPE [dbo].[utClaim] AS TABLE (
    [RNO]                INT           NULL,
    [ProviderId]         INT           NULL,
    [PayorId]            INT           NULL,
    [NoteTitle]          VARCHAR (500) NULL,
    [ProviderFeedbackId] VARCHAR (50)  NULL,
    [Dx]                 VARCHAR (500) NULL);

