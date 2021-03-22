CREATE PROCEDURE [dbo].[USPGetBlockCharts] --1,'QA','Available',1          
@ProjectID INT,                              
@Role VARCHAR(50),                            
@ChartType VARCHAR(50),                            
@UserId INT                            
AS                              
BEGIN  
SELECT  CC.ClinicalCaseID,PatientMRN                              
    ,PatientFirstName + ' ' + PatientLastName AS [Name]                              
    ,CAST(DateOfService AS VARCHAR(20)) AS DateOfService      
 ,isnull(CC.ProviderId,0)           
 ,Bc.Name AS BlockCategory          
 ,Bh.Remarks AS BlockRemarks          
 ,Bh.CreateDate AS BlockedDate                         
 FROM ClinicalCase CC INNER JOIN WorkItem W                               
   ON W.ClinicalCaseId = CC.ClinicalCaseId                          
   INNER JOIN ProjectUser pu ON pu.ProjectId = w.ProjectId             
   INNER JOIN BlockHistory Bh ON Bh.ClinicalCaseId = CC.ClinicalcaseId               
   INNER JOIN BlockCategory Bc ON Bh.BlockCategoryId = Bc.BlockCategoryId               
 WHERE CC.ProjectId = @ProjectID           
 AND ((w.AssignedTo = @UserId AND ISNULL(W.IsBlocked, 0) = 1))          
 AND pu.UserId = @UserId AND pu.RoleId = 1 AND pu.IsActive = 1                
 ORDER BY CC.ClinicalCaseID        
  
END