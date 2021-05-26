    
CREATE PROCEDURE [dbo].[UspProvidedpostedChartReport] --NULL,'PerDay'      
@ProjectId INT = NULL,      
@RangeType VARCHAR(100)      
AS      
BEGIN      
    
IF @ProjectId = 0     
 SET @ProjectId = NULL      
      
IF @RangeType = 'PerDay'      
BEGIN      
 SELECT COUNT(*) Total,CONVERT(VARCHAR, PostingDate, 1) [Date]  FROM WorkItem W       
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId 
 INNER JOIN ProviderPosted PP ON PP.ClinicalCaseId=CC.ClinicalCaseId
 WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))    
 AND W.StatusId =17    
 GROUP BY CONVERT(VARCHAR, PostingDate, 1)      
END      
ELSE IF @RangeType = 'PerWeek'      
BEGIN      
 SELECT COUNT(*) Total,    
   DATEPART(ISO_WEEK, PostingDate) [Week],   
 --DATEPART(WEEK, CodedDate)-DATEPART(WEEK, DATEADD(MM, DATEDIFF(MM,0,CodedDate), 0))+ 1 AS [Week],      
 DATENAME(month, PostingDate) AS [Month],      
 DATEPART(yyyy, PostingDate) AS [Year]      
 FROM WorkItem W       
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId
 INNER JOIN ProviderPosted PP ON PP.ClinicalCaseId=CC.ClinicalCaseId
 WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))    
 AND W.StatusId =17  
 GROUP BY DATEPART(ISO_WEEK, PostingDate),      
    DATENAME(month, PostingDate),      
    DATEPART(yyyy , PostingDate)--,DATEPART(WEEK, DATEADD(MM, DATEDIFF(MM,0,CodedDate), 0))      
END      
ELSE IF @RangeType = 'PerMonth'      
BEGIN     
    
;with cte as(     
 SELECT COUNT(*) Total,       
 DATENAME(month, PostingDate) AS [Month],      
 DATEPART(mm, PostingDate) AS [Monthnumber],      
 DATEPART(yyyy, PostingDate) AS [Year]      
 FROM WorkItem W       
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId
 INNER JOIN ProviderPosted PP ON PP.ClinicalCaseId=CC.ClinicalCaseId
 WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))    
 AND W.StatusId =17  
 GROUP BY DATENAME(month, PostingDate),      
    DATEPART(yyyy , PostingDate),      
    DATEPART(mm, PostingDate)      
)    
 select Total,[Month],[Year] from cte  ORDER BY Monthnumber      
END      
      
END      
GO