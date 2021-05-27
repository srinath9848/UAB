ALTER PROCEDURE [dbo].[UspPostedChartReport] --NULL,'PerDay'  
@ProjectId INT = NULL,  
@RangeType VARCHAR(100),
@StartDate Date,
@EndDate Date
AS  
BEGIN  
  
  
IF @RangeType = 'PerDay'  
BEGIN  
 SELECT   
 CONVERT(VARCHAR, VersionDate, 1) AS [Date]  
 , COUNT(*) AS Total  
 , COUNT(*)*100/SUM(COUNT(*)) OVER() AS [Percentage]  
 FROM WorkItem W   
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId  
 INNER JOIN Version v ON v.ClinicalCaseId = CC.ClinicalcaseId  
 WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))  
 AND v.StatusId in (16,17)
 AND (VersionDate >= @StartDate OR (VersionDate = ISNULL(@StartDate,VersionDate)))
 AND (VersionDate <= @EndDate OR (VersionDate = ISNULL(@EndDate,VersionDate)))
 GROUP BY CONVERT(VARCHAR, VersionDate, 1)  
END  
ELSE IF @RangeType = 'PerWeek'  
BEGIN  
 SELECT DATEPART(ISO_WEEK, VersionDate) AS [Week Number],  
 DATENAME(month, VersionDate) AS [Month Name],  
 DATEPART(yyyy, VersionDate) AS [Year],  
 COUNT(*) AS Total,  
 COUNT(*)*100/SUM(COUNT(*)) OVER() AS [Percentage]  
 FROM WorkItem W   
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId  
 INNER JOIN Version v ON v.ClinicalCaseId = CC.ClinicalcaseId  
 WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))  
 AND v.StatusId in (16,17)
 AND (VersionDate >= @StartDate OR (VersionDate = ISNULL(@StartDate,VersionDate)))
 AND (VersionDate <= @EndDate OR (VersionDate = ISNULL(@EndDate,VersionDate)))
 GROUP BY DATEPART(ISO_WEEK, VersionDate),  
    DATENAME(month, VersionDate),  
    DATEPART(yyyy , VersionDate)--,DATEPART(WEEK, DATEADD(MM, DATEDIFF(MM,0,VersionDate), 0))  
END  
ELSE IF @RangeType = 'PerMonth'  
BEGIN   
 SELECT DATENAME(month, VersionDate) AS [Month Name],  
 --DATEPART(mm, VersionDate) AS [Month Number],  
 DATEPART(yyyy, VersionDate) AS [Year],  
 COUNT(*) AS Total,  
 COUNT(*)*100/SUM(COUNT(*)) OVER() AS [Percentage]  
 FROM WorkItem W   
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId  
 INNER JOIN Version v ON v.ClinicalCaseId = CC.ClinicalcaseId  
 WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))  
 AND v.StatusId in (16,17)
 AND (VersionDate >= @StartDate OR (VersionDate = ISNULL(@StartDate,VersionDate)))
 AND (VersionDate <= @EndDate OR (VersionDate = ISNULL(@EndDate,VersionDate)))
 GROUP BY DATENAME(month, VersionDate),  
    DATEPART(yyyy , VersionDate),  
    DATEPART(mm, VersionDate)  
 ORDER BY DATEPART(mm, VersionDate)  
END  
  
END  
  
GO