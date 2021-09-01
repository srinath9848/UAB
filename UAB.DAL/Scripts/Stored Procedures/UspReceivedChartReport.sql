CREATE PROCEDURE [dbo].[UspReceivedChartReport]-- NULL,'PerWeek'      
@ProjectId INT = NULL,      
@RangeType VARCHAR(100),  
@StartDate Datetime,    
@EndDate Datetime,  
@TimeZoneOffset DECIMAL(8,2)   
AS      
BEGIN      
    
IF @ProjectId = 0     
 SET @ProjectId = NULL      
      
IF @RangeType = 'PerDay'      
BEGIN  
 SELECT CONVERT(VARCHAR, DATEADD(Second, @TimeZoneOffset * 60 * 60, CreatedDate), 1) [Date],COUNT(*) Total  
 FROM WorkItem W       
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId      
 WHERE W.ProjectId = @ProjectId
 --AND W.StatusId > 2
 AND DATEADD(Second, @TimeZoneOffset * 60 * 60, CreatedDate) >= @StartDate
 AND DATEADD(Second, @TimeZoneOffset * 60 * 60, CreatedDate) <= @EndDate
 GROUP BY CONVERT(VARCHAR, DATEADD(Second, @TimeZoneOffset * 60 * 60, CreatedDate), 1)  
END      
ELSE IF @RangeType = 'PerWeek'      
BEGIN      
 SELECT        
CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, CreatedDate) - 6, CreatedDate)), 101) AS [Week Start Date],
 CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, CreatedDate), CreatedDate)), 101) AS [Week End Date],
 FORMAT(CAST(CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, CreatedDate) - 6, CreatedDate)), 101) AS DATETIME),'dd MMM')
 +'-'+FORMAT(CAST(CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, CreatedDate), CreatedDate)), 101) AS DATETIME),'dd MMM yyy') AS [Start Date - End Date], 
 COUNT(*) Total  
 FROM WorkItem W       
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId      
 WHERE W.ProjectId = @ProjectId
 --AND W.StatusId > 2    
 AND  DATEADD(Second, @TimeZoneOffset * 60 * 60, CreatedDate) >= @StartDate
 AND DATEADD(Second, @TimeZoneOffset * 60 * 60, CreatedDate) <= @EndDate
 GROUP BY --DATEPART(WEEK, CreatedDate),      
 CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, CreatedDate) - 6, CreatedDate)), 101),
 CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, CreatedDate), CreatedDate)), 101),
 FORMAT(CAST(CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, CreatedDate) - 6, CreatedDate)), 101) AS DATETIME),'dd MMM')
 +'-'+FORMAT(CAST(CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, CreatedDate), CreatedDate)), 101) AS DATETIME),'dd MMM yyy')
 order by CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, CreatedDate) - 6, CreatedDate)), 101)
END      
ELSE IF @RangeType = 'PerMonth'      
BEGIN     
    
;with cte as(     
 SELECT      
 DATENAME(month, DATEADD(Second, @TimeZoneOffset * 60 * 60, CreatedDate)) AS [Month],      
 DATEPART(mm, DATEADD(Second, @TimeZoneOffset * 60 * 60, CreatedDate)) AS [Monthnumber],      
 DATEPART(yyyy, DATEADD(Second, @TimeZoneOffset * 60 * 60, CreatedDate)) AS [Year],  
 COUNT(*) Total  
 FROM WorkItem W       
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId      
 WHERE W.ProjectId = @ProjectId
 --AND W.StatusId > 2
 AND DATEADD(Second, @TimeZoneOffset * 60 * 60, CreatedDate) >= @StartDate
 AND DATEADD(Second, @TimeZoneOffset * 60 * 60, CreatedDate) <= @EndDate
 GROUP BY DATENAME(month, DATEADD(Second, @TimeZoneOffset * 60 * 60, CreatedDate)),      
    DATEPART(yyyy , DATEADD(Second, @TimeZoneOffset * 60 * 60, CreatedDate)),      
    DATEPART(mm, DATEADD(Second, @TimeZoneOffset * 60 * 60, CreatedDate))      
)    
 select [Month],[Year],Total from cte  ORDER BY Monthnumber      
END      
      
END
GO