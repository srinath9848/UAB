CREATE TABLE [dbo].[EMCodeLevel] (
    [Id]      INT         IDENTITY (1, 1) NOT NULL,
    [EMCode]  VARCHAR (5) NULL,
    [EMLevel] INT         NOT NULL
);


GO
CREATE NONCLUSTERED INDEX [IX_EMCodeLevel_EMCode_EMLevel]
    ON [dbo].[EMCodeLevel]([EMCode] ASC, [EMLevel] ASC);

