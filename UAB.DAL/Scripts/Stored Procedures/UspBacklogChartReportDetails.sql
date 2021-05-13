
CREATE PROCEDURE UspBacklogChartReportDetails         
@ProjectId INT = NULL,          
@statusId INT,
@DelayDays INt         
AS          
BEGIN 
SELECT cc.DateOfService,cc.ClinicalCaseId,cc.PatientFirstName + ' ' + cc.PatientLastName AS [Name],cc.PatientMRN,s.Name
from       
WorkItem wi      
join clinicalcase cc on cc.ClinicalCaseID=wi.ClinicalCaseID      
join Project p on p.ProjectId=wi.ProjectId      
join status s on s.StatusId=wi.StatusId      
where      
wi.StatusId =@statusId AND DATEDIFF(DAY,cc.DateOfService,GETDATE())>=p.SLAInDays      
and p.ProjectId=@projectId    
and DATEDIFF(DAY,cc.DateOfService,GETDATE())=@DelayDays
END