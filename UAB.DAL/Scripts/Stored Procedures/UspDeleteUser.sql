---------UspDeleteUser----
Create procedure UspDeleteUser(    
@UserId int = null    
)    
As    
Begin    
Delete from [Projectuser] where UserId = @UserId    
End