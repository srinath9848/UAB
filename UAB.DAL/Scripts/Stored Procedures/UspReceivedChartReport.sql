
CREATE PROCEDURE [dbo].[UspReceivedChartReport]-- NULL,'PerWeek'    
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
 SELECT CONVERT(VARCHAR, createddate, 1) [Date],COUNT(*) Total  FROM WorkItem W     
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId    
 WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))  
 AND (createddate >= @StartDate OR (CreatedDate = ISNULL(@StartDate,CreatedDate)))  
 AND (createddate <= @EndDate OR (CreatedDate = ISNULL(@EndDate,CreatedDate)))  
 GROUP BY CONVERT(VARCHAR, CreatedDate, 1)
END    
ELSE IF @RangeType = 'PerWeek'    
BEGIN    
 SELECT      
  DATEPART(ISO_WEEK, CreatedDate) [Week]
 --DATEPART(WEEK, CreatedDate)-DATEPART(WEEK, DATEADD(MM, DATEDIFF(MM,0,CreatedDate), 0))+ 1 AS [Week],    
 ,DATENAME(month, CreatedDate) AS [Month],    
 DATEPART(yyyy, CreatedDate) AS [Year],
 COUNT(*) Total
 FROM WorkItem W     
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId    
 WHERE W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId))  
AND (createddate >= @StartDate OR (CreatedDate = ISNULL(@StartDate,CreatedDate)))  
 AND (createddate <= @EndDate OR (CreatedDate = ISNULL(@EndDate,CreatedDate)))  
 GROUP BY --DATEPART(WEEK, CreatedDate),    
    DATEPART(ISO_WEEK, CreatedDate) ,
	DATENAME(month, CreatedDate),    
    DATEPART(yyyy , CreatedDate)--,DATEPART(WEEK, DATEADD(MM, DATEDIFF(MM,0,CreatedDate), 0))    
END    
ELSE IF @RangeType = 'PerMonth'    
BEGIN   
  
;with cte as(   
 SELECT    
 DATENAME(month, CreatedDate) AS [Month],    
 DATEPART(mm, CreatedDate) AS [Monthnumber],    
 DATEPART(yyyy, CreatedDate) AS [Year],
 COUNT(*) Total
 FROM WorkItem W     
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId    
 WHERE W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId))
 AND (createddate >= @StartDate OR (CreatedDate = ISNULL(@StartDate,CreatedDate)))  
 AND (createddate <= @EndDate OR (CreatedDate = ISNULL(@EndDate,CreatedDate)))  
 GROUP BY DATENAME(month, CreatedDate),    
    DATEPART(yyyy , CreatedDate),    
    DATEPART(mm, CreatedDate)    
)  
 select [Month],[Year],Total from cte  ORDER BY Monthnumber    
END    
    
END    
GO


