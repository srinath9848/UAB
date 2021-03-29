Create  PROCEDURE USPGetSearchData           
AS          
BEGIN          
SELECT distinct  CC.ClinicalCaseID,
cc.PatientFirstName as FirstName,
cc.PatientLastName as LastName,
PatientMRN,pro.Name as Provider,  
   CAST(DateOfService AS VARCHAR(20)) AS DateOfService,
   p.Name as ProjectName,  
    s.Name   as Status ,wi.IsBlocked,wi.AssignedTo
from ClinicalCase cc
join WorkItem wi on cc.ClinicalCaseId=wi.ClinicalCaseId
join Status s on s.StatusId=wi.StatusId
join Project p on p.ProjectId=cc.ProjectId
left join WorkItemProvider wip on wip.ClinicalCaseId=cc.ClinicalCaseId
left join Provider pro on pro.ProviderId=wip.ProviderId
END