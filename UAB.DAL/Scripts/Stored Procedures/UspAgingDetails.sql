
CREATE PROCEDURE UspAgingDetails           
@ProjectId INT = NULL,            
@ColumnName varchar(50)  
AS            
BEGIN   
SELECT cc.DateOfService,cc.ClinicalCaseId,cc.PatientFirstName + ' ' + cc.PatientLastName AS [Name],cc.PatientMRN,s.Name  
from         
WorkItem wi        
join clinicalcase cc on cc.ClinicalCaseID=wi.ClinicalCaseID        
join Project p on p.ProjectId=wi.ProjectId        
join status s on s.StatusId=wi.StatusId        
where      
wi.ProjectId=@ProjectId  
AND wi.isBlocked = 1  
END