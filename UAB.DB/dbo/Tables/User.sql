CREATE TABLE [dbo].[User] (
    [UserId]    INT            IDENTITY (1, 1) NOT NULL,
    [Email]     VARCHAR (255)  NOT NULL,
    [IsActive]  BIT            DEFAULT ('1') NOT NULL,
    [FirstName] VARCHAR (8000) NULL,
    [LastName]  VARCHAR (8000) NULL,
    PRIMARY KEY CLUSTERED ([UserId] ASC)
);

