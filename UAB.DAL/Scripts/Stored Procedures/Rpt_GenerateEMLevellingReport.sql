/****** Object:  StoredProcedure [dbo].[Rpt_GenerateEMLevellingReport]    Script Date: 8/11/2021 11:00:29 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[Rpt_GenerateEMLevellingReport] (@ProjectId INT, @DoSStart date, @DoSEnd date, @DateType varchar(15), @ListId INT=NULL, @ProviderId INT=NULL)      
AS      
BEGIN   
  
DECLARE @cols AS NVARCHAR(MAX),  
@query AS NVARCHAR(MAX)  
  
SELECT @cols = STUFF((SELECT DISTINCT ',' + QUOTENAME(TheDate)  
FROM ( SELECT DISTINCT LEFT(CONVERT(varchar, CASE WHEN @DateType = 'CreatedDate' THEN CreatedDate ELSE DateOfService END, 101), 5) AS TheDate  
FROM ClinicalCase  
WHERE ProjectId = @ProjectId  
--AND (ListId = @ListId OR @ListId IS NULL)  
--AND (ProviderId = @ProviderId OR @ProviderId IS NULL)  
AND ( (@DateType = 'DateOfService' AND DateOfService >= @DoSStart AND DateOfService <= @DoSEnd )  
OR (@DateType = 'CreatedDate' AND CAST(CreatedDate AS DATE) >= @DoSStart AND CAST(CreatedDate AS DATE) <= @DoSEnd ) )  
) AS x  
FOR XML PATH(''), TYPE  
).value('.', 'NVARCHAR(MAX)')  
,1,1,'')  
  
SET @query = 'SELECT EMLevel, EMCode,' + @cols + ' from       
             (      
    SELECT e.EMLevel, e.EMCode, LEFT(CONVERT(varchar, CASE WHEN ''' + CAST(@DateType AS varchar) + ''' = ''CreatedDate'' THEN cc.CreatedDate ELSE cc.DateOfService END, 101), 5) AS [Date]  
       , COUNT(cc.ClinicalCaseId) AS EMClaimCount      
    FROM ClinicalCase cc      
    JOIN WorkItem wi      
    ON cc.ClinicalCaseId = wi.ClinicalCaseId      
    JOIN CptCode cpt      
    ON wi.ClinicalCaseId = cpt.ClinicalCaseId  
       JOIN (                
                     SELECT ClinicalCaseId, VersionId, ROW_NUMBER() OVER(PARTITION BY ClinicalCaseId ORDER BY VersionId DESC) AS rn  
                     FROM dbo.[Version] WHERE StatusId = 3  
    ) AS v   
       ON v.ClinicalCaseId = cpt.ClinicalCaseId                
    AND v.VersionID = cpt.VersionID  
    RIGHT JOIN EMCodeLevel e      
    ON e.EMCode = cpt.CptCode      
    WHERE cc.ProjectId = ' + CAST(@ProjectId AS varchar) + ' '  
  
 IF (@ListId IS NOT NULL)  
 SET @query = @query + ' AND cc.ListId = ''' + CAST(@ListId AS varchar) + ''''  
   
 IF (@ProviderId IS NOT NULL)  
 SET @query = @query + ' AND cc.ProviderId = ''' + CAST(@ProviderId AS varchar) + ''''  
  
    SET @query = @query + ' AND ( (''' + CAST(@DateType AS varchar) + ''' = ''DateOfService'' AND DateOfService >= ''' + CAST(@DoSStart AS varchar) + ''' AND DateOfService <= ''' + CAST(@DoSEnd AS varchar) + ''' )  
          OR (''' + CAST(@DateType AS varchar) + ''' = ''CreatedDate'' AND CAST(CreatedDate AS DATE) >= ''' + CAST(@DoSStart AS varchar) + ''' AND CAST(CreatedDate AS DATE) <= ''' + CAST(@DoSEnd AS varchar) + ''' ) )  
       AND v.rn = 1  
    GROUP BY e.EMLevel, e.EMCode, LEFT(CONVERT(varchar, CASE WHEN ''' + CAST(@DateType AS varchar) + ''' = ''CreatedDate'' THEN cc.CreatedDate ELSE cc.DateOfService END, 101), 5)     
            ) x      
            pivot       
            (      
                sum(EMClaimCount)      
                for [Date] in (' + @cols + ')      
            ) p '      
      
EXECUTE(@query)  
  
SET @query = 'SELECT EMLevel, EMCode,' + @cols + ' from       
             (    
 SELECT     
 EMLevel, EMCode, [Date]  
 , CAST(ROUND((EMCodeCount * 100.0 / EMLevelSum), 2) AS numeric(5,2)) AS EMDistributionPercent    
 FROM (    
 SELECT e.EMLevel, e.EMCode    
 , LEFT(CONVERT(varchar, CASE WHEN ''' + CAST(@DateType AS varchar) + ''' = ''CreatedDate'' THEN cc.CreatedDate ELSE cc.DateOfService END, 101), 5) AS [Date]    
 , COUNT(cc.ClinicalCaseId) AS EMCodeCount    
 , SUM(COUNT(cc.ClinicalCaseId)) OVER(PARTITION BY EMLevel, LEFT(CONVERT(varchar, CASE WHEN ''' + CAST(@DateType AS varchar) + ''' = ''CreatedDate'' THEN cc.CreatedDate ELSE cc.DateOfService END, 101), 5)) AS EMLevelSum    
 FROM ClinicalCase cc      
 JOIN WorkItem wi      
 ON cc.ClinicalCaseId = wi.ClinicalCaseId      
 JOIN CptCode cpt      
 ON wi.ClinicalCaseId = cpt.ClinicalCaseId  
 JOIN (                
   SELECT ClinicalCaseId, VersionId, ROW_NUMBER() OVER(PARTITION BY ClinicalCaseId ORDER BY VersionId DESC) AS rn  
   FROM dbo.[Version] WHERE StatusId = 3  
    ) AS v   
 ON v.ClinicalCaseId = cpt.ClinicalCaseId                
    AND v.VersionID = cpt.VersionID  
 RIGHT JOIN EMCodeLevel e      
 ON e.EMCode = cpt.CptCode      
 WHERE cc.ProjectId = ' + CAST(@ProjectId AS varchar) + ' '  
  
 IF (@ListId IS NOT NULL)  
 SET @query = @query + ' AND cc.ListId = ''' + CAST(@ListId AS varchar) + ''''  
   
 IF (@ProviderId IS NOT NULL)  
 SET @query = @query + ' AND cc.ProviderId = ''' + CAST(@ProviderId AS varchar) + ''''  
  
    SET @query = @query + ' AND ( (''' + CAST(@DateType AS varchar) + ''' = ''DateOfService'' AND DateOfService >= ''' + CAST(@DoSStart AS varchar) + ''' AND DateOfService <= ''' + CAST(@DoSEnd AS varchar) + ''' )  
        OR (''' + CAST(@DateType AS varchar) + ''' = ''CreatedDate'' AND CAST(CreatedDate AS DATE) >= ''' + CAST(@DoSStart AS varchar) + ''' AND CAST(CreatedDate AS DATE) <= ''' + CAST(@DoSEnd AS varchar) + ''' ) )    
 AND v.rn = 1  
 GROUP BY e.EMLevel, e.EMCode, LEFT(CONVERT(varchar, CASE WHEN ''' + CAST(@DateType AS varchar) + ''' = ''CreatedDate'' THEN cc.CreatedDate ELSE cc.DateOfService END, 101), 5)     
 ) AS r    
) x      
pivot       
(      
 sum(EMDistributionPercent)      
 for [Date] in (' + @cols + ')      
) p '      
      
EXECUTE(@query)      
  
END  
GO


