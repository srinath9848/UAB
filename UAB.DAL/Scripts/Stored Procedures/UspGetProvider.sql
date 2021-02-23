CREATE procedure [dbo].[UspGetProvider]
AS
BEGIN

Select p.ProviderID, p.Name from Provider p
Order by p.ProviderID
End
GO


