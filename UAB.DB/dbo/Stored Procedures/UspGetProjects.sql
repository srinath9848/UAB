CREATE procedure [dbo].[UspGetProjects]    
AS    
BEGIN    
Select p.ProjectId, p.Name as ProjectName from Project p  
Order by p.ProjectId  
End 