ALTER PROCEDURE [dbo].[UspPendingChartDetailReport]
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
 SELECT CC.ClinicalCaseId AS ClinicalCaseId, CC.PatientFirstName+' '+CC.PatientLastName AS Name,CC.PatientMRN AS PatientMRN,CC.DateOfService AS DateOfService  FROM WorkItem W     
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId  
 INNER JOIN BlockHistory BH on CC.ClinicalcaseId = bh.ClinicalcaseId
 INNER JOIN BlockCategory BC on BH.BlockCategoryID = BC.BlockCategoryId
 WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))
 AND CONVERT(VARCHAR, BH.CreateDate, 1)=@Date
END
ELSE IF @RangeType = 'PerWeek'    
BEGIN    
 SELECT CC.ClinicalCaseId AS ClinicalCaseId,CC.PatientFirstName+' '+CC.PatientLastName AS Name ,CC.PatientMRN AS PatientMRN,CC.DateOfService AS DateOfService
 FROM WorkItem W     
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId  
 INNER JOIN BlockHistory BH on CC.ClinicalcaseId = bh.ClinicalcaseId
 INNER JOIN BlockCategory BC on BH.BlockCategoryID = BC.BlockCategoryId
 WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))  
 AND DATEPART(WEEK, BH.CreateDate)-DATEPART(WEEK, DATEADD(MM, DATEDIFF(MM,0,BH.CreateDate), 0))+ 1 = @Week
 AND DATENAME(month, BH.CreateDate)=@Month
 AND DATEPART(yyyy, BH.CreateDate)=@Year   
END
ELSE IF @RangeType = 'PerMonth'    
BEGIN   
SELECT CC.ClinicalCaseId AS ClinicalCaseId,CC.PatientFirstName+' '+CC.PatientLastName AS Name ,CC.PatientMRN AS PatientMRN,CC.DateOfService AS DateOfService
 FROM WorkItem W     
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId  
 INNER JOIN BlockHistory BH on CC.ClinicalcaseId = bh.ClinicalcaseId
 INNER JOIN BlockCategory BC on BH.BlockCategoryID = BC.BlockCategoryId
 WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))  
 AND DATENAME(month, BH.CreateDate)=@Month
 AND DATEPART(yyyy, BH.CreateDate)=@Year
END
END
Go