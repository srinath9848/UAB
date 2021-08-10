CREATE PROCEDURE [dbo].[UspGetUserInfo]    
(        
 @Email varchar(200)          
)        
AS          
BEGIN    
    
 ;WITH Roles AS    
 (    
 SELECT DISTINCT pu.UserId, r.[Name]    
 FROM ProjectUser pu    
 JOIN [Role] r    
 ON r.RoleId = pu.RoleId    
 )    
 SELECT DISTINCT u.UserId, u.Email, RoleName = STUFF((    
  SELECT N',' + [Name]    
  FROM Roles    
  WHERE UserId = pu.UserId    
  FOR XML PATH(''), TYPE).value(N'.[1]', N'nvarchar(max)'), 1, 1, N'')    
 FROM [ProjectUser] pu    
 JOIN [User] u ON pu.UserId = u.UserId     
 JOIN [Role] r ON pu.RoleId = r.RoleId     
 WHERE u.Email = @Email AND pu.IsActive = 1 AND u.IsActive = 1    
    
END 