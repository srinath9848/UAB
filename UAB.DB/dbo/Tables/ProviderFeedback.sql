CREATE TABLE [dbo].[ProviderFeedback] (
    [ProviderFeedbackId] INT           IDENTITY (1, 1) NOT NULL,
    [Feedback]           VARCHAR (255) NOT NULL,
    CONSTRAINT [PK__Provider__5956E7C80EBE7B02] PRIMARY KEY CLUSTERED ([ProviderFeedbackId] ASC)
);

