alter FUNCTION dbo.fn_GetUserName (@userId int)  
RETURNS varchar(50) AS  
BEGIN  
 DECLARE @UserName varchar(50)  

  SELECT @UserName = FirstName + ' ' + LastName   
 FROM [User] 
 WHERE UserId = @userId  
   
 --SELECT @UserName = iu.FirstName + ' ' + iu.LastName   
 --FROM [User] u  
 --JOIN IdentityServer.dbo.Users iu  
 --ON u.email = iu.email  
 --WHERE u.UserId = @userId  
   
 RETURN @UserName  
END