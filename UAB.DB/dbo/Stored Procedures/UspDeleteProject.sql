

Create procedure [dbo].[UspDeleteProject](
@ProjectId int = null
)
As
Begin
Delete from Project where ProjectID = @ProjectId
End
