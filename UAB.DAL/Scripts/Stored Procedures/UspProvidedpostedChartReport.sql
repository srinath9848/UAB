CREATE PROCEDURE [dbo].[UspProvidedpostedChartReport] --NULL,'PerDay'        
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
 SELECT CONVERT(VARCHAR, PostingDate, 1) [Date],COUNT(*) Total  FROM WorkItem W         
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId   
 INNER JOIN ProviderPosted PP ON PP.ClinicalCaseId=CC.ClinicalCaseId  
 WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))      
 AND W.StatusId =17  
 AND (PostingDate >= @StartDate OR (PostingDate = ISNULL(@StartDate,PostingDate)))  
 AND (PostingDate <= @EndDate OR (PostingDate = ISNULL(@EndDate,PostingDate)))  
 GROUP BY CONVERT(VARCHAR, PostingDate, 1)        
END        
ELSE IF @RangeType = 'PerWeek'        
BEGIN        
 SELECT 
 CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, PostingDate) - 6, PostingDate)), 101) AS [Week Start Date],
 CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, PostingDate), PostingDate)), 101) AS [Week End Date],
 FORMAT(CAST(CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, PostingDate) - 6, PostingDate)), 101) AS DATETIME),'dd MMM')
 +'-'+FORMAT(CAST(CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, PostingDate), PostingDate)), 101) AS DATETIME),'dd MMM yyy') AS [Start Date - End Date],
 COUNT(*) Total  
 FROM WorkItem W         
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId  
 INNER JOIN ProviderPosted PP ON PP.ClinicalCaseId=CC.ClinicalCaseId  
 WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))      
 AND W.StatusId =17  
 AND (PostingDate >= @StartDate OR (PostingDate = ISNULL(@StartDate,PostingDate)))  
 AND (PostingDate <= @EndDate OR (PostingDate = ISNULL(@EndDate,PostingDate)))  
 GROUP BY 
 CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, PostingDate) - 6, PostingDate)), 101),
	CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, PostingDate), PostingDate)), 101),
	FORMAT(CAST(CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, PostingDate) - 6, PostingDate)), 101) AS DATETIME),'dd MMM')
 +'-'+FORMAT(CAST(CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, PostingDate), PostingDate)), 101) AS DATETIME),'dd MMM yyy')
 order by CONVERT(varchar(50), (DATEADD(dd, @@DATEFIRST - DATEPART(dw, PostingDate) - 6, PostingDate)), 101)
END        
ELSE IF @RangeType = 'PerMonth'        
BEGIN       
      
;with cte as(       
 SELECT          
 DATENAME(month, PostingDate) AS [Month],        
 DATEPART(mm, PostingDate) AS [Monthnumber],        
 DATEPART(yyyy, PostingDate) AS [Year],  
 COUNT(*) Total  
 FROM WorkItem W         
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId  
 INNER JOIN ProviderPosted PP ON PP.ClinicalCaseId=CC.ClinicalCaseId  
 WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))      
 AND W.StatusId =17  
 AND (PostingDate >= @StartDate OR (PostingDate = ISNULL(@StartDate,PostingDate)))  
 AND (PostingDate <= @EndDate OR (PostingDate = ISNULL(@EndDate,PostingDate)))  
 GROUP BY DATENAME(month, PostingDate),        
    DATEPART(yyyy , PostingDate),        
    DATEPART(mm, PostingDate)        
)      
 select [Month],[Year],Total from cte  ORDER BY Monthnumber        
END        
        
END
GO