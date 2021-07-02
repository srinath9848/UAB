

CREATE procedure [dbo].[UspGetPayor]
AS
BEGIN

Select p.PayorId, p.Name from Payor p
Order by p.PayorID
End
