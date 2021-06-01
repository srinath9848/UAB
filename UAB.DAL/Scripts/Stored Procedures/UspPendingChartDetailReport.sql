ALTER PROCEDURE [dbo].[UspPendingChartDetailReport]--  1,'PerWeek','5/17/2021 4:16:40 PM',20,'May',2021
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
 SELECT CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name,P.Name AS Provider
 FROM WorkItem W       
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId    
 INNER JOIN BlockHistory BH on CC.ClinicalcaseId = bh.ClinicalcaseId  
 INNER JOIN BlockCategory BC on BH.BlockCategoryID = BC.BlockCategoryId
 LEFT JOIN List L ON L.ListId = CC.ListId
 LEFT JOIN Provider P ON P.ProviderId = CC.ProviderId
 WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))  
 AND CONVERT(VARCHAR, BH.CreateDate, 1)=@Date  
END  
ELSE IF @RangeType = 'PerWeek'      
BEGIN      
 SELECT CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name,P.Name AS Provider
 FROM WorkItem W       
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId    
 INNER JOIN BlockHistory BH on CC.ClinicalcaseId = bh.ClinicalcaseId  
 INNER JOIN BlockCategory BC on BH.BlockCategoryID = BC.BlockCategoryId
 LEFT JOIN List L ON L.ListId = CC.ListId
 LEFT JOIN Provider P ON P.ProviderId = CC.ProviderId
 WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))    
 --AND DATEPART(WEEK, BH.CreateDate)-DATEPART(WEEK, DATEADD(MM, DATEDIFF(MM,0,BH.CreateDate), 0))+ 1 = @Week  
   AND DATEPART(ISO_WEEK, BH.CreateDate) = @Week
 AND DATENAME(month, BH.CreateDate)=@Month  
 AND DATEPART(yyyy, BH.CreateDate)=@Year     
END  
ELSE IF @RangeType = 'PerMonth'      
BEGIN     
SELECT CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name,P.Name AS Provider
 FROM WorkItem W       
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId    
 INNER JOIN BlockHistory BH on CC.ClinicalcaseId = bh.ClinicalcaseId  
 INNER JOIN BlockCategory BC on BH.BlockCategoryID = BC.BlockCategoryId
 LEFT JOIN List L ON L.ListId = CC.ListId
 LEFT JOIN Provider P ON P.ProviderId = CC.ProviderId
 WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))    
 AND DATENAME(month, BH.CreateDate)=@Month  
 AND DATEPART(yyyy, BH.CreateDate)=@Year  
END  
END  
GO