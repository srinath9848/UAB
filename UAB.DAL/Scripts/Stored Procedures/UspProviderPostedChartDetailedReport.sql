CREATE PROCEDURE [dbo].[UspProviderPostedChartDetailedReport] --NULL,'PerDay'          
@ProjectId INT = NULL,          
@RangeType VARCHAR(100),      
@Date DateTime2 = NULL,      
@Week INT = NULL,      
@Month VARCHAR(10) = NULL,      
@Year INT = NULL    
AS          
BEGIN          
        
IF @ProjectId = 0         
 SET @ProjectId = NULL          
          
IF @RangeType = 'PerDay'          
BEGIN          
 SELECT CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId    
 ,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name, ISNULL(P.Name,Pro.Name) AS Provider      
 FROM WorkItem W           
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId     
 INNER JOIN ProviderPosted PP ON PP.ClinicalCaseId=CC.ClinicalCaseId    
 LEFT JOIN List L ON L.ListId = CC.ListId    
 LEFT JOIN Provider P ON P.ProviderId = CC.ProviderId   
  LEFT JOIN WorkItemProvider Wip ON Wip.ClinicalCaseId = CC.ClinicalCaseId
 LEFT JOIN Provider Pro ON Pro.ProviderID = Wip.ProviderId 
 WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))        
 AND W.StatusId =17    
 AND CONVERT(VARCHAR, PostingDate, 1) = @Date        
END          
ELSE IF @RangeType = 'PerWeek'          
BEGIN          
 SELECT CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId    
 ,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name,ISNULL(P.Name,Pro.Name) AS Provider    
 FROM WorkItem W           
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId    
 INNER JOIN ProviderPosted PP ON PP.ClinicalCaseId=CC.ClinicalCaseId    
 LEFT JOIN List L ON L.ListId = CC.ListId    
 LEFT JOIN Provider P ON P.ProviderId = CC.ProviderId    
   LEFT JOIN WorkItemProvider Wip ON Wip.ClinicalCaseId = CC.ClinicalCaseId
 LEFT JOIN Provider Pro ON Pro.ProviderID = Wip.ProviderId 
 WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))        
 AND W.StatusId =17    
 AND DATEPART(ISO_WEEK, PostingDate) = @Week        
  AND  DATENAME(month, PostingDate) = @Month         
  AND  DATEPART(yyyy , PostingDate) = @Year    
END          
ELSE IF @RangeType = 'PerMonth'          
BEGIN         
        
SELECT CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId    
 ,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name,ISNULL(P.Name,Pro.Name) AS Provider    
 FROM WorkItem W           
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId    
 INNER JOIN ProviderPosted PP ON PP.ClinicalCaseId=CC.ClinicalCaseId    
 LEFT JOIN List L ON L.ListId = CC.ListId    
 LEFT JOIN Provider P ON P.ProviderId = CC.ProviderId  
   LEFT JOIN WorkItemProvider Wip ON Wip.ClinicalCaseId = CC.ClinicalCaseId
 LEFT JOIN Provider Pro ON Pro.ProviderID = Wip.ProviderId   
 WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))        
 AND W.StatusId =17    
 AND DATENAME(month, PostingDate) = @Month    
  AND  DATEPART(yyyy , PostingDate) = @Year    
END          
          
END
GO