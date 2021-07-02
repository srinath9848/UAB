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
  DATEPART(ISO_WEEK, DATEADD(Second, @TimeZoneOffset * 60 * 60, CreatedDate)) [Week]  
 --DATEPART(WEEK, CreatedDate)-DATEPART(WEEK, DATEADD(MM, DATEDIFF(MM,0,CreatedDate), 0))+ 1 AS [Week],      
 ,DATENAME(month, DATEADD(Second, @TimeZoneOffset * 60 * 60, CreatedDate)) AS [Month],      
 DATEPART(yyyy, DATEADD(Second, @TimeZoneOffset * 60 * 60, CreatedDate)) AS [Year],  
 COUNT(*) Total  
 FROM WorkItem W       
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId      
 WHERE W.ProjectId = @ProjectId
 --AND W.StatusId > 2    
 AND  DATEADD(Second, @TimeZoneOffset * 60 * 60, CreatedDate) >= @StartDate
 AND DATEADD(Second, @TimeZoneOffset * 60 * 60, CreatedDate) <= @EndDate
 GROUP BY --DATEPART(WEEK, CreatedDate),      
    DATEPART(ISO_WEEK, DATEADD(Second, @TimeZoneOffset * 60 * 60, CreatedDate)) ,  
 DATENAME(month, DATEADD(Second, @TimeZoneOffset * 60 * 60, CreatedDate)),      
    DATEPART(yyyy , DATEADD(Second, @TimeZoneOffset * 60 * 60, CreatedDate))--,DATEPART(WEEK, DATEADD(MM, DATEDIFF(MM,0,CreatedDate), 0))      
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