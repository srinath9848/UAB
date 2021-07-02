
Create procedure [dbo].[UspUpdateErrorType](
@ErrorTypeId int = null,
@Name varchar(100)
)
As
Begin
Update ErrorType set Name = @Name where ErrorTypeID = @ErrorTypeId
End
