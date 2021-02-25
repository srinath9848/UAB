Create procedure UspGetUser(
@userId int
)        
AS            
BEGIN            
select u.UserId,u.Email,u.IsActive,r.RoleId,r.Name as RoleName,p.ProjectId,p.Name as ProjectName,p.IsActive,ps.ProjectUserId,ps.SamplePercentage    
from [Projectuser] ps join          
[user] u on ps.UserId =u.UserId join          
[Role] r on ps.RoleId =r.RoleId    join        
[Project] p on ps.ProjectId =p.ProjectId      
where ps.UserId=@userId
End 