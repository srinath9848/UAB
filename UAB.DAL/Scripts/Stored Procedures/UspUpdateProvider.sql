Create procedure [dbo].[UspUpdateProvider](
@ProviderId int = null,
@Name varchar(100)
)
As
Begin
Update Provider set Name = @Name where ProviderID = @ProviderId
End
GO


