alter procedure UspGetChartCount 
AS
BEGIN

SELECT P.ProjectId,P.Name,W.StatusId,COUNT(W.StatusId) Cnt FROM Project P LEFT JOIN ClinicalCase CC ON P.ProjectId = CC.ProjectId
LEFT JOIN WorkItem W ON CC.ClinicalCaseId = W.ClinicalCaseId
GROUP BY P.ProjectId,P.Name,W.StatusId
ORDER BY P.ProjectId

END