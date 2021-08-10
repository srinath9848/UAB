

Create procedure [dbo].[UspUpdateProviderFeedback](
@ProviderFeedbackId int = null,
@Feedback varchar(100)
)
As
Begin
Update ProviderFeedback set Feedback = @Feedback where ProviderFeedbackID = @ProviderFeedbackId
End
