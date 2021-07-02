

Create procedure UspDeleteProvider(
@ProviderId int = null
)
As
Begin
Delete from Provider where ProviderID = @ProviderId
End