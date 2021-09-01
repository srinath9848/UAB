CREATE PROCEDURE [dbo].[UspPostedChartReport] --NULL,'PerDay'      
@ProjectId INT = NULL,      
@RangeType VARCHAR(100),    
@StartDate Datetime,    
@EndDate Datetime,  
@TimeZoneOffset DECIMAL(8,2)     
AS      
BEGIN      
      
      
IF @RangeType = 'PerDay'      
BEGIN      
 SELECT    distinct   
 CONVERT(VARCHAR, DATEADD(Second, @TimeZoneOffset * 60 * 60, VersionDate), 1) AS [Date]      
 , COUNT(*) AS Total      
 , COUNT(*)*100/SUM(COUNT(*)) OVER() AS [Percentage]      
 FROM WorkItem W       
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId      
 INNER JOIN Version v ON v.ClinicalCaseId = CC.ClinicalcaseId      
 WHERE W.ProjectId = @ProjectId     
 AND v.StatusId in (16)    
 AND DATEADD(Second, @TimeZoneOffset * 60 * 60, VersionDate) >= @StartDate  
 AND DATEADD(Second, @TimeZoneOffset * 60 * 60, VersionDate) <= @EndDate  
 GROUP BY CONVERT(VARCHAR, DATEADD(Second, @TimeZoneOffset * 60 * 60, VersionDate), 1)     
END      
ELSE IF @RangeType = 'PerWeek'      
BEGIN      
 SELECT distinct 
 CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, VersionDate) - 6, VersionDate)), 101) AS [Week Start Date],
 CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, VersionDate), VersionDate)), 101) AS [Week End Date],
 FORMAT(CAST(CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, VersionDate) - 6, VersionDate)), 101) AS DATETIME),'dd MMM')
 +'-'+FORMAT(CAST(CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, VersionDate), VersionDate)), 101) AS DATETIME),'dd MMM yyy') AS [Start Date - End Date],      
 COUNT(*) AS Total,      
 COUNT(*)*100/SUM(COUNT(*)) OVER() AS [Percentage]      
 FROM WorkItem W       
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId      
 INNER JOIN Version v ON v.ClinicalCaseId = CC.ClinicalcaseId      
 WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))      
 AND v.StatusId in (16)    
 AND DATEADD(Second, @TimeZoneOffset * 60 * 60, VersionDate) >= @StartDate  
 AND DATEADD(Second, @TimeZoneOffset * 60 * 60, VersionDate) <= @EndDate    
 GROUP BY 
 CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, VersionDate) - 6, VersionDate)), 101),
 CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, VersionDate), VersionDate)), 101),
 FORMAT(CAST(CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, VersionDate) - 6, VersionDate)), 101) AS DATETIME),'dd MMM')
 +'-'+FORMAT(CAST(CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, VersionDate), VersionDate)), 101) AS DATETIME),'dd MMM yyy')
 order by CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, VersionDate) - 6, VersionDate)), 101)
END      
ELSE IF @RangeType = 'PerMonth'      
BEGIN       
 SELECT distinct 
 DATEPART(mm, DATEADD(Second, @TimeZoneOffset * 60 * 60, VersionDate)) AS [Month Number],
 DATENAME(month, DATEADD(Second, @TimeZoneOffset * 60 * 60, VersionDate)) AS [Month Name],      
 --DATEPART(mm, VersionDate) AS [Month Number],      
 DATEPART(yyyy, DATEADD(Second, @TimeZoneOffset * 60 * 60, VersionDate)) AS [Year],      
 COUNT(*) AS Total,      
 COUNT(*)*100/SUM(COUNT(*)) OVER() AS [Percentage]      
 FROM WorkItem W       
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId      
 INNER JOIN Version v ON v.ClinicalCaseId = CC.ClinicalcaseId      
 WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))      
 AND v.StatusId in (16)    
 AND DATEADD(Second, @TimeZoneOffset * 60 * 60, VersionDate) >= @StartDate  
 AND DATEADD(Second, @TimeZoneOffset * 60 * 60, VersionDate) <= @EndDate  
 GROUP BY DATENAME(month, DATEADD(Second, @TimeZoneOffset * 60 * 60, VersionDate)),      
    DATEPART(yyyy , DATEADD(Second, @TimeZoneOffset * 60 * 60, VersionDate)),      
    DATEPART(mm, DATEADD(Second, @TimeZoneOffset * 60 * 60, VersionDate))      
 ORDER BY DATEPART(mm, DATEADD(Second, @TimeZoneOffset * 60 * 60, VersionDate))      
END      
      
END      
GO