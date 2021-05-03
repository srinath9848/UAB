CREATE PROCEDURE UspAgingDashboard  
AS  
BEGIN  
  
SELECT p.[Name] AS ProjectName, pt.ProjectTypeName  
, SUM(CASE WHEN pt.ProjectTypeName = 'Ambulatory' AND DATEDIFF(DAY, cc.DateOfService, GETDATE()) > 3   
    THEN 1    
   WHEN pt.ProjectTypeName = 'IP' AND DATEDIFF(DAY, cc.DateOfService, GETDATE()) > 5  
    THEN 1   
    ELSE 0 END) AS Due  
, COUNT(1) AS Total  
, SUM(CASE WHEN DATEDIFF(DAY, cc.DateOfService, GETDATE()) BETWEEN 0 AND 3 THEN 1 ELSE 0 END) AS [0_3_DAYS]  
, SUM(CASE WHEN DATEDIFF(DAY, cc.DateOfService, GETDATE()) BETWEEN 4 AND 5 THEN 1 ELSE 0 END) AS [4_5_DAYS]  
, SUM(CASE WHEN DATEDIFF(DAY, cc.DateOfService, GETDATE()) BETWEEN 6 AND 7 THEN 1 ELSE 0 END) AS [6_7_DAYS]  
, SUM(CASE WHEN DATEDIFF(DAY, cc.DateOfService, GETDATE()) > 7  THEN 1 ELSE 0 END) AS [8+_DAYS]  
FROM Project p  
JOIN ProjectType pt  
ON p.ProjectTypeId = pt.ProjectTypeId  
JOIN WorkItem wi  
ON wi.ProjectId = p.ProjectId  
JOIN ClinicalCase cc  
ON wi.ClinicalCaseId = cc.ClinicalCaseId  
WHERE wi.StatusId < 16 --only not posted items  
GROUP BY p.[Name], pt.ProjectTypeName  
  
  
SELECT p.[Name] AS ProjectName, pt.ProjectTypeName  
, SUM(CASE WHEN pt.ProjectTypeName = 'Ambulatory' AND DATEDIFF(DAY, cc.DateOfService, GETDATE()) > 3   
    THEN 1    
   WHEN pt.ProjectTypeName = 'IP' AND DATEDIFF(DAY, cc.DateOfService, GETDATE()) > 5  
    THEN 1   
    ELSE 0 END) AS Due  
, COUNT(1) AS Total  
, SUM(CASE WHEN wi.StatusId = 1 AND (  
 (pt.ProjectTypeName = 'Ambulatory' AND DATEDIFF(DAY, cc.DateOfService, GETDATE()) <= 3 )  
 OR  
 (pt.ProjectTypeName = 'IP' AND DATEDIFF(DAY, cc.DateOfService, GETDATE()) <= 5)  
) THEN 1 ELSE 0 END) AS NotReadyForCoding  
, SUM(CASE WHEN wi.StatusId = 1 AND (  
 (pt.ProjectTypeName = 'Ambulatory' AND DATEDIFF(DAY, cc.DateOfService, GETDATE()) > 3 )  
 OR  
 (pt.ProjectTypeName = 'IP' AND DATEDIFF(DAY, cc.DateOfService, GETDATE()) > 5)  
) THEN 1 ELSE 0 END) AS ReadyForCoding  
, SUM(CASE WHEN wi.StatusId IN (2, 14) AND wi.isBlocked = 0 THEN 1 ELSE 0 END) AS InCoding  
, SUM(CASE WHEN wi.StatusId IN (4, 5, 12) AND wi.isBlocked = 0 THEN 1 ELSE 0 END) AS InQA  
, SUM(CASE WHEN wi.StatusId IN (8, 9, 13) AND wi.isBlocked = 0 THEN 1 ELSE 0 END) AS InShadowQA  
, SUM(CASE WHEN wi.StatusId = 15 THEN 1 ELSE 0 END) AS ReadyForPosting  
, SUM(CASE WHEN wi.isBlocked = 1 THEN 1 ELSE 0 END) AS Blocked  
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