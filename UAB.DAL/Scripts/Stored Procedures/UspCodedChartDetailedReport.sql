
CREATE PROCEDURE [dbo].[UspCodedChartDetailedReport] --NULL,'PerWeek'      
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
 ,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name,P.Name AS Provider
 FROM WorkItem W       
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId
 LEFT JOIN List L ON L.ListId = CC.ListId
 LEFT JOIN Provider P ON P.ProviderId = CC.ProviderId
 WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))    
 AND W.StatusId NOT IN (1,2)
 AND CONVERT(VARCHAR, CodedDate, 1)=@Date
END      
ELSE IF @RangeType = 'PerWeek'      
BEGIN      
 SELECT CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId
 ,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name,P.Name AS Provider
 FROM WorkItem W       
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId
 LEFT JOIN List L ON L.ListId = CC.ListId
 LEFT JOIN Provider P ON P.ProviderId = CC.ProviderId
 WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))    
 AND W.StatusId NOT IN (1,2)
 AND DATEPART(ISO_WEEK, CodedDate) = @Week     
 AND  DATENAME(month, CodedDate) = @Month    
  AND  DATEPART(yyyy , CodedDate) = @Year
END      
ELSE IF @RangeType = 'PerMonth'      
BEGIN
 SELECT CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId
 ,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name,P.Name AS Provider
 FROM WorkItem W       
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId
 LEFT JOIN List L ON L.ListId = CC.ListId
 LEFT JOIN Provider P ON P.ProviderId = CC.ProviderId
 WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))    
 AND W.StatusId NOT IN (1,2)
 AND CONVERT(VARCHAR, CodedDate, 1)=@Date
 AND DATENAME(month, CodedDate) = @Month      
  AND  DATEPART(yyyy , CodedDate) = @Year         
END      
      
END      
GO
