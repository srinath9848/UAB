
Create procedure UspUpdatePayor(
@PayorId int = null,
@Name varchar(100)
)
As
Begin
Update Payor set Name = @Name where PayorID = @PayorId
End