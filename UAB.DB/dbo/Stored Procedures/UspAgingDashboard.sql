CREATE PROCEDURE [dbo].[UspAgingDashboard]  
AS  
BEGIN  
  
SELECT p.[Name] AS 'Project Name', pt.ProjectTypeName AS 'Project Type'  
, SUM(CASE WHEN pt.ProjectTypeName = 'Ambulatory' AND DATEDIFF(DAY, cc.DateOfService, GETUTCDATE()) > 3   
    THEN 1    
   WHEN pt.ProjectTypeName = 'IP' AND DATEDIFF(DAY, cc.DateOfService, GETUTCDATE()) > 5  
    THEN 1   
    ELSE 0 END) AS Due  
, COUNT(1) AS Total  
, SUM(CASE WHEN DATEDIFF(DAY, cc.DateOfService, GETUTCDATE()) BETWEEN 0 AND 3 THEN 1 ELSE 0 END) AS '0-3 Days'  
, SUM(CASE WHEN DATEDIFF(DAY, cc.DateOfService, GETUTCDATE()) BETWEEN 4 AND 5 THEN 1 ELSE 0 END) AS '4-5 Days'  
, SUM(CASE WHEN DATEDIFF(DAY, cc.DateOfService, GETUTCDATE()) BETWEEN 6 AND 7 THEN 1 ELSE 0 END) AS '6-7 Days'  
, SUM(CASE WHEN DATEDIFF(DAY, cc.DateOfService, GETUTCDATE()) > 7  THEN 1 ELSE 0 END) AS '8+ Days'  
FROM Project p  
JOIN ProjectType pt  
ON p.ProjectTypeId = pt.ProjectTypeId  
JOIN WorkItem wi  
ON wi.ProjectId = p.ProjectId  
JOIN ClinicalCase cc  
ON wi.ClinicalCaseId = cc.ClinicalCaseId  
WHERE wi.StatusId < 16 --only not posted items  
GROUP BY p.[Name], pt.ProjectTypeName  
  
  
SELECT p.[Name] AS 'Project Name', pt.ProjectTypeName AS 'Project Type'  
, SUM(CASE WHEN pt.ProjectTypeName = 'Ambulatory' AND DATEDIFF(DAY, cc.DateOfService, GETUTCDATE()) > 3   
    THEN 1    
   WHEN pt.ProjectTypeName = 'IP' AND DATEDIFF(DAY, cc.DateOfService, GETUTCDATE()) > 5  
    THEN 1   
    ELSE 0 END) AS Due  
, COUNT(1) AS Total  
, SUM(CASE WHEN wi.StatusId = 1 AND (  
 (pt.ProjectTypeName = 'Ambulatory' AND DATEDIFF(DAY, cc.DateOfService, GETUTCDATE()) <= 3 )  
 OR  
 (pt.ProjectTypeName = 'IP' AND DATEDIFF(DAY, cc.DateOfService, GETUTCDATE()) <= 5)  
) THEN 1 ELSE 0 END) AS 'Not Ready For Coding'  
, SUM(CASE WHEN wi.StatusId = 1 AND (  
 (pt.ProjectTypeName = 'Ambulatory' AND DATEDIFF(DAY, cc.DateOfService, GETUTCDATE()) > 3 )  
 OR  
 (pt.ProjectTypeName = 'IP' AND DATEDIFF(DAY, cc.DateOfService, GETUTCDATE()) > 5)  
) THEN 1 ELSE 0 END) AS 'Ready For Coding'  
, SUM(CASE WHEN wi.StatusId IN (2, 14) AND wi.isBlocked = 0 THEN 1 ELSE 0 END) AS 'In Coding'  
, SUM(CASE WHEN wi.StatusId IN (4, 5, 12) AND wi.isBlocked = 0 THEN 1 ELSE 0 END) AS 'In QA'  
, SUM(CASE WHEN wi.StatusId IN (8, 9, 13) AND wi.isBlocked = 0 THEN 1 ELSE 0 END) AS 'In ShadowQA'  
, SUM(CASE WHEN wi.StatusId = 15 THEN 1 ELSE 0 END) AS 'Ready For Posting'  
, SUM(CASE WHEN wi.isBlocked = 1 THEN 1 ELSE 0 END) AS 'Blocked'  
FROM Project p  
JOIN ProjectType pt  
ON p.ProjectTypeId = pt.ProjectTypeId  
JOIN WorkItem wi  
ON wi.ProjectId = p.ProjectId  
JOIN ClinicalCase cc  
ON wi.ClinicalCaseId = cc.ClinicalCaseId  
WHERE wi.StatusId < 16 --only not posted items  
GROUP BY p.[Name], pt.ProjectTypeName  
  
END