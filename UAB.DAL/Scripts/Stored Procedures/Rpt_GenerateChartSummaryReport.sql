CREATE PROCEDURE [dbo].[Rpt_GenerateChartSummaryReport] (@ProjectId INT, @DoSStart date, @DoSEnd date)     
 --1,'3/4/2021 12:00:00 AM','5/19/2021 12:00:00 AM'     
AS  
BEGIN        
  
;WITH CCBlockHistory AS (  
 SELECT cc.ClinicalCaseId, bc.BlockType, ROW_NUMBER() OVER (PARTITION BY cc.clinicalcaseid ORDER BY BlockHistoryID DESC) AS rno  
 FROM ClinicalCase cc  
 LEFT JOIN BlockHistory bh  
 ON bh.ClinicalCaseId = cc.ClinicalCaseId  
 LEFT JOIN BlockCategory bc      
 ON bc.BlockCategoryId = bh.BlockCategoryId   
 WHERE cc.ProjectId = @ProjectId  
 AND cc.DateOfService >= @DoSStart  
 AND cc.DateOfService <= @DoSEnd  
)  
,Summary AS      
(      
SELECT wi.ProjectId, Convert(varchar,cc.DateOfService,1) AS DateOfService, p.[Name] AS ProjectName      
, COUNT(DISTINCT cc.ClinicalCaseId) AS Received      
, SUM(CASE WHEN wi.StatusId = 4 THEN 1 ELSE 0 END) AS Coded      
, SUM(CASE WHEN wi.StatusId = 16 THEN 1 ELSE 0 END) AS Posted      
, SUM(CASE WHEN wi.StatusId = 17 THEN 1 ELSE 0 END) AS ProviderPosted      
, SUM(CASE WHEN wi.IsBlocked = 1 AND ISNULL(BlockType, '') = 'Internal' THEN 1 ELSE 0 END) AS InternalBlocked      
, SUM(CASE WHEN wi.IsBlocked = 1 AND ISNULL(BlockType, '') = 'External' THEN 1 ELSE 0 END) AS ExternalBlocked      
, SUM(CASE WHEN wi.StatusId IN (1, 2) AND wi.IsBlocked = 0 THEN 1 ELSE 0 END) AS NotCoded --ReadyForCoding,CodingInProgress      
, SUM(CASE WHEN wi.StatusId > 3 AND wi.StatusId < 15 AND wi.IsBlocked = 0 THEN 1 ELSE 0 END) AS NotAudited --Audit      
, SUM(CASE WHEN wi.StatusId = 15 AND wi.IsBlocked = 0 THEN 1 ELSE 0 END) AS NotPosted --ReadyForPosting      
FROM ClinicalCase cc  
JOIN WorkItem wi      
ON wi.ClinicalCaseId = cc.ClinicalCaseId      
JOIN Project p      
ON p.ProjectId = wi.ProjectId  
LEFT JOIN CCBlockHistory bh  
ON bh.ClinicalCaseId = cc.ClinicalCaseId  
AND bh.rno = 1  
WHERE cc.ProjectId = @ProjectId  
AND cc.DateOfService >= @DoSStart  
AND cc.DateOfService <= @DoSEnd  
GROUP BY wi.ProjectId, cc.DateOfService, p.[Name]      
)      
SELECT  ProjectName, DateOfService, Received, Coded, Posted, ProviderPosted      
, Posted + ProviderPosted AS TotalPosted      
,InternalBlocked, ExternalBlocked, NotCoded, NotAudited, NotPosted      
,InternalBlocked + ExternalBlocked + NotCoded + NotAudited + NotPosted AS TotalBacklog      
FROM Summary      
      
      
--;WITH Summary AS      
--(      
--SELECT wi.ProjectId, cc.DateOfService      
--, COUNT(DISTINCT cc.ClinicalCaseId) AS Received      
--, COUNT(c.ClinicalCaseId) AS TotalClaims      
--, SUM(CASE WHEN wi.StatusId = 16 THEN 1 ELSE 0 END) AS Posted      
--, SUM(CASE WHEN wi.StatusId = 17 THEN 1 ELSE 0 END) AS ProviderPosted      
----, SUM(CASE WHEN wi.StatusId IN (16, 17) THEN 1 ELSE 0 END) AS TotalPosted      
--, SUM(CASE WHEN wi.IsBlocked = 1 AND ISNULL(bc.BlockType, '') = 'External' THEN 1 ELSE 0 END) AS Question      
--, SUM(CASE WHEN wi.StatusId IN (6, 2) THEN 1 ELSE 0 END) AS NotCoded --ReadyForCoding,CodingInProgress      
--, SUM(CASE WHEN wi.StatusId = 15 THEN 1 ELSE 0 END) AS NotPosted --ReadyForPosting      
--FROM ClinicalCase cc      
--JOIN WorkItem wi      
--ON wi.ClinicalCaseId = cc.ClinicalCaseId      
--JOIN [Status] s      
--ON wi.StatusId = s.StatusId      
--LEFT JOIN Claim c      
--ON c.ClinicalCaseId = cc.ClinicalCaseId      
--LEFT JOIN [Version] v      
--ON v.ClinicalCaseId = c.ClinicalCaseId      
--AND v.StatusId = 3 --CodingCompleted      
--LEFT JOIN BlockHistory bh      
--ON bh.ClinicalCaseId = cc.ClinicalCaseId      
--LEFT JOIN BlockCategory bc      
--ON bc.BlockCategoryId = bh.BlockCategoryId      
--WHERE cc.ProjectId = @ProjectId      
--AND cc.DateOfService >= @DoSStart      
--AND cc.DateOfService <= @DoSEnd      
--GROUP BY wi.ProjectId, cc.DateOfService      
--)      
--SELECT ProjectId, DateOfService, Received, TotalClaims, Posted, ProviderPosted      
--, Posted + ProviderPosted AS TotalPosted      
--,Question, NotCoded, NotPosted      
--,Question + NotCoded + NotPosted AS TotalBacklog      
--FROM Summary      
      
END
GO


