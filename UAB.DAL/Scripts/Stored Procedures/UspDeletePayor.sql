Create procedure [dbo].[UspDeletePayor](
@PayorId int = null
)
As
Begin
Delete from Payor where PayorID = @PayorId
End
GO


