

CREATE TABLE [dbo].[BlockResponse](
	[BlockResponseId] [int] IDENTITY(1,1) NOT NULL,
	[ClinicalCaseId] [int] NULL,
	[ResponseRemarks] [varchar](100) NULL,
	[ResponseByUserId] [int] NULL,
	[ResponseDate] [datetime2](7) NULL,
 CONSTRAINT [PK_dbo.BlockResponse] PRIMARY KEY CLUSTERED 
(
	[BlockResponseId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


