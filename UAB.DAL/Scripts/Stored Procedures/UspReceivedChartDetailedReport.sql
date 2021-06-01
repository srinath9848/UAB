
CREATE PROCEDURE [dbo].[UspReceivedChartDetailedReport]-- NULL,'PerWeek'    
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
 AND CONVERT(VARCHAR, CreatedDate, 1)=@Date
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
 AND   DATEPART(ISO_WEEK, CreatedDate) = @Week
 AND	DATENAME(month, CreatedDate) = @Month   
 AND   DATEPART(yyyy , CreatedDate) = @Year
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
 AND DATENAME(month, CreatedDate) = @Month    
 AND   DATEPART(yyyy , CreatedDate) = @Year      
END    
    
END    
GO