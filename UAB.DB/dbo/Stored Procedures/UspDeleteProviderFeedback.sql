

Create procedure [dbo].[UspDeleteProviderFeedback](
@ProviderFeedbackId int = null
)
As
Begin
Delete from ProviderFeedback where ProviderFeedbackId = @ProviderFeedbackId
End
