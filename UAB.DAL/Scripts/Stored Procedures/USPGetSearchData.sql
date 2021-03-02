

create PROCEDURE USPGetSearchData         
AS        
BEGIN        
SELECT  CC.ClinicalCaseID,cc.PatientFirstName as FirstName,cc.PatientLastName as LastName,PatientMRN ,    
   CAST(DateOfService AS VARCHAR(20)) AS DateOfService,p.Name as ProjectName,
    s.Name   as Status      
   FROM ClinicalCase CC INNER JOIN WorkItem W         
  ON W.ClinicalCaseId = CC.ClinicalCaseId     
  JOIN Status s on s.StatusId=w.StatusId  
  Join Project p on p.ProjectId=w.ProjectId     
ORDER BY CC.CreatedDate        
END