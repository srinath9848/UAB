Create procedure [dbo].[UspDeleteErrorType](
@ErrorTypeId int = null
)
As
Begin
Delete from ErrorType where ErrorTypeID = @ErrorTypeId
End
GO


