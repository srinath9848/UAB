CREATE procedure [dbo].[UspAddProviderFeedback]
@Feedback varchar(255)
AS
BEGIN

Insert into ProviderFeedback (Feedback)
Values (@Feedback)
END
GO


