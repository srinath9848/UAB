CREATE PROCEDURE [dbo].[Rpt_GenerateEMLevellingReport] (@ProjectId INT, @DoSStart date, @DoSEnd date)    
AS    
BEGIN    
    
DECLARE @cols AS NVARCHAR(MAX),    
    @query  AS NVARCHAR(MAX)    
    
select @cols = STUFF((SELECT DISTINCT ',' + QUOTENAME(DateOfService)     
                    FROM ( SELECT DISTINCT LEFT(CONVERT(varchar, DateOfService, 101), 5) AS DateOfService    
       FROM ClinicalCase     
       WHERE ProjectId = @ProjectId     
       AND DateOfService >= @DoSStart AND DateOfService <= @DoSEnd    
       ) AS x    
            FOR XML PATH(''), TYPE    
            ).value('.', 'NVARCHAR(MAX)')     
        ,1,1,'')    

set @query = 'SELECT EMLevel, EMCode,' + @cols + ' from     
             (    
    SELECT e.EMLevel, e.EMCode, LEFT(CONVERT(varchar, DateOfService, 101), 5) AS DateOfService, COUNT(cc.ClinicalCaseId) AS EMClaimCount    
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
    WHERE cc.ProjectId = ' + CAST(@ProjectId AS varchar) + '     
    AND cc.DateOfService >= ''' + CAST(@DoSStart AS varchar) + '''     
    AND cc.DateOfService <= ''' + CAST(@DoSEnd AS varchar) + '''    
	AND v.rn = 1
    GROUP BY e.EMLevel, e.EMCode, LEFT(CONVERT(varchar, cc.DateOfService, 101), 5)    
            ) x    
            pivot     
            (    
                sum(EMClaimCount)    
                for DateOfService in (' + @cols + ')    
            ) p '    
    
EXECUTE(@query)    
  
set @query = 'SELECT EMLevel, EMCode,' + @cols + ' from     
             (  
	SELECT   
	EMLevel, EMCode, DateOfService  
	, CAST(ROUND((EMCodeCount * 100.0 / EMLevelSum), 2) AS numeric(5,2)) AS EMDistributionPercent  
	FROM (  
	SELECT e.EMLevel, e.EMCode  
	, LEFT(CONVERT(varchar, DateOfService, 101), 5) AS DateOfService  
	, COUNT(cc.ClinicalCaseId) AS EMCodeCount  
	, SUM(COUNT(cc.ClinicalCaseId)) OVER(PARTITION BY EMLevel, LEFT(CONVERT(varchar, cc.DateOfService, 101), 5)) AS EMLevelSum  
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
	WHERE cc.ProjectId = ' + CAST(@ProjectId AS varchar) + '     
	AND cc.DateOfService >= ''' + CAST(@DoSStart AS varchar) + '''     
	AND cc.DateOfService <= ''' + CAST(@DoSEnd AS varchar) + '''    
	AND v.rn = 1
	GROUP BY e.EMLevel, e.EMCode, LEFT(CONVERT(varchar, cc.DateOfService, 101), 5)  
	) AS r  
) x    
pivot     
(    
	sum(EMDistributionPercent)    
	for DateOfService in (' + @cols + ')    
) p '    
    
EXECUTE(@query)    
  
END