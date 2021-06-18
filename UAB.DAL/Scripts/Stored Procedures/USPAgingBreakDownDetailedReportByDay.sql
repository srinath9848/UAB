CREATE PROCEDURE [dbo].[USPAgingBreakDownDetailedReportByDay] (@ProjectId INT,@ProjectTypeName varchar(50),@ColumnName varchar(50))    
AS    
BEGIN    
IF @ColumnName = 'Due'    
BEGIN    
IF @ProjectTypeName = 'Ambulatory'    
BEGIN    
SELECT distinct CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId    
 ,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name,Pro.Name AS Provider    
FROM Project p    
JOIN ProjectType pt    
ON p.ProjectTypeId = pt.ProjectTypeId    
JOIN WorkItem wi    
ON wi.ProjectId = p.ProjectId    
JOIN ClinicalCase cc    
ON wi.ClinicalCaseId = cc.ClinicalCaseId    
LEFT JOIN List L ON L.ListId = CC.ListId    
 LEFT JOIN Provider Pro ON Pro.ProviderId = CC.ProviderId    
WHERE wi.StatusId < 16 --only not posted items    
AND pt.ProjectTypeName = @ProjectTypeName    
AND p.ProjectId = @ProjectId    
AND DATEDIFF(DAY, cc.DateOfService, GETUTCDATE()) > 3    
END    
ELSE IF @ProjectTypeName = 'IP'    
BEGIN    
SELECT distinct CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId    
 ,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name, ISNULL(Pro.Name,PP.Name) AS Provider       
FROM Project p    
JOIN ProjectType pt    
ON p.ProjectTypeId = pt.ProjectTypeId    
JOIN WorkItem wi    
ON wi.ProjectId = p.ProjectId    
JOIN ClinicalCase cc    
ON wi.ClinicalCaseId = cc.ClinicalCaseId    
LEFT JOIN List L ON L.ListId = CC.ListId    
 LEFT JOIN Provider Pro ON Pro.ProviderId = CC.ProviderId   
  LEFT JOIN WorkItemProvider Wip ON Wip.ClinicalCaseId = CC.ClinicalCaseId
 LEFT JOIN Provider PP ON PP.ProviderID = Wip.ProviderId 
WHERE wi.StatusId < 16 --only not posted items    
AND pt.ProjectTypeName = @ProjectTypeName    
AND p.ProjectId = @ProjectId    
AND DATEDIFF(DAY, cc.DateOfService, GETUTCDATE()) > 5    
END    
END    
ELSE IF @ColumnName = 'Total'    
BEGIN    
SELECT distinct CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId    
 ,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name,ISNULL(Pro.Name,PP.Name) AS Provider    
FROM Project p    
JOIN ProjectType pt    
ON p.ProjectTypeId = pt.ProjectTypeId    
JOIN WorkItem wi    
ON wi.ProjectId = p.ProjectId    
JOIN ClinicalCase cc    
ON wi.ClinicalCaseId = cc.ClinicalCaseId    
LEFT JOIN List L ON L.ListId = CC.ListId    
 LEFT JOIN Provider Pro ON Pro.ProviderId = CC.ProviderId    
   LEFT JOIN WorkItemProvider Wip ON Wip.ClinicalCaseId = CC.ClinicalCaseId
 LEFT JOIN Provider PP ON PP.ProviderID = Wip.ProviderId 
WHERE wi.StatusId < 16 --only not posted items    
AND pt.ProjectTypeName = @ProjectTypeName    
AND p.ProjectId = @ProjectId    
--AND DATEDIFF(DAY, cc.DateOfService, GETUTCDATE()) BETWEEN 0 AND 3    
AND (DATEDIFF(DAY, cc.DateOfService, GETUTCDATE()) > 3 OR DATEDIFF(DAY, cc.DateOfService, GETUTCDATE()) BETWEEN 0 AND 3 OR DATEDIFF(DAY, cc.DateOfService, GETUTCDATE()) BETWEEN 4 AND 5     
OR DATEDIFF(DAY, cc.DateOfService, GETUTCDATE()) BETWEEN 6 AND 7 OR DATEDIFF(DAY, cc.DateOfService, GETUTCDATE()) > 7)    
END    
ELSE IF @ColumnName = '0-3 Days'    
BEGIN    
SELECT distinct CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId    
 ,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name,ISNULL(Pro.Name,PP.Name) AS Provider    
FROM Project p    
JOIN ProjectType pt    
ON p.ProjectTypeId = pt.ProjectTypeId    
JOIN WorkItem wi    
ON wi.ProjectId = p.ProjectId    
JOIN ClinicalCase cc    
ON wi.ClinicalCaseId = cc.ClinicalCaseId    
LEFT JOIN List L ON L.ListId = CC.ListId    
 LEFT JOIN Provider Pro ON Pro.ProviderId = CC.ProviderId  
   LEFT JOIN WorkItemProvider Wip ON Wip.ClinicalCaseId = CC.ClinicalCaseId
 LEFT JOIN Provider PP ON PP.ProviderID = Wip.ProviderId   
WHERE wi.StatusId < 16 --only not posted items    
AND pt.ProjectTypeName = @ProjectTypeName    
AND p.ProjectId = @ProjectId    
AND DATEDIFF(DAY, cc.DateOfService, GETUTCDATE()) BETWEEN 0 AND 3    
END    
ELSE IF @ColumnName = '4-5 Days'    
BEGIN    
SELECT distinct CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId    
 ,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name,ISNULL(Pro.Name,PP.Name) AS Provider    
FROM Project p    
JOIN ProjectType pt    
ON p.ProjectTypeId = pt.ProjectTypeId    
JOIN WorkItem wi    
ON wi.ProjectId = p.ProjectId    
JOIN ClinicalCase cc    
ON wi.ClinicalCaseId = cc.ClinicalCaseId    
LEFT JOIN List L ON L.ListId = CC.ListId    
 LEFT JOIN Provider Pro ON Pro.ProviderId = CC.ProviderId
   LEFT JOIN WorkItemProvider Wip ON Wip.ClinicalCaseId = CC.ClinicalCaseId
 LEFT JOIN Provider PP ON PP.ProviderID = Wip.ProviderId     
WHERE wi.StatusId < 16 --only not posted items    
AND pt.ProjectTypeName = @ProjectTypeName    
AND p.ProjectId = @ProjectId    
AND DATEDIFF(DAY, cc.DateOfService, GETUTCDATE()) BETWEEN 4 AND 5    
END    
ELSE IF @ColumnName = '6-7 Days'    
BEGIN    
SELECT distinct CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId    
 ,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name,ISNULL(Pro.Name,PP.Name) AS Provider    
FROM Project p    
JOIN ProjectType pt    
ON p.ProjectTypeId = pt.ProjectTypeId    
JOIN WorkItem wi    
ON wi.ProjectId = p.ProjectId    
JOIN ClinicalCase cc    
ON wi.ClinicalCaseId = cc.ClinicalCaseId    
LEFT JOIN List L ON L.ListId = CC.ListId    
 LEFT JOIN Provider Pro ON Pro.ProviderId = CC.ProviderId    
   LEFT JOIN WorkItemProvider Wip ON Wip.ClinicalCaseId = CC.ClinicalCaseId
 LEFT JOIN Provider PP ON PP.ProviderID = Wip.ProviderId 
WHERE wi.StatusId < 16 --only not posted items    
AND pt.ProjectTypeName = @ProjectTypeName    
AND p.ProjectId = @ProjectId    
AND DATEDIFF(DAY, cc.DateOfService, GETUTCDATE()) BETWEEN 6 AND 7    
END    
ELSE IF @ColumnName = '8+ Days'    
BEGIN    
SELECT distinct CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId    
 ,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name,ISNULL(Pro.Name,PP.Name) AS Provider    
FROM Project p    
JOIN ProjectType pt    
ON p.ProjectTypeId = pt.ProjectTypeId    
JOIN WorkItem wi    
ON wi.ProjectId = p.ProjectId    
JOIN ClinicalCase cc    
ON wi.ClinicalCaseId = cc.ClinicalCaseId    
LEFT JOIN List L ON L.ListId = CC.ListId    
 LEFT JOIN Provider Pro ON Pro.ProviderId = CC.ProviderId    
   LEFT JOIN WorkItemProvider Wip ON Wip.ClinicalCaseId = CC.ClinicalCaseId
 LEFT JOIN Provider PP ON PP.ProviderID = Wip.ProviderId 
WHERE wi.StatusId < 16 --only not posted items    
AND pt.ProjectTypeName = @ProjectTypeName    
AND p.ProjectId = @ProjectId    
AND DATEDIFF(DAY, cc.DateOfService, GETUTCDATE()) > 7    
END    
END 
GO
