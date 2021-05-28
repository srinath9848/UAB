
ALTER PROCEDURE [dbo].[UspPendingChartReport] --NULL,'PerWeek'      
@ProjectId INT = NULL,      
@RangeType VARCHAR(100),
@StartDate Date,
@EndDate Date
AS      
BEGIN      
    
IF @ProjectId = 0     
 SET @ProjectId = NULL      
      
IF @RangeType = 'PerDay'      
BEGIN      
 SELECT CONVERT(VARCHAR, BH.CreateDate, 1) [Date],COUNT(*) Total  FROM WorkItem W       
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId    
 INNER JOIN BlockHistory BH on CC.ClinicalcaseId = bh.ClinicalcaseId  
 INNER JOIN BlockCategory BC on BH.BlockCategoryID = BC.BlockCategoryId  
 WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))
 AND (CreateDate >= @StartDate OR (CreateDate = ISNULL(@StartDate,CreateDate)))
 AND (CreateDate <= @EndDate OR (CreateDate = ISNULL(@EndDate,CreateDate)))
 GROUP BY CONVERT(VARCHAR, BH.CreateDate, 1)      
END      
ELSE IF @RangeType = 'PerWeek'      
BEGIN      
 SELECT        
   DATEPART(ISO_WEEK, BH.CreateDate) [Week],
 --DATEPART(WEEK, BH.CreateDate)-DATEPART(WEEK, DATEADD(MM, DATEDIFF(MM,0,BH.CreateDate), 0))+ 1 AS [Week],      
 DATENAME(month, BH.CreateDate) AS [Month],      
 DATEPART(yyyy, BH.CreateDate) AS [Year],
 COUNT(*) Total
 FROM WorkItem W       
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId    
 INNER JOIN BlockHistory BH on CC.ClinicalcaseId = bh.ClinicalcaseId  
 INNER JOIN BlockCategory BC on BH.BlockCategoryID = BC.BlockCategoryId  
 WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))
 AND (CreateDate >= @StartDate OR (CreateDate = ISNULL(@StartDate,CreateDate)))
 AND (CreateDate <= @EndDate OR (CreateDate = ISNULL(@EndDate,CreateDate)))
 GROUP BY DATEPART(ISO_WEEK, BH.CreateDate),      
    DATENAME(month, BH.CreateDate),      
    DATEPART(yyyy , BH.CreateDate)--,DATEPART(WEEK, DATEADD(MM, DATEDIFF(MM,0,BH.CreateDate), 0))      
END      
ELSE IF @RangeType = 'PerMonth'      
BEGIN     
    
;with cte as(     
 SELECT        
 DATENAME(month, BH.CreateDate) AS [Month],      
 DATEPART(mm, BH.CreateDate) AS [Monthnumber],      
 DATEPART(yyyy, BH.CreateDate) AS [Year],
 COUNT(*) Total
 FROM WorkItem W       
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId    
 INNER JOIN BlockHistory BH on CC.ClinicalcaseId = bh.ClinicalcaseId  
 INNER JOIN BlockCategory BC on BH.BlockCategoryID = BC.BlockCategoryId  
 WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))
 AND (CreateDate >= @StartDate OR (CreateDate = ISNULL(@StartDate,CreateDate)))
 AND (CreateDate <= @EndDate OR (CreateDate = ISNULL(@EndDate,CreateDate)))
 GROUP BY DATENAME(month, BH.CreateDate),      
    DATEPART(yyyy , BH.CreateDate),      
    DATEPART(mm, BH.CreateDate)      
)    
 select [Month],[Year],Total from cte  ORDER BY Monthnumber      
END      
      
END      
GO


