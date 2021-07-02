

CREATE procedure [dbo].[UspGetErrorType]
AS
BEGIN

Select e.ErrorTypeId, e.Name from ErrorType e
Order by e.ErrorTypeId
End
