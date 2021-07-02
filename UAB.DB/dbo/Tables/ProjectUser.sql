CREATE TABLE [dbo].[ProjectUser] (
    [ProjectUserId]    INT IDENTITY (1, 1) NOT NULL,
    [UserId]           INT NOT NULL,
    [ProjectId]        INT NOT NULL,
    [RoleId]           INT NOT NULL,
    [IsActive]         BIT DEFAULT ('1') NOT NULL,
    [SamplePercentage] INT DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([ProjectUserId] ASC)
);

