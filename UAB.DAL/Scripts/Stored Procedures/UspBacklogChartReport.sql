Create  PROCEDURE USPGetSearchData           
@mrn varchar(100)=null,           
@fname varchar(50)=null,        
@lname varchar(50)=null,        
@projectId int=null,        
@providerId int=null,      
@DoSFrom Date =null,      
@DoSTo Date =null,      
@StatusId int=null,  
@IncludeBlocked int      
AS                            
BEGIN               
;with searchcte as(                         
SELECT distinct  CC.ClinicalCaseID,                  
cc.PatientFirstName as FirstName,                  
cc.PatientLastName as LastName,                  
PatientMRN,ISnull(pro.Name,pro1.Name) as Provider,                    
   CAST(DateOfService AS VARCHAR(20)) AS DateOfService,                 
   p.Name as ProjectName,                    
    s.Name   as Status  ,wi.IsBlocked  ,wi.AssignedTo              
 , dense_rank() OVER(PARTITION BY cc.ClinicalCaseId ORDER BY ISNULL(pro.Name, 0) ) AS providerorder            
 from ClinicalCase cc   
 join WorkItem wi on cc.ClinicalCaseId=wi.ClinicalCaseId 
 join Status s on s.StatusId=wi.StatusId  
join Project p on p.ProjectId=cc.ProjectId  
 left join WorkItemProvider wip on wip.ClinicalCaseId=cc.ClinicalCaseId                 
left join Provider pro on pro.ProviderId=cc.ProviderId  
left join Provider pro1 on pro1.ProviderId=wip.ProviderId  
Left JOIN ProjectUser pu  on  p.ProjectId = pu.ProjectId         
where cc.PatientMRN =ISNULL(@mrn,cc.PatientMRN)            
AND cc.PatientFirstName=ISNULL(@fname,cc.PatientFirstName)         
AND cc.PatientLastName=ISNULL(@lname,cc.PatientLastName)          
AND p.ProjectId=ISNULL(@projectId,p.ProjectId)        
AND (cc.ProviderId=ISNULL(@providerId,cc.ProviderId) OR wip.ProviderId=ISNULL(@providerId,wip.ProviderId))  
AND (@DoSFrom is null or cc.DateOfService >= @DoSFrom )  
AND (@DoSTo is null or cc.DateOfService <= @DoSTo )      
AND  s.StatusId=ISNULL(@StatusId,s.StatusId)      
AND  wi.IsBlocked=@IncludeBlocked       
)            
select * from searchcte where providerorder=1            
END

