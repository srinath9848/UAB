CREATE TABLE [dbo].[InputFileFieldMapping] (
    [InputFileFieldMappingId] INT          IDENTITY (1, 1) NOT NULL,
    [FieldName]               VARCHAR (50) NULL,
    [FieldOrder]              INT          NULL,
    [ProjectTypeID]           INT          NULL,
    PRIMARY KEY CLUSTERED ([InputFileFieldMappingId] ASC),
    FOREIGN KEY ([ProjectTypeID]) REFERENCES [dbo].[ProjectType] ([ProjectTypeId])
);

