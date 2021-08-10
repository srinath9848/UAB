CREATE PROCEDURE [dbo].[UspBacklogChartReportDetails]               
@ProjectId INT = NULL,                
@statusId INT,      
@DelayDays INt               
AS                
BEGIN       
 SELECT distinct CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId      
 ,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name, ISNULL(Pro.Name,PP.Name) AS Provider     
from             
WorkItem wi            
join clinicalcase cc on cc.ClinicalCaseID=wi.ClinicalCaseID            
join Project p on p.ProjectId=wi.ProjectId            
join status s on s.StatusId=wi.StatusId      
LEFT JOIN List L ON L.ListId = CC.ListId      
 LEFT JOIN Provider Pro ON Pro.ProviderId = CC.ProviderId      
  LEFT JOIN WorkItemProvider Wip ON Wip.ClinicalCaseId = CC.ClinicalCaseId  
 LEFT JOIN Provider PP ON PP.ProviderID = Wip.ProviderId  
where            
wi.StatusId =@statusId AND DATEDIFF(DAY,cc.DateOfService,GETUTCDATE())>=p.SLAInDays            
and p.ProjectId=@projectId          
and DATEDIFF(DAY,cc.DateOfService,GETUTCDATE())=@DelayDays      
END 