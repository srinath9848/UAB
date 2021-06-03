
CREATE PROCEDURE [dbo].[UspPostedChartDetailedReport] --NULL,'PerDay'  
@ProjectId INT = NULL,      
@RangeType VARCHAR(100),  
@Date DateTime2 = NULL,  
@Week INT = NULL,  
@Month VARCHAR(10) = NULL,  
@Year INT = NULL
AS  
BEGIN  
  
  
IF @RangeType = 'PerDay'  
BEGIN  
 SELECT CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId
 ,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name,P.Name AS Provider
 FROM WorkItem W   
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId  
 INNER JOIN Version v ON v.ClinicalCaseId = CC.ClinicalcaseId
 LEFT JOIN List L ON L.ListId = CC.ListId
 LEFT JOIN Provider P ON P.ProviderId = CC.ProviderId
 WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))  
 AND v.StatusId in (16,17)
 AND CONVERT(VARCHAR, VersionDate, 1)=@Date
END  
ELSE IF @RangeType = 'PerWeek'  
BEGIN  
 SELECT CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId
 ,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name,P.Name AS Provider
 FROM WorkItem W   
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId  
 INNER JOIN Version v ON v.ClinicalCaseId = CC.ClinicalcaseId
 LEFT JOIN List L ON L.ListId = CC.ListId
 LEFT JOIN Provider P ON P.ProviderId = CC.ProviderId
 WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))  
 AND v.StatusId in (16,17)
 AND DATEPART(ISO_WEEK, VersionDate) = @Week  
  AND  DATENAME(month, VersionDate) = @Month
  AND  DATEPART(yyyy , VersionDate) = @Year
END  
ELSE IF @RangeType = 'PerMonth'  
BEGIN   
 SELECT CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId
 ,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name,P.Name AS Provider
 FROM WorkItem W   
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId  
 INNER JOIN Version v ON v.ClinicalCaseId = CC.ClinicalcaseId
 LEFT JOIN List L ON L.ListId = CC.ListId
 LEFT JOIN Provider P ON P.ProviderId = CC.ProviderId
 WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))  
 AND v.StatusId in (16,17)
 AND DATENAME(month, VersionDate) = @Month 
 AND   DATEPART(yyyy , VersionDate) = @Year
END  
  
END  
  
GO


