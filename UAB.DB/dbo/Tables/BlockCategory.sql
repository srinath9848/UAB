CREATE TABLE [dbo].[BlockCategory] (
    [BlockCategoryId] INT          IDENTITY (1, 1) NOT NULL,
    [Name]            VARCHAR (50) NULL,
    [BlockType]       VARCHAR (50) NULL,
    CONSTRAINT [PK_dbo.BlockCategory] PRIMARY KEY CLUSTERED ([BlockCategoryId] ASC)
);

