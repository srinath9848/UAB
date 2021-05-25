CREATE  PROCEDURE USPGetSearchData                   
AS                  
BEGIN     
;with searchcte as(               
SELECT distinct  CC.ClinicalCaseID,        
cc.PatientFirstName as FirstName,        
cc.PatientLastName as LastName,        
PatientMRN,pro.Name as Provider,          
   CAST(DateOfService AS VARCHAR(20)) AS DateOfService,        
   p.Name as ProjectName,          
    s.Name   as Status  ,wi.IsBlocked  ,wi.AssignedTo    
 , dense_rank() OVER(PARTITION BY cc.ClinicalCaseId ORDER BY ISNULL(pro.Name, 0) ) AS providerorder  
 from ClinicalCase cc join WorkItem wi on cc.ClinicalCaseId=wi.ClinicalCaseId join Status s on s.StatusId=wi.StatusId join Project p on p.ProjectId=cc.ProjectId left join WorkItemProvider wip on wip.ClinicalCaseId=cc.ClinicalCaseId       
left join Provider pro on pro.ProviderId=wip.ProviderId      
)  
select * from searchcte where providerorder=1  
END