CREATE TABLE [dbo].[WorkItem] (
    [WorkItemId]      INT           IDENTITY (1, 1) NOT NULL,
    [ClinicalCaseId]  INT           NOT NULL,
    [StatusId]        INT           NOT NULL,
    [CodedDate]       DATETIME      NULL,
    [AssignedTo]      INT           NULL,
    [AssignedBy]      INT           NULL,
    [NoteTitle]       VARCHAR (200) NULL,
    [QABy]            INT           NULL,
    [ShadowQABy]      INT           NULL,
    [ProjectId]       INT           NOT NULL,
    [QADate]          DATETIME2 (7) NULL,
    [ShadowQADate]    DATETIME2 (7) NULL,
    [AssignedDate]    DATETIME2 (7) NULL,
    [OnHold]          BIT           DEFAULT ((0)) NULL,
    [IsPriority]      INT           DEFAULT ((0)) NOT NULL,
    [IsBlocked]       INT           DEFAULT ((0)) NOT NULL,
    [IsQA]            BIT           DEFAULT ((0)) NULL,
    [IsShadowQA]      BIT           DEFAULT ((0)) NULL,
    [IsWrongProvider] BIT           DEFAULT ((0)) NULL,
    CONSTRAINT [PK__WorkItem__A10D1B45EA268A61] PRIMARY KEY CLUSTERED ([WorkItemId] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_WorkItem_ProjectId_StatusId]
    ON [dbo].[WorkItem]([ProjectId] ASC, [StatusId] ASC, [AssignedTo] ASC, [IsPriority] ASC, [IsBlocked] ASC);

