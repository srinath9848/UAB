CREATE TABLE [dbo].[CoderQuestion] (
    [CoderQuestionId] INT           IDENTITY (1, 1) NOT NULL,
    [ClinicalCaseId]  INT           NOT NULL,
    [Question]        VARCHAR (255) NOT NULL,
    [QuestionBy]      INT           NOT NULL,
    [QuestionDate]    DATE          NOT NULL,
    [Answer]          VARCHAR (255) NULL,
    [AnsweredBy]      INT           NULL,
    [AnsweredDate]    DATE          NULL,
    PRIMARY KEY CLUSTERED ([CoderQuestionId] ASC),
    CONSTRAINT [FKCoderQuest192420] FOREIGN KEY ([ClinicalCaseId]) REFERENCES [dbo].[ClinicalCase] ([ClinicalCaseId])
);

