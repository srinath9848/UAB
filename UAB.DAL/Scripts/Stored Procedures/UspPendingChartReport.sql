CREATE PROCEDURE [dbo].[UspPendingChartReport] --NULL,'PerWeek'        
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
 SELECT CONVERT(VARCHAR, BH.CreateDate, 1) [Date],COUNT(DISTINCT cc.ClinicalCaseId) Total  FROM WorkItem W         
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId      
 INNER JOIN BlockHistory BH on CC.ClinicalcaseId = bh.ClinicalcaseId    
 INNER JOIN BlockCategory BC on BH.BlockCategoryID = BC.BlockCategoryId    
 WHERE W.ProjectId = @ProjectId  
 AND DATEADD(Second, @TimeZoneOffset * 60 * 60, CreateDate) >= @StartDate
 AND DATEADD(Second, @TimeZoneOffset * 60 * 60, CreateDate) <= @EndDate
 GROUP BY CONVERT(VARCHAR, BH.CreateDate, 1)        
END        
ELSE IF @RangeType = 'PerWeek'        
BEGIN        
 SELECT          
  CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, CreateDate) - 6, CreateDate)), 101) AS [Week Start Date],
 CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, CreateDate), CreateDate)), 101) AS [Week End Date],
 FORMAT(CAST(CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, CreateDate) - 6, CreateDate)), 101) AS DATETIME),'dd MMM')
 +'-'+FORMAT(CAST(CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, CreateDate), CreateDate)), 101) AS DATETIME),'dd MMM yyy') AS [Start Date - End Date],
 COUNT(DISTINCT cc.ClinicalCaseId) Total  
 FROM WorkItem W         
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId      
 INNER JOIN BlockHistory BH on CC.ClinicalcaseId = bh.ClinicalcaseId    
 INNER JOIN BlockCategory BC on BH.BlockCategoryID = BC.BlockCategoryId    
 WHERE W.ProjectId = @ProjectId  
 AND DATEADD(Second, @TimeZoneOffset * 60 * 60, CreateDate) >= @StartDate 
 AND DATEADD(Second, @TimeZoneOffset * 60 * 60, CreateDate) <= @EndDate
 GROUP BY 
 CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, CreateDate) - 6, CreateDate)), 101),
 CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, CreateDate), CreateDate)), 101),
 FORMAT(CAST(CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, CreateDate) - 6, CreateDate)), 101) AS DATETIME),'dd MMM')
 +'-'+FORMAT(CAST(CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, CreateDate), CreateDate)), 101) AS DATETIME),'dd MMM yyy')
 order by CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, CreateDate) - 6, CreateDate)), 101)
END        
ELSE IF @RangeType = 'PerMonth'        
BEGIN       
      
;with cte as(       
 SELECT          
 DATENAME(month, DATEADD(Second, @TimeZoneOffset * 60 * 60, BH.CreateDate)) AS [Month],        
 DATEPART(mm, DATEADD(Second, @TimeZoneOffset * 60 * 60, BH.CreateDate)) AS [Monthnumber],        
 DATEPART(yyyy, DATEADD(Second, @TimeZoneOffset * 60 * 60, BH.CreateDate)) AS [Year],  
 COUNT(DISTINCT cc.ClinicalCaseId) Total  
 FROM WorkItem W         
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId      
 INNER JOIN BlockHistory BH on CC.ClinicalcaseId = bh.ClinicalcaseId    
 INNER JOIN BlockCategory BC on BH.BlockCategoryID = BC.BlockCategoryId    
 WHERE W.ProjectId = @ProjectId
 AND DATEADD(Second, @TimeZoneOffset * 60 * 60, CreateDate) >= @StartDate
 AND DATEADD(Second, @TimeZoneOffset * 60 * 60, CreateDate) <= @EndDate 
 GROUP BY DATENAME(month, DATEADD(Second, @TimeZoneOffset * 60 * 60, BH.CreateDate)),        
    DATEPART(yyyy , DATEADD(Second, @TimeZoneOffset * 60 * 60, BH.CreateDate)),        
    DATEPART(mm, DATEADD(Second, @TimeZoneOffset * 60 * 60, BH.CreateDate))        
)      
 select [Month],[Year],Total from cte  ORDER BY Monthnumber        
END        
        
END        
GO