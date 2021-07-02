CREATE procedure [dbo].[UspGetProviderFeedback]  
AS  
BEGIN  
Select p.ProviderFeedbackId, p.Feedback from ProviderFeedback p  
Order by p.ProviderFeedbackId  
End  
