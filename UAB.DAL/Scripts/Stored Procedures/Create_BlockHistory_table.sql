CREATE TABLE [dbo].[BlockHistory](
	BlockHistoryId [int] IDENTITY(1,1) NOT NULL,
	BlockedByUserId [int] NULL,
	BlockCategoryId [int] NULL,
	Remarks [varchar](50) NULL,
	CreateDate [datetime2](7) NULL,
 CONSTRAINT [PK_dbo.BlockHistory] PRIMARY KEY CLUSTERED 
(
	[BlockHistoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO