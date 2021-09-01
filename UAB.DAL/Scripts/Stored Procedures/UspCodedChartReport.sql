CREATE PROCEDURE [dbo].[UspCodedChartReport] --1,'PerDay','6/7/2021 12:00:00 AM','6/7/2021 11:59:00 PM',5.5
@ProjectId INT = NULL,          
@RangeType VARCHAR(100),    
@StartDate DateTIME,    
@EndDate DateTIME,
@TimeZoneOffset DECIMAL(8,2)     
AS          
BEGIN          
        
IF @ProjectId = 0         
 SET @ProjectId = NULL          
          
IF @RangeType = 'PerDay'          
BEGIN          
 SELECT CONVERT(VARCHAR, DATEADD(Second, @TimeZoneOffset * 60 * 60, CodedDate), 1) [Date]
 ,COUNT(*) Total  FROM WorkItem W           
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId        
 WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))        
 AND W.StatusId NOT IN (1,2)    
 AND DATEADD(Second, @TimeZoneOffset * 60 * 60, CodedDate) >= @StartDate
 AND DATEADD(Second, @TimeZoneOffset * 60 * 60, CodedDate) <= @EndDate
 GROUP BY CONVERT(VARCHAR, DATEADD(Second, @TimeZoneOffset * 60 * 60, CodedDate), 1)        
END          
ELSE IF @RangeType = 'PerWeek'          
BEGIN          
 SELECT   
 CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, CodedDate) - 6, CodedDate)), 101) AS [Week Start Date],
 CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, CodedDate), CodedDate)), 101) AS [Week End Date],
 FORMAT(CAST(CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, CodedDate) - 6, CodedDate)), 101) AS DATETIME),'dd MMM')
 +'-'+FORMAT(CAST(CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, CodedDate), CodedDate)), 101) AS DATETIME),'dd MMM yyy') AS [Start Date - End Date],
 COUNT(*) Total    
 FROM WorkItem W           
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId        
 WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))        
 AND W.StatusId NOT IN (1,2)    
 AND DATEADD(Second, @TimeZoneOffset * 60 * 60, CodedDate) >= @StartDate    
 AND DATEADD(Second, @TimeZoneOffset * 60 * 60, CodedDate) <= @EndDate 
 GROUP BY CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, CodedDate) - 6, CodedDate)), 101),
 CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, CodedDate), CodedDate)), 101),
 FORMAT(CAST(CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, CodedDate) - 6, CodedDate)), 101) AS DATETIME),'dd MMM')
 +'-'+FORMAT(CAST(CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, CodedDate), CodedDate)), 101) AS DATETIME),'dd MMM yyy')
 order by Convert(datetime,CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, CodedDate) - 6, CodedDate)), 101),101)
 END          
ELSE IF @RangeType = 'PerMonth'          
BEGIN         
        
;with cte as(         
 SELECT            
 DATENAME(month, DATEADD(Second, @TimeZoneOffset * 60 * 60, CodedDate)) AS [Month],          
 DATEPART(mm, DATEADD(Second, @TimeZoneOffset * 60 * 60, CodedDate)) AS [Monthnumber],          
 DATEPART(yyyy, DATEADD(Second, @TimeZoneOffset * 60 * 60, CodedDate)) AS [Year],    
 COUNT(*) Total    
 FROM WorkItem W           
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId        
 WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))        
 AND W.StatusId NOT IN (1,2)    
 AND DATEADD(Second, @TimeZoneOffset * 60 * 60, CodedDate) >= @StartDate    
 AND DATEADD(Second, @TimeZoneOffset * 60 * 60, CodedDate) <= @EndDate    
 GROUP BY DATENAME(month, DATEADD(Second, @TimeZoneOffset * 60 * 60, CodedDate)),          
    DATEPART(yyyy , DATEADD(Second, @TimeZoneOffset * 60 * 60, CodedDate)),          
    DATEPART(mm, DATEADD(Second, @TimeZoneOffset * 60 * 60, CodedDate))          
)        
 select [Month],[Year],Total from cte  ORDER BY Monthnumber          
END          
          
END 
GO