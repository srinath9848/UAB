CREATE TABLE [dbo].[Client] (
    [ClientId] INT           IDENTITY (1, 1) NOT NULL,
    [Name]     VARCHAR (255) NOT NULL,
    [IsActive] BIT           DEFAULT ('1') NOT NULL,
    PRIMARY KEY CLUSTERED ([ClientId] ASC)
);

