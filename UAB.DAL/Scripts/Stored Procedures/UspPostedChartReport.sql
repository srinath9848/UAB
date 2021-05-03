  
  
  
  
CREATE PROCEDURE [dbo].[UspPostedChartReport] --NULL,'PerDay'  
@ProjectId INT = NULL,  
@RangeType VARCHAR(100)  
AS  
BEGIN  
  
  
IF @RangeType = 'PerDay'  
BEGIN  
 SELECT COUNT(*) Total,CONVERT(VARCHAR, VersionDate, 1) Date,COUNT(*)*100/SUM(COUNT(*)) OVER() as Percentage  FROM WorkItem W   
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId  
 INNER JOIN Version v ON v.ClinicalCaseId = CC.ClinicalcaseId  
 WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))  
 AND v.StatusId in (16,17)  
 GROUP BY CONVERT(VARCHAR, VersionDate, 1)  
END  
ELSE IF @RangeType = 'PerWeek'  
BEGIN  
 SELECT COUNT(*) Total,   
 DATEPART(WEEK, VersionDate)-DATEPART(WEEK, DATEADD(MM, DATEDIFF(MM,0,VersionDate), 0))+ 1 AS WeekOfMonth,  
 DATENAME(month, VersionDate) AS [MonthName],  
 DATEPART(yyyy, VersionDate) AS [Year],  
 COUNT(*)*100/SUM(COUNT(*)) OVER() as Percentage  
 FROM WorkItem W   
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId  
 INNER JOIN Version v ON v.ClinicalCaseId = CC.ClinicalcaseId  
 WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))  
 AND v.StatusId in (16,17)  
 GROUP BY DATEPART(WEEK, VersionDate),  
    DATENAME(month, VersionDate),  
    DATEPART(yyyy , VersionDate),DATEPART(WEEK, DATEADD(MM, DATEDIFF(MM,0,VersionDate), 0))  
END  
ELSE IF @RangeType = 'PerMonth'  
BEGIN  
 SELECT COUNT(*) Total,   
 DATENAME(month, VersionDate) AS [MonthName],  
 DATEPART(mm, VersionDate) AS [Monthnumber],  
 DATEPART(yyyy, VersionDate) AS [YEAR],  
 COUNT(*)*100/SUM(COUNT(*)) OVER() as Percentage  
 FROM WorkItem W   
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId  
 INNER JOIN Version v ON v.ClinicalCaseId = CC.ClinicalcaseId  
 WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))  
 AND v.StatusId in (16,17)  
 GROUP BY DATENAME(month, VersionDate),  
    DATEPART(yyyy , VersionDate),  
    DATEPART(mm, VersionDate)  
 ORDER BY Monthnumber  
END  
  
END  
  