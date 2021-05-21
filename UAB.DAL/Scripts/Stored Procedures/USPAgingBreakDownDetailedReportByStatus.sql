CREATE PROCEDURE [dbo].[USPAgingBreakDownDetailedReportByStatus] (@ProjectId INT,@ProjectTypeName varchar(50),@ColumnName varchar(50))
AS
BEGIN
IF @ColumnName = 'Due'
BEGIN
IF @ProjectTypeName = 'Ambulatory'
BEGIN
SELECT cc.PatientMRN, cc.PatientFirstName+' '+cc.PatientLastName as Name
FROM Project p
JOIN ProjectType pt
ON p.ProjectTypeId = pt.ProjectTypeId
JOIN WorkItem wi
ON wi.ProjectId = p.ProjectId
JOIN ClinicalCase cc
ON wi.ClinicalCaseId = cc.ClinicalCaseId
WHERE wi.StatusId < 16 --only not posted items
AND pt.ProjectTypeName = @ProjectTypeName
AND p.ProjectId = @ProjectId
AND DATEDIFF(DAY, cc.DateOfService, GETDATE()) > 3
END
ELSE IF @ProjectTypeName = 'IP'
BEGIN
SELECT cc.PatientMRN, cc.PatientFirstName+' '+cc.PatientLastName as Name
FROM Project p
JOIN ProjectType pt
ON p.ProjectTypeId = pt.ProjectTypeId
JOIN WorkItem wi
ON wi.ProjectId = p.ProjectId
JOIN ClinicalCase cc
ON wi.ClinicalCaseId = cc.ClinicalCaseId
WHERE wi.StatusId < 16 --only not posted items
AND pt.ProjectTypeName = @ProjectTypeName
AND p.ProjectId = @ProjectId
AND DATEDIFF(DAY, cc.DateOfService, GETDATE()) > 5
END
END
ELSE IF @ColumnName = 'Total'
BEGIN
SELECT cc.PatientMRN, cc.PatientFirstName+' '+cc.PatientLastName as Name
FROM Project p
JOIN ProjectType pt
ON p.ProjectTypeId = pt.ProjectTypeId
JOIN WorkItem wi
ON wi.ProjectId = p.ProjectId
JOIN ClinicalCase cc
ON wi.ClinicalCaseId = cc.ClinicalCaseId
WHERE wi.StatusId < 16 --only not posted items
AND pt.ProjectTypeName = @ProjectTypeName
AND p.ProjectId = @ProjectId
--AND DATEDIFF(DAY, cc.DateOfService, GETDATE()) BETWEEN 0 AND 3
AND (DATEDIFF(DAY, cc.DateOfService, GETDATE()) > 3 OR DATEDIFF(DAY, cc.DateOfService, GETDATE()) BETWEEN 0 AND 3 OR DATEDIFF(DAY, cc.DateOfService, GETDATE()) BETWEEN 4 AND 5 
OR DATEDIFF(DAY, cc.DateOfService, GETDATE()) BETWEEN 6 AND 7 OR DATEDIFF(DAY, cc.DateOfService, GETDATE()) > 7)
END
ELSE IF @ColumnName = 'Not Ready For Coding'
BEGIN
IF @ProjectTypeName = 'Ambulatory'
BEGIN
SELECT cc.PatientMRN,cc.PatientFirstName+' '+cc.PatientLastName AS Name
FROM Project p
JOIN ProjectType pt
ON p.ProjectTypeId = pt.ProjectTypeId
JOIN WorkItem wi
ON wi.ProjectId = p.ProjectId
JOIN ClinicalCase cc
ON wi.ClinicalCaseId = cc.ClinicalCaseId
WHERE wi.StatusId < 16 --only not posted items
AND pt.ProjectTypeName = @ProjectTypeName
AND p.ProjectId=@ProjectId
AND wi.StatusId = 1 
AND (pt.ProjectTypeName = @ProjectTypeName AND DATEDIFF(DAY, cc.DateOfService, GETDATE()) <= 3 )
END
ELSE IF @ProjectTypeName = 'IP'
BEGIN
SELECT cc.PatientMRN,cc.PatientFirstName+' '+cc.PatientLastName AS Name
FROM Project p
JOIN ProjectType pt
ON p.ProjectTypeId = pt.ProjectTypeId
JOIN WorkItem wi
ON wi.ProjectId = p.ProjectId
JOIN ClinicalCase cc
ON wi.ClinicalCaseId = cc.ClinicalCaseId
WHERE wi.StatusId < 16 --only not posted items
AND pt.ProjectTypeName = @ProjectTypeName
AND p.ProjectId=@ProjectId
AND wi.StatusId = 1 
AND (pt.ProjectTypeName = @ProjectTypeName AND DATEDIFF(DAY, cc.DateOfService, GETDATE()) <= 5 )
END
END
ELSE IF @ColumnName = 'Ready For Coding'
BEGIN
IF @ProjectTypeName = 'Ambulatory'
BEGIN
SELECT cc.PatientMRN,cc.PatientFirstName+' '+cc.PatientLastName AS Name
FROM Project p
JOIN ProjectType pt
ON p.ProjectTypeId = pt.ProjectTypeId
JOIN WorkItem wi
ON wi.ProjectId = p.ProjectId
JOIN ClinicalCase cc
ON wi.ClinicalCaseId = cc.ClinicalCaseId
WHERE wi.StatusId < 16 --only not posted items
AND pt.ProjectTypeName = @ProjectTypeName
AND p.ProjectId=@ProjectId
AND wi.StatusId = 1 
AND (pt.ProjectTypeName = @ProjectTypeName AND DATEDIFF(DAY, cc.DateOfService, GETDATE()) > 3 )
END
ELSE IF @ProjectTypeName = 'IP'
BEGIN
SELECT cc.PatientMRN,cc.PatientFirstName+' '+cc.PatientLastName AS Name
FROM Project p
JOIN ProjectType pt
ON p.ProjectTypeId = pt.ProjectTypeId
JOIN WorkItem wi
ON wi.ProjectId = p.ProjectId
JOIN ClinicalCase cc
ON wi.ClinicalCaseId = cc.ClinicalCaseId
WHERE wi.StatusId < 16 --only not posted items
AND pt.ProjectTypeName = @ProjectTypeName
AND p.ProjectId=@ProjectId
AND wi.StatusId = 1 
AND (pt.ProjectTypeName = @ProjectTypeName AND DATEDIFF(DAY, cc.DateOfService, GETDATE()) > 5 )
END
END
ELSE IF @ColumnName = 'In Coding'
BEGIN
SELECT cc.PatientMRN,cc.PatientFirstName+' '+cc.PatientLastName AS Name
FROM Project p
JOIN ProjectType pt
ON p.ProjectTypeId = pt.ProjectTypeId
JOIN WorkItem wi
ON wi.ProjectId = p.ProjectId
JOIN ClinicalCase cc
ON wi.ClinicalCaseId = cc.ClinicalCaseId
WHERE wi.StatusId < 16 --only not posted items
AND pt.ProjectTypeName = @ProjectTypeName
AND p.ProjectId=@ProjectId
AND wi.StatusId IN (2, 14) AND wi.isBlocked = 0
END
ELSE IF @ColumnName = 'In QA'
BEGIN
SELECT cc.PatientMRN,cc.PatientFirstName+' '+cc.PatientLastName AS Name
FROM Project p
JOIN ProjectType pt
ON p.ProjectTypeId = pt.ProjectTypeId
JOIN WorkItem wi
ON wi.ProjectId = p.ProjectId
JOIN ClinicalCase cc
ON wi.ClinicalCaseId = cc.ClinicalCaseId
WHERE wi.StatusId < 16 --only not posted items
AND pt.ProjectTypeName = @ProjectTypeName
AND p.ProjectId=@ProjectId
AND wi.StatusId IN (4, 5, 12) AND wi.isBlocked = 0
END
ELSE IF @ColumnName = 'In ShadowQA'
BEGIN
SELECT cc.PatientMRN,cc.PatientFirstName+' '+cc.PatientLastName AS Name
FROM Project p
JOIN ProjectType pt
ON p.ProjectTypeId = pt.ProjectTypeId
JOIN WorkItem wi
ON wi.ProjectId = p.ProjectId
JOIN ClinicalCase cc
ON wi.ClinicalCaseId = cc.ClinicalCaseId
WHERE wi.StatusId < 16 --only not posted items
AND pt.ProjectTypeName = @ProjectTypeName
AND p.ProjectId=@ProjectId
AND wi.StatusId IN (8, 9, 13) AND wi.isBlocked = 0
END
ELSE IF @ColumnName = 'Ready For Posting'
BEGIN
SELECT cc.PatientMRN,cc.PatientFirstName+' '+cc.PatientLastName AS Name
FROM Project p
JOIN ProjectType pt
ON p.ProjectTypeId = pt.ProjectTypeId
JOIN WorkItem wi
ON wi.ProjectId = p.ProjectId
JOIN ClinicalCase cc
ON wi.ClinicalCaseId = cc.ClinicalCaseId
WHERE wi.StatusId < 16 --only not posted items
AND pt.ProjectTypeName = @ProjectTypeName
AND p.ProjectId=@ProjectId
AND wi.StatusId = 15 AND wi.isBlocked = 0
END
END