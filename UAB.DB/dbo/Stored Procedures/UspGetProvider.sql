CREATE procedure [dbo].[UspGetProvider]  
AS  
BEGIN  
  
Select p.ProviderID, p.Name from Provider p  
WHERE p.Name is not null
Order by p.ProviderID  
End  