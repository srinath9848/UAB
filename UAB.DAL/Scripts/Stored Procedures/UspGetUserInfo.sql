CREATE procedure UspGetUserInfo(
@Email varchar(200)  
)
AS  
BEGIN  
select u.UserId,u.Email,u.IsActive,r.RoleId,r.Name,p.ProjectId,p.IsActive
from [Projectuser] p join
[user] u on p.UserId =u.UserId join
[Role] r on p.RoleId =r.RoleId 
where u.Email=@Email
End  