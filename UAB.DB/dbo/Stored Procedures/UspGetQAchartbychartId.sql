
CREATE PROCEDURE [dbo].[UspGetQAchartbychartId]      
 --1,'QA','Block',1 ,812                         
 @ProjectID INT      
 ,@Role VARCHAR(50)      
 ,@ChartType VARCHAR(50)      
 ,@UserId INT    
 ,@ClinicalCaseId INT     
AS      
BEGIN 
 IF @Role = 'QA'      
   AND @ChartType = 'Block'      
  BEGIN      
 ;WITH ClaimCTE AS         
 (        
 SELECT DISTINCT wp.WorkItemProviderId, wp.ClinicalCaseId, wp.VersionId, wp.ProviderId, wp.PayorId,  wp.ProviderFeedbackId, wp.ClaimId  , DENSE_RANK() OVER(PARTITION BY wp.ClinicalCaseId ORDER BY ISNULL(wp.ClaimId, 0) ) AS ClaimOrder  FROM     
 WorkItemProvider wp  LEFT JOIN Claim c  ON wp.ClinicalCaseId = c.ClinicalCaseId  AND wp.VersionId = c.VersionId  WHERE wp.ClinicalCaseId = @ClinicalCaseId        
 ), VersionCTE      
   AS (      
    SELECT v.ClinicalCaseId      
     ,v.VersionId      
     ,v.VersionDate      
     ,ROW_NUMBER() OVER (      
      PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC      
      ) AS rn      
    FROM [Version] v      
    JOIN WorkItem wi ON v.ClinicalCaseId = wi.ClinicalCaseId      
    JOIN ProjectUser pu ON pu.ProjectId = wi.ProjectId      
    WHERE wi.ProjectId = @ProjectID      
     AND pu.RoleId = 2      
     AND wi.StatusId IN (      
      4      
      ,5      
      )      
     AND v.StatusId = 3      
    )      
   SELECT DISTINCT     
     dbo.fn_GetUserName(AssignedTo) AS CodedBy      
    ,dbo.fn_GetUserName(QABy) AS QABy      
    ,dbo.fn_GetUserName(ShadowQABy) AS ShadowQABy      
    ,cc.ClinicalCaseID      
    ,PatientMRN      
    ,PatientFirstName + ' ' + PatientLastName AS [Name]      
    ,CAST(DateOfService AS VARCHAR(10)) AS DateOfService      
    ,isnull(CC.ProviderId, 0) ProviderId      
    ,c.PayorId      
    ,c.ProviderFeedbackId      
    ,W.NoteTitle      
    ,dx.DxCode      
    ,cp.CPTCode      
    ,'' AS Modifier --Need to remove later         
    ,bc.Name AS BlockCategory      
    ,bh.Remarks AS BlockRemarks      
    ,bh.CreateDate AS BlockedDate  
  ,p.Name AS ProviderText      
     ,pa.Name AS PayorText      
     ,pf.Feedback AS ProviderFeedbackText  
    ,l.[Name] AS ListName        
 ,c.ClaimId        
 ,c.ClaimOrder        
   --INTO #QABlocked          
   FROM ClaimCTE c        
   JOIN ClinicalCase cc ON c.ClinicalCaseId =CC.ClinicalCaseId    
   INNER JOIN WorkItem W ON W.ClinicalCaseId = cc.ClinicalCaseId      
   LEFT JOIN List l ON l.ListId = CC.ListId      
   LEFT JOIN BlockHistory bh ON bh.ClinicalCaseId = CC.ClinicalCaseId      
   LEFT JOIN BlockCategory bc ON bc.BlockCategoryId = bh.BlockCategoryId  
   LEFT JOIN Provider p ON p.ProviderId = c.ProviderId      
   LEFT JOIN Payor pa ON pa.PayorId = c.PayorId      
   LEFT JOIN ProviderFeedback pf ON pf.ProviderFeedbackId = c.ProviderFeedbackId  
   INNER JOIN VersionCTE v ON v.ClinicalCaseId = cc.ClinicalCaseId      
   LEFT JOIN (                                                
  SELECT DISTINCT dx.ClinicalCaseId, dx.VersionId, ISNULL(dx.ClaimId, 0) ClaimId,         
  SUBSTRING(                                                
   (                                                
    SELECT ',' + dxc.DxCode  AS [text()]                                                
    FROM dbo.DxCode dxc                                                
    WHERE dx.ClinicalCaseId = dxc.ClinicalCaseId AND dx.VersionID = dxc.VersionID AND ISNULL(dx.ClaimId, 0) = ISNULL(dxc.ClaimId, 0)        
  ORDER BY dxc.ClinicalCaseId                                        
    FOR XML PATH ('')                                      
   ), 2, 1000) [DxCode]                                                
  FROM dbo.DxCode dx WHERE dx.ClinicalCaseId = @ClinicalCaseId        
 ) dx                                                
 ON c.ClinicalCaseId = dx.ClinicalCaseId AND dx.VersionId = c.VersionId AND dx.ClaimId = ISNULL(c.ClaimId, 0)                            
 LEFT JOIN (                                                
  SELECT DISTINCT cp.ClinicalCaseId, cp.VersionId,  ISNULL(cp.ClaimId, 0) ClaimId,        
  SUBSTRING(                                                
   (                                                
    SELECT '|' + cptc.cptCode + '^'+isnull(cptc.Modifier,'') + '^'+ISNULL(cptc.qty,'')+'^'+isnull(cptc.Links,'')   AS [text()]                         
    FROM dbo.cptcode cptc                                                
    WHERE cp.ClinicalCaseId = cptc.ClinicalCaseId AND cp.VersionID = cptc.VersionID AND ISNULL(cp.ClaimId, 0) = ISNULL(cptc.ClaimId, 0)                                                
    ORDER BY cptc.ClinicalCaseId                                                
    FOR XML PATH ('')                                    
   ), 2, 1000) [CptCode]                                                
  FROM dbo.cptcode cp WHERE cp.ClinicalCaseId = @ClinicalCaseId        
 ) cp                                              ON W.ClinicalCaseId = cp.ClinicalCaseId AND cp.VersionId = c.VersionId AND cp.ClaimId = ISNULL(c.ClaimId, 0)     
   WHERE c.ClinicalCaseId = @ClinicalCaseId AND bh.BlockedInQueueKind = 'QA'
   ORDER BY c.ClaimOrder      
   end    
   end     