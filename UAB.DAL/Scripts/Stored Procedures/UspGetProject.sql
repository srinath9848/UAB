CREATE procedure [dbo].[UspGetProject]      
AS      
BEGIN      
select       
p.ProjectId      
,p.Name ProjectName      
,p.IsActive ActiveProject      
,p.CreatedDate      
,p.InputFileLocation      
,p.InputFileFormat      
,c.ClientId      
,c.Name ClientName      
,c.IsActive ActiveClient      
,pt.ProjectTypeId      
,pt.ProjectTypeName    
,p.SLAInDays  
,p.TPICProjectId    
from Project p       
join Client c on p.ClientId=c.ClientId      
join ProjectType pt on p.ProjectTypeId=pt.ProjectTypeId      
order by p.ProjectId      
END