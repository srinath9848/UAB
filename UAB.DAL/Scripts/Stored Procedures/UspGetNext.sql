CREATE PROCEDURE USPGetNext --1,1    
@StatusID INT,    
@ProjectID INT    
AS    
BEGIN    
    
SELECT TOP 1 CC.ClinicalCaseID,PatientMRN    
   ,PatientFirstName + ' ' + PatientLastName AS [Name]    
   ,CAST(DateOfService AS VARCHAR(20)) AS DateOfService    
   FROM ClinicalCase CC INNER JOIN WorkItem W     
  ON W.ClinicalCaseId = CC.ClinicalCaseId    
WHERE CC.ProjectId = @ProjectID AND W.StatusId = @StatusID    
ORDER BY CC.CreatedDate    
    
END