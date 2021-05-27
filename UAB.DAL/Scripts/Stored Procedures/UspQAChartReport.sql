ALTER PROCEDURE [dbo].[UspQAChartReport] --NULL,'PerDay'      
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
 SELECT COUNT(*) Total,CONVERT(VARCHAR, CodedDate, 1) [Date]  FROM WorkItem W       
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId    
 WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))    
 AND W.StatusId NOT IN (1,2,3,4,5,13)
 AND (CodedDate >= @StartDate OR (CodedDate = ISNULL(@StartDate,CodedDate)))
 AND (CodedDate <= @EndDate OR (CodedDate = ISNULL(@EndDate,CodedDate)))
 GROUP BY CONVERT(VARCHAR, CodedDate, 1)      
END      
ELSE IF @RangeType = 'PerWeek'      
BEGIN      
 SELECT COUNT(*) Total
 ,DATEPART(ISO_WEEK, CodedDate) [Week]      
 --DATEPART(WEEK, CodedDate)-DATEPART(WEEK, DATEADD(MM, DATEDIFF(MM,0,CodedDate), 0))+ 1 AS [Week],      
 ,DATENAME(month, CodedDate) AS [Month],      
 DATEPART(yyyy, CodedDate) AS [Year]      
 FROM WorkItem W       
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId    
 WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))    
 AND W.StatusId NOT IN (1,2,3,4,5,13)
 AND (CodedDate >= @StartDate OR (CodedDate = ISNULL(@StartDate,CodedDate)))
 AND (CodedDate <= @EndDate OR (CodedDate = ISNULL(@EndDate,CodedDate)))
 GROUP BY DATEPART(ISO_WEEK, CodedDate),      
    DATENAME(month, CodedDate),      
    DATEPART(yyyy , CodedDate)--,DATEPART(WEEK, DATEADD(MM, DATEDIFF(MM,0,CodedDate), 0))      
END      
ELSE IF @RangeType = 'PerMonth'      
BEGIN     
    
;with cte as(     
 SELECT COUNT(*) Total,       
 DATENAME(month, CodedDate) AS [Month],      
 DATEPART(mm, CodedDate) AS [Monthnumber],      
 DATEPART(yyyy, CodedDate) AS [Year]      
 FROM WorkItem W       
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId    
 WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))    
 AND W.StatusId NOT IN (1,2,3,4,5,13)
 AND (CodedDate >= @StartDate OR (CodedDate = ISNULL(@StartDate,CodedDate)))
 AND (CodedDate <= @EndDate OR (CodedDate = ISNULL(@EndDate,CodedDate)))
 GROUP BY DATENAME(month, CodedDate),      
    DATEPART(yyyy , CodedDate),      
    DATEPART(mm, CodedDate)      
)    
 select Total,[Month],[YEAR] from cte  ORDER BY Monthnumber      
END      
      
END      
      
GO