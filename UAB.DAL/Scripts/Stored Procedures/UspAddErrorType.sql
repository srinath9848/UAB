CREATE procedure [dbo].[UspAddErrorType]
@Name varchar(50)
AS
BEGIN

Insert into ErrorType (Name)
Values (@Name)
END
GO


