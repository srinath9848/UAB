
CREATE procedure [dbo].[UspAddPayor]
@Name varchar(50)
AS
BEGIN

Insert into Payor (Name)
Values (@Name)
END
