----UspGetUsers-------
Create procedure UspGetUsers  
AS      
BEGIN      
select u.UserId,u.Email,u.IsActive,r.RoleId,r.Name as RoleName,p.ProjectId,p.Name as ProjectName,p.IsActive    
from [Projectuser] ps join    
[user] u on ps.UserId =u.UserId join    
[Role] r on ps.RoleId =r.RoleId    join  
[Project] p on ps.ProjectId =p.ProjectId  
End 