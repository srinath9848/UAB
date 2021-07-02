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
   DATEPART(ISO_WEEK, PostingDate) [Week],     
 --DATEPART(WEEK, CodedDate)-DATEPART(WEEK, DATEADD(MM, DATEDIFF(MM,0,CodedDate), 0))+ 1 AS [Week],        
 DATENAME(month, PostingDate) AS [Month],        
 DATEPART(yyyy, PostingDate) AS [Year],  
 COUNT(*) Total  
 FROM WorkItem W         
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId  
 INNER JOIN ProviderPosted PP ON PP.ClinicalCaseId=CC.ClinicalCaseId  
 WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))      
 AND W.StatusId =17  
 AND (PostingDate >= @StartDate OR (PostingDate = ISNULL(@StartDate,PostingDate)))  
 AND (PostingDate <= @EndDate OR (PostingDate = ISNULL(@EndDate,PostingDate)))  
 GROUP BY DATEPART(ISO_WEEK, PostingDate),        
    DATENAME(month, PostingDate),        
    DATEPART(yyyy , PostingDate)--,DATEPART(WEEK, DATEADD(MM, DATEDIFF(MM,0,CodedDate), 0))        
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