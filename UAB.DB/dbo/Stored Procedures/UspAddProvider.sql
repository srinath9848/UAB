
CREATE procedure UspAddProvider
@Name varchar(50)
AS
BEGIN

Insert into Provider (Name)
Values (@Name)
END