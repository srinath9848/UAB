alter procedure UspGetChartCountByStatus --'1,9,10,8'
@StatusIds VARCHAR(100) 
AS  
BEGIN  
  
SELECT P.ProjectId,P.Name,W.StatusId,COUNT(W.StatusId) Cnt FROM Project P LEFT JOIN ClinicalCase CC ON P.ProjectId = CC.ProjectId  
LEFT JOIN WorkItem W ON CC.ClinicalCaseId = W.ClinicalCaseId AND W.StatusId IN (SELECT VALUE FROM dbo.fnSplit(NULLIF(@StatusIds, ''), ',')  )
GROUP BY P.ProjectId,P.Name,W.StatusId  
ORDER BY P.ProjectId  
  
END  
  
  
  
  
  
  