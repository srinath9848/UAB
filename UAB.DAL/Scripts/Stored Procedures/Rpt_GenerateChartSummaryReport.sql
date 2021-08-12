ALTER PROCEDURE [dbo].[Rpt_GenerateChartSummaryReport] (@ProjectId INT, @DoSStart date, @DoSEnd date, @DateType varchar(15))     
 --1,'3/4/2021 12:00:00 AM','5/19/2021 12:00:00 AM'     
AS  
BEGIN        
  IF @DateType='DateOfService'
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
 --AND ( (@DateType='DateOfService' AND cc.DateOfService >= @DoSStart AND cc.DateOfService <= @DoSEnd )
 --OR (@DateType = 'CreatedDate' AND cc.CreatedDate >= @DoSStart AND cc.CreatedDate <= @DoSEnd ) )
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
--AND ( (@DateType='DateOfService' AND cc.DateOfService >= @DoSStart AND cc.DateOfService <= @DoSEnd )
-- OR (@DateType = 'CreatedDate' AND cc.CreatedDate >= @DoSStart AND cc.CreatedDate <= @DoSEnd ) )
GROUP BY wi.ProjectId, cc.DateOfService, p.[Name]      
)      
SELECT  ProjectName, DateOfService, Received, Coded, Posted, ProviderPosted      
, Posted + ProviderPosted AS TotalPosted      
,InternalBlocked, ExternalBlocked, NotCoded, NotAudited, NotPosted      
,InternalBlocked + ExternalBlocked + NotCoded + NotAudited + NotPosted AS TotalBacklog      
FROM Summary  
END
ELSE
BEGIN
;WITH CCBlockHistory AS (  
 SELECT cc.ClinicalCaseId, bc.BlockType, ROW_NUMBER() OVER (PARTITION BY cc.clinicalcaseid ORDER BY BlockHistoryID DESC) AS rno  
 FROM ClinicalCase cc  
 LEFT JOIN BlockHistory bh  
 ON bh.ClinicalCaseId = cc.ClinicalCaseId  
 LEFT JOIN BlockCategory bc      
 ON bc.BlockCategoryId = bh.BlockCategoryId   
 WHERE cc.ProjectId = @ProjectId  
 AND cc.CreatedDate >= @DoSStart  
 AND cc.CreatedDate <= @DoSEnd  
 --AND ( (@DateType='DateOfService' AND cc.DateOfService >= @DoSStart AND cc.DateOfService <= @DoSEnd )
 --OR (@DateType = 'CreatedDate' AND cc.CreatedDate >= @DoSStart AND cc.CreatedDate <= @DoSEnd ) )
)  
,Summary AS      
(      
SELECT wi.ProjectId, Convert(varchar,cc.CreatedDate,1) AS CreatedDate, p.[Name] AS ProjectName      
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
AND cc.CreatedDate >= @DoSStart  
AND cc.CreatedDate <= @DoSEnd  
--AND ( (@DateType='DateOfService' AND cc.DateOfService >= @DoSStart AND cc.DateOfService <= @DoSEnd )
-- OR (@DateType = 'CreatedDate' AND cc.CreatedDate >= @DoSStart AND cc.CreatedDate <= @DoSEnd ) )
GROUP BY wi.ProjectId, cc.CreatedDate, p.[Name]      
)      
SELECT  ProjectName, CreatedDate, Received, Coded, Posted, ProviderPosted      
, Posted + ProviderPosted AS TotalPosted      
,InternalBlocked, ExternalBlocked, NotCoded, NotAudited, NotPosted      
,InternalBlocked + ExternalBlocked + NotCoded + NotAudited + NotPosted AS TotalBacklog      
FROM Summary  
END
 
END
GO