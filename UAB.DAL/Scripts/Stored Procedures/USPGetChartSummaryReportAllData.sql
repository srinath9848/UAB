CREATE PROCEDURE [dbo].[USPGetChartSummaryReportAllData] --1,'2020-02-16','2021-02-16','ServiceDate'  
@ProjectId INT = NULL,     
@StartDate DateTIME,            
@EndDate DateTIME,         
@DateType VARCHAR(50)       
AS                                
BEGIN          
  
        
;WITH searchcte as(                             
 SELECT DISTINCT      
 CC.ClinicalCaseID,  
  cc.PatientFirstName + ' ' + ISNULL(cc.PatientLastName,'') as PatientName,
  pro.Name AS Provider,
  bpro.Name AS [Billing Provider],
  PatientMRN,          
  CAST(DateOfService AS VARCHAR(20)) AS DateOfService,     
  CC.CreatedDate AS DateUploaded,                  
  p.[Name] as ProjectName,  
  s.[Name] as [Status],  
  wi.IsBlocked,  
  wi.AssignedTo,  
  wi.QABy,  
  wi.ShadowQABy,        
  p.ProjectId   
  ,PF.Feedback  
 ,ISNULL(pr.Name, ISNULL(pro.Name ,procc.Name)) as [PostedBy]       
 ,wi.PostingDate  
 ,DENSE_RANK() OVER(PARTITION BY cc.ClinicalCaseId ORDER BY wip.VersionId DESC, wip.WorkItemProviderID ASC ) AS providerorder      
 ,'' AS DX
 ,'' AS CPT
 ,py.Name as Insurance
 from ClinicalCase cc           
 JOIN WorkItem wi on cc.ClinicalCaseId=wi.ClinicalCaseId          
 LEFT JOIN Project p on p.ProjectId=cc.ProjectId           
 LEFT JOIN [Status] s on s.StatusId=wi.StatusId      
 LEFT JOIN WorkItemProvider wip on wip.ClinicalCaseId=cc.ClinicalCaseId     
 LEFT JOIN ProviderFeedback PF ON ISNULL(wip.ProviderFeedbackId,0) = PF.ProviderFeedbackId       
 LEFT JOIN [Provider] pro on pro.ProviderId=wip.ProviderId 
 LEFT JOIN [Provider] bpro on bpro.ProviderId=wip.BillingProviderId
 LEFT JOIN [Provider] procc on procc.ProviderId = cc.ProviderId   
 LEFT JOIN ProviderPosted pp ON pp.ClinicalCaseId = cc.ClinicalCaseId      
 LEFT JOIN [Provider] pr ON pr.ProviderId = pp.ProviderId    
 left join Payor py on py.PayorId = wip.PayorId  
 WHERE p.ProjectId = @ProjectId   
 AND @StartDate <= case when @DateType = 'DateOfService' then DateOfService 
						else DATEADD(dd, 0, DATEDIFF(dd, 0, cc.CreatedDate))
						end
 AND @EndDate >= case when @DateType = 'DateOfService' then DateOfService 
						else DATEADD(dd, 0, DATEDIFF(dd, 0, cc.CreatedDate))
						end
 
 )               
SELECT * INTO #Result FROM searchcte          
WHERE providerorder=1       
    
select *, dbo.fn_GetUserName(AssignedTo) AS CodedByName            
  ,dbo.fn_GetUserName(QABy) AS QAByName                                      
    ,dbo.fn_GetUserName(ShadowQABy) AS ShadowQAByName from #Result
	  Order BY ClinicalCaseId  

---Below code is to get Dx,CPT Info    
                             
;WITH ClaimCTE AS                                         
 (                                        
 SELECT DISTINCT wp.ClinicalCaseId                                      
 ,wp.VersionId                                          
 ,wp.ClaimId                                      
 ,DENSE_RANK() OVER (                                      
 PARTITION BY wp.ClinicalCaseId ORDER BY ISNULL(wp.ClaimId, 0)                                      
 ) AS ClaimOrder                   
 , DENSE_RANK() OVER(PARTITION BY wp.ClinicalCaseId ORDER BY wp.VersionId DESC) as rn                  
 FROM WorkItemProvider wp            
 LEFT JOIN Claim c ON c.ClinicalCaseId = wp.ClinicalCaseId            
 AND c.VersionId = wp.VersionId            
 AND ISNULL(c.ClaimId, 0) = ISNULL(wp.ClaimId, 0)           
 WHERE wp.ClinicalCaseId IN (select clinicalcaseid from #Result)     
 )                                      
 ,   CoderVersionCTE                                      
   AS (      
   SELECT * FROM (    
    SELECT --TOP 1     
 v.ClinicalCaseId                                      
     ,v.VersionId                                      
     ,v.VersionDate                                      
     ,v.StatusId                                      
     ,ROW_NUMBER() OVER (                                      
      PARTITION BY v.ClinicalCaseId ORDER BY v.VersionId DESC                                      
      ) AS rn                                      
    FROM [Version] v                                      
    JOIN WorkItem wi ON v.ClinicalCaseId = wi.ClinicalCaseId          
    JOIN DxCode dx ON dx.ClinicalCaseId = wi.ClinicalCaseId AND dx.VersionId = v.VersionId         
 WHERE wi.ClinicalCaseId IN (select clinicalcaseid from #Result)      
     AND v.StatusId IN (3, 6, 10, 12)            
  ) AS x WHERE rn = 1    
    )                                      
   SELECT DISTINCT CC.ClinicalCaseID                                        
    ,cp.CPTCode                                      
    ,dx.DxCode                                      
    ,c.ClaimOrder                                      
   FROM ClaimCTE c                                      
   INNER JOIN ClinicalCase cc ON cc.ClinicalCaseId = c.ClinicalCaseId                                      
   INNER JOIN WorkItem W ON W.ClinicalCaseId = CC.ClinicalCaseId            
   INNER JOIN [User] u ON u.UserId = W.AssignedTo    
   INNER JOIN CoderVersionCTE cv ON cv.ClinicalCaseID = cc.ClinicalCaseID                  
   LEFT JOIN (                                      
    SELECT DISTINCT cp.ClinicalCaseId    
     ,cp.VersionId, cp.ClaimId                                      
     ,SUBSTRING((                                      
       SELECT '|' + cptc.cptCode + '-' + isnull(cptc.Modifier, '') + '-' + ISNULL(cptc.qty, '') + '-' + isnull(cptc.Links, '') AS [text()]                                   
       FROM dbo.cptcode cptc                                      
       WHERE cp.ClinicalCaseId = cptc.ClinicalCaseId                                   
        AND cp.VersionID = cptc.VersionID AND ISNULL(cp.ClaimId, 0) = ISNULL(cptc.ClaimId, 0)                                      
       ORDER BY cptc.ClinicalCaseId                                      
       FOR XML PATH('')                                      
       ), 2, 1000) [CptCode]                                      
    FROM dbo.cptcode cp                                      
    ) cp ON cp.ClinicalCaseId = c.ClinicalCaseId                                      
    AND cp.VersionID = cv.VersionID AND ISNULL(cp.ClaimId, 0) = ISNULL(c.ClaimId, 0)                                      
    LEFT JOIN (                                      
    SELECT DISTINCT dx.ClinicalCaseId                                      
     ,dx.VersionId, dx.ClaimId                                      
     ,SUBSTRING((                                      
       SELECT ',' + dxc.DxCode AS [text()]                                      
       FROM dbo.DxCode dxc                                  
       WHERE dx.ClinicalCaseId = dxc.ClinicalCaseId                                      
        AND dxc.VersionId = dx.VersionId AND ISNULL(dx.ClaimId, 0) = ISNULL(dxc.ClaimId, 0)                                      
       ORDER BY dxc.ClinicalCaseId                                      
       FOR XML PATH('')                                      
       ), 2, 1000) [DxCode]                                      
    FROM dbo.DxCode dx                                      
    ) dx ON dx.ClinicalCaseId = c.ClinicalCaseId                                      
  AND dx.VersionID = cv.VersionID AND ISNULL(dx.ClaimId, 0) = ISNULL(c.ClaimId, 0)       
 WHERE W.ClinicalCaseId IN (select clinicalcaseid from #Result)    
 AND W.Statusid in(15,16,17)        
   ORDER BY CC.ClinicalCaseId, c.ClaimOrder    
    
END    
GO


