CREATE PROCEDURE [dbo].[USPGetNext] --1,'Coder','Block',1    
 @ProjectID INT                            
 ,@Role VARCHAR(50)                            
 ,@ChartType VARCHAR(50)                            
 ,@UserId INT,                  
 @PrevOrNextCCID INT = 0                          
AS                            
BEGIN                            
 /*                                                          
TODO:                                                          
1. Implement the App Lock - DONE                                                
2. If the user has an open & assigned chart, bring it first before getting it from the pool - DONE                                                
3. ALTER TABLE ClinicalCase DROP COLUMN ProjectId;                                                          
4.                                                           
*/                            
 BEGIN TRY                            
  BEGIN TRANSACTION                            
                            
  DECLARE @RC INT                            
  DECLARE @message VARCHAR(500)                            
                            
  EXEC @RC = sp_getapplock @Resource = 'USPGetNext'                            
   ,@LockMode = 'Exclusive'                            
   ,@LockOwner = 'Transaction'                            
   ,@LockTimeout = 15000 --15 seconds                                                
                            
  IF @RC < 0                            
  BEGIN                            
   SELECT @message = CONVERT(VARCHAR(30), GETUTCDATE(), 121) + ': Sorry, could not obtain a lock within the timeout period, return code was ' + CONVERT(VARCHAR(30), @RC) + '.'                            
                            
   RAISERROR (                            
     @message                            
     ,0                            
     ,1                            
     )                            
   WITH NOWAIT;                            
                            
   ROLLBACK TRANSACTION                            
                            
   RETURN @RC                            
  END                            
  ELSE                            
  BEGIN                            
   SELECT @message = CONVERT(VARCHAR(30), GETUTCDATE(), 121) + ': AppLock obtained ..'                            
                            
   RAISERROR (                            
     @message                            
     ,0                            
     ,1                            
     )                            
   WITH NOWAIT;                            
  END                            
                            
  DECLARE @WorkItemID INT                            
  DECLARE @ClinicalCaseId INT                            
                            
  IF @Role = 'Coder'                            
   AND @ChartType = 'Available'                            
  BEGIN                            
   SELECT TOP 1 CC.ClinicalCaseID                            
    ,PatientMRN                            
    ,PatientFirstName + ' ' + PatientLastName AS [Name]                            
    --   ,CAST(DateOfService AS VARCHAR(20)) AS DateOfService                                              
    --,isnull(CC.ProviderId, 0)  ProviderId                                                                   
    ,DateOfService                            
    ,isnull(CC.ProviderId, 0)  as ProviderId                         
    ,l.[Name] AS ListName                            
   INTO #CoderAvailable                            
   FROM ClinicalCase CC                            
   INNER JOIN WorkItem W ON W.ClinicalCaseId = CC.ClinicalCaseId                            
   INNER JOIN ProjectUser pu ON pu.ProjectId = w.ProjectId                            
   LEFT JOIN List l ON l.ListId = CC.ListId                            
   WHERE CC.ProjectId = @ProjectID                            
    AND (                            
     (   
      W.IsPriority = 1                            
      AND W.StatusID IN (                            
       1                            
       ,2                 
       )                            
      AND (                            
       W.AssignedTo = @UserId                           
       OR W.AssignedTo IS NULL                            
       )                            
      )                            
     OR (                            
      W.StatusId = 2                            
      AND w.AssignedTo = @UserId                            
      AND ISNULL(W.IsBlocked, 0) = 0                            
      )            
     OR W.StatusId = 1                            
     )                            
    AND pu.UserId = @UserId                            
    AND pu.RoleId = 1                            
    AND pu.IsActive = 1                       
   ORDER BY CASE                             
     WHEN W.IsPriority = 1                            
      THEN 1                            
     WHEN (                            
       W.StatusId = 2                            
       AND w.AssignedTo = @UserId                      
       AND ISNULL(W.IsBlocked, 0) = 0                            
       )                            
      THEN 2                            
     WHEN W.StatusId = 1                            
      THEN 3                            
     END ASC                            
    ,CC.DateOfService ASC                            
                            
   UPDATE WorkItem                            
   SET StatusId = 2                            
    ,AssignedTo = @UserId                            
    ,CodedDate = GETUTCDATE()                            
   WHERE ClinicalCaseId = (                          
     SELECT ClinicalCaseId                            
     FROM #CoderAvailable                            
     )                            
                            
   --IF NOT EXISTS (                            
   --  SELECT 1                            
   --  FROM Version                            
   --  WHERE UserID = @UserId                            
   --  AND StatusId = 2                            
   --  )                            
   -- AND EXISTS (                            
   --  SELECT 1                            
   --  FROM #CoderAvailable                            
   --  )                            
   --BEGIN                            
    INSERT INTO Version (                            
     ClinicalcaseID                            
     ,VersionDate                            
     ,userID                            
     ,Statusid                            
     )                            
    SELECT (                            
      SELECT ClinicalCaseId                            
      FROM #CoderAvailable                            
      )                            
     ,GETUTCDATE()                            
     ,@UserId                            
     ,2                            
   --END                            
                            
   SELECT *                            
   FROM #CoderAvailable                            
  END                            
ELSE IF @Role = 'Coder'                            
   AND @ChartType = 'Incorrect'                            
  BEGIN                            
                            
  SELECT DISTINCT TOP 1 @ClinicalCaseId =                             
   wi.ClinicalCaseId                             
   FROM WorkItem wi                            
   JOIN ProjectUser pu                            
   ON pu.ProjectId = wi.ProjectId                            
   WHERE wi.ProjectId = @ProjectID                            
       AND wi.StatusId = 14                            
       AND wi.AssignedTo = @UserId                            
       AND pu.RoleId = 1                            
       AND pu.IsActive = 1                            
   ORDER BY wi.ClinicalCaseId                            
                        
;WITH ClaimCTE AS                               
 (                              
 --SELECT DISTINCT wa.WorkItemAuditId                            
 --    ,wa.ClinicalCaseId                            
 --    ,wa.VersionId                            
 --    ,wp.ProviderId                            
 --    ,wp.PayorId                 
 --    ,wp.ProviderFeedbackId                             
 --    ,wa.ClaimId                            
 --    ,DENSE_RANK() OVER (                            
 --     PARTITION BY wa.ClinicalCaseId ORDER BY ISNULL(wa.ClaimId, 0)                            
 --     ) AS ClaimOrder         
 --  , DENSE_RANK() OVER(PARTITION BY wa.ClinicalCaseId ORDER BY wa.VersionId DESC) as rn        
 --   FROM WorkItemAudit wa                            
 --   LEFT JOIN Claim c ON wa.ClinicalCaseId = c.ClinicalCaseId                            
 --   AND wa.VersionId = c.VersionId        
	--LEFT JOIN WorkItemProvider wp ON wp.ClinicalCaseId = wa.ClinicalCaseId 
	--AND ISNULL(wp.ClaimId, 0) = ISNULL(wa.ClaimId, 0)
	--AND wp.VersionId = wa.VersionId
	--WHERE wp.ClinicalCaseId = @ClinicalCaseId

    SELECT DISTINCT wp.ClinicalCaseId                            
     ,wp.VersionId                            
     ,wp.ProviderId                            
     ,wp.PayorId                 
     ,wp.ProviderFeedbackId                             
     ,wp.ClaimId                            
     ,DENSE_RANK() OVER (                            
      PARTITION BY wp.ClinicalCaseId ORDER BY ISNULL(wp.ClaimId, 0)                            
      ) AS ClaimOrder         
   , DENSE_RANK() OVER(PARTITION BY wp.ClinicalCaseId ORDER BY wp.VersionId DESC) as rn        
    FROM WorkItemProvider wp
	LEFT JOIN WorkItemAudit wa ON wp.ClinicalCaseId = wa.ClinicalCaseId 
	AND ISNULL(wp.ClaimId, 0) = ISNULL(wa.ClaimId, 0)
	AND wp.VersionId = wa.VersionId
	LEFT JOIN Claim c ON wa.ClinicalCaseId = c.ClinicalCaseId                            
    AND wa.VersionId = c.VersionId
	WHERE wp.ClinicalCaseId = @ClinicalCaseId
 )                            
 ,  QAVersionCTE                            
   AS (                            
    SELECT TOP 1 v.ClinicalCaseId                            
     ,v.VersionId                            
     ,v.VersionDate                            
     ,v.StatusId                            
     ,ROW_NUMBER() OVER (                            
      PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC                            
      ) AS rn                            
    FROM [Version] v                            
    JOIN WorkItem wi ON v.ClinicalCaseId = wi.ClinicalCaseId                            
    WHERE wi.ClinicalCaseId = @ClinicalCaseId                          
     AND wi.StatusId = 14                  
  --AND v.StatusId = 7                  
     AND v.StatusId IN (                            
      6, 7                            
      ,8        
   ,13    
   ,10    
      )    
   ORDER BY v.VersionId DESC                        
    )                            
    ,ShadowQAVersionCTE                            
   AS (                            
    SELECT v.ClinicalCaseId                        
     ,v.VersionId                            
     ,v.VersionDate                            
     ,v.StatusId                            
     ,ROW_NUMBER() OVER (                            
      PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC                            
      ) AS rn                            
    FROM [Version] v                            
    JOIN WorkItem wi ON v.ClinicalCaseId = wi.ClinicalCaseId                            
    WHERE wi.ClinicalCaseId = @ClinicalCaseId                            
     AND wi.StatusId = 14                            
     AND v.StatusId = 11                            
    )                            
    ,CoderVersionCTE                            
   AS (                            
    SELECT v.ClinicalCaseId                            
     ,v.VersionId                            
     ,v.VersionDate                            
     ,v.StatusId                            
     ,ROW_NUMBER() OVER (                            
      PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC                            
      ) AS rn                            
    FROM [Version] v                            
    JOIN WorkItem wi ON v.ClinicalCaseId = wi.ClinicalCaseId                            
    WHERE wi.ClinicalCaseId = @ClinicalCaseId                            
     AND wi.StatusId = 14                            
     AND v.StatusId = 3                            
    )                     
    ,AuditCTE                            
   AS (                            
    SELECT wa.[ClinicalCaseId], wa.ClaimId                            
     ,MAX(CASE                             
       WHEN FieldName = 'PayorId'                            
        THEN FieldValue            
       END) AS QAPayorId                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderID'                            
        THEN FieldValue                            
       END) AS QAProviderID                            
     ,MAX(CASE                             
       WHEN FieldName = 'CPTCode'                            
        THEN FieldValue                            
       END) AS QACPTCode                            
     ,MAX(CASE                             
       WHEN FieldName = 'Mod'                            
        THEN FieldValue                            
       END) AS QAMod                            
     ,MAX(CASE                             
       WHEN FieldName = 'Dx'                            
     THEN FieldValue                            
       END) AS QADx                            
 ,MAX(CASE                             
       WHEN FieldName = 'ProviderFeedbackID'                            
        THEN FieldValue                            
       END) AS QAProviderFeedbackID                            
     ,MAX(CASE                             
       WHEN FieldName = 'PayorId'                            
        THEN Remark                            
       END) AS QAPayorIdRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderID'                            
        THEN Remark                            
       END) AS QAProviderIDRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'CPTCode'                            
        THEN Remark                            
       END) AS QACPTCodeRemark                            
 ,MAX(CASE                             
       WHEN FieldName = 'Mod'                            
        THEN Remark                            
       END) AS QAModRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'Dx'                            
        THEN Remark                            
       END) AS QADxRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderFeedbackID'                            
        THEN Remark                            
       END) AS QAProviderFeedbackIDRemark                            
     ,wa.ErrorTypeId QAErrorTypeId                      
    FROM [dbo].[WorkItemAudit] wa                            
    JOIN QAVersionCTE qv ON qv.ClinicalCaseId = wa.ClinicalCaseId                            
     AND wa.VersionId = qv.VersionId        
  WHERE qv.rn = 1        
    GROUP BY wa.ClinicalCaseId                            
     , wa.ErrorTypeId                        
     , wa.ClaimId                        
    )                            
    ,ShadowQAAuditCTE                            
   AS (                            
    SELECT wa.[ClinicalCaseId], wa.ClaimId                            
     ,MAX(CASE                             
       WHEN FieldName = 'PayorId'                            
        THEN FieldValue                            
       END) AS ShadowQAPayorId                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderID'                            
        THEN FieldValue                         
       END) AS ShadowQAProviderID                            
     ,MAX(CASE                             
       WHEN FieldName = 'CPTCode'                            
        THEN FieldValue                            
       END) AS ShadowQACPTCode                            
     ,MAX(CASE                             
       WHEN FieldName = 'Mod'                            
        THEN FieldValue                            
       END) AS ShadowQAMod                            
     ,MAX(CASE                             
       WHEN FieldName = 'Dx'                            
        THEN FieldValue                            
       END) AS ShadowQADx                  
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderFeedbackID'                            
        THEN FieldValue                            
       END) AS ShadowQAProviderFeedbackID                            
     ,MAX(CASE                             
       WHEN FieldName = 'PayorId'                            
        THEN Remark                            
       END) AS ShadowQAPayorIdRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderID'                            
        THEN Remark                            
       END) AS ShadowQAProviderIDRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'CPTCode'                            
        THEN Remark                            
       END) AS ShadowQACPTCodeRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'Mod'                            
        THEN Remark                            
       END) AS ShadowQAModRemark                                 
 ,MAX(CASE                             
       WHEN FieldName = 'Dx'                 
        THEN Remark                            
       END) AS ShadowQADxRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderFeedbackID'                            
        THEN Remark                            
       END) AS ShadowQAProviderFeedbackIDRemark                            
     ,wa.ErrorTypeId ShadowQAErrorTypeId                            
    FROM [dbo].[WorkItemAudit] wa                            
    JOIN ShadowQAVersionCTE qv ON qv.ClinicalCaseId = wa.ClinicalCaseId                            
     AND wa.VersionId = qv.VersionId        
  WHERE qv.rn = 1        
    GROUP BY wa.ClinicalCaseId                            
     , wa.ErrorTypeId                            
     , wa.ClaimId                        
    )                            
   SELECT DISTINCT                          
    cc.ClinicalCaseID                            
    ,PatientMRN                            
    ,PatientFirstName + ' ' + PatientLastName AS [Name]                            
    ,CAST(DateOfService AS VARCHAR(10)) AS DateOfService    
    ,dbo.fn_GetUserName(AssignedTo) AS CodedBy                            
    ,dbo.fn_GetUserName(QABy) AS QABy                            
    ,dbo.fn_GetUserName(ShadowQABy) AS ShadowQABy                            
    ,c.ProviderId                            
    ,p.Name AS ProviderText                            
    ,c.PayorId                            
    ,pa.Name AS PayorText                            
    ,c.ProviderFeedbackId                            
    ,Pf.Feedback AS FeedbackText                            
    ,wi.NoteTitle                            
    ,dx.DxCode                            
    ,cp.CPTCode                            
    ,ISNULL(QAPayorId, ShadowQAPayorId) AS QAPayorId                            
    ,ISNULL(QAPa.[Name], SQAPa.[Name]) AS QAPayorText                            
    ,ISNULL(QAProviderID, ShadowQAProviderID) AS QAProviderID                            
    ,ISNULL(QAP.[Name], SQAP.[Name]) AS QAProviderText                            
    ,ISNULL(QACPTCode, ShadowQACPTCode) AS QACPTCode    
    ,ISNULL(QADx, ShadowQADx) AS QADx                            
    ,ISNULL(QAProviderFeedbackID, ShadowQAProviderFeedbackID) AS QAProviderFeedbackID                            
    ,ISNULL(QAPf.Feedback, SQAPf.Feedback) AS QAFeedbackText                            
    ,ISNULL(QAPayorIdRemark, ShadowQAPayorIdRemark) AS QAPayorIdRemark                            
    ,ISNULL(QAProviderIDRemark, ShadowQAProviderIDRemark) AS QAProviderIDRemark                            
    ,ISNULL(QACPTCodeRemark, ShadowQACPTCodeRemark) AS QACPTCodeRemark    
    ,ISNULL(QADxRemark, ShadowQADxRemark) AS QADxRemark                            
    ,ISNULL(QAProviderFeedbackIDRemark, ShadowQAProviderFeedbackIDRemark) AS QAProviderFeedbackIDRemark                                
 ,QAErrorTypeId                        
 ,ShadowQAErrorTypeId                          
    ,c.ClaimId                            
    ,c.ClaimOrder                            
   ,l.[Name] AS ListName                            
   FROM ClaimCTE c                            
   INNER JOIN ClinicalCase cc ON cc.ClinicalCaseId = c.ClinicalCaseId                            
   INNER JOIN WorkItem wi ON wi.ClinicalCaseId = cc.ClinicalCaseId    
   LEFT JOIN [Provider] P ON c.ProviderId = P.ProviderID                            
   LEFT JOIN ProviderFeedback Pf ON c.ProviderFeedbackId = Pf.ProviderFeedbackId                            
   LEFT JOIN Payor Pa ON c.PayorId = pa.PayorId    
   LEFT JOIN CoderVersionCTE cv ON cv.ClinicalCaseId = cc.ClinicalCaseId    
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
   LEFT JOIN (                   
    SELECT DISTINCT cp.ClinicalCaseId                            
     ,cp.VersionId, cp.ClaimId                            
     ,SUBSTRING((                            
       SELECT '|' + cptc.cptCode + '^' + isnull(cptc.Modifier, '') + '^' + ISNULL(cptc.qty, '') + '^' + isnull(cptc.Links, '') AS [text()]                            
       FROM dbo.cptcode cptc                            
       WHERE cp.ClinicalCaseId = cptc.ClinicalCaseId                            
        AND cp.VersionID = cptc.VersionID AND ISNULL(cp.ClaimId, 0) = ISNULL(cptc.ClaimId, 0)                            
       ORDER BY cptc.ClinicalCaseId                            
       FOR XML PATH('')                            
       ), 2, 1000) [CptCode]                            
    FROM dbo.cptcode cp                            
    ) cp ON cc.ClinicalCaseId = cp.ClinicalCaseId                            
    AND cp.VersionId = cv.VersionId AND ISNULL(cp.ClaimId, 0) = ISNULL(c.ClaimId, 0)                            
   LEFT JOIN AuditCTE a ON a.ClinicalCaseId = cc.ClinicalCaseId AND ISNULL(a.ClaimId, 0) = ISNULL(c.ClaimId, 0)                           
   LEFT JOIN [Provider] QAP ON a.QAProviderID = QAP.ProviderID                            
   LEFT JOIN ProviderFeedback QAPf ON a.QAProviderFeedbackID = QAPf.ProviderFeedbackId                            
   LEFT JOIN Payor QAPa ON QAPa.PayorId = a.QAPayorId                            
   LEFT JOIN ShadowQAAuditCTE sa ON sa.ClinicalCaseId = cc.ClinicalCaseId AND ISNULL(sa.ClaimId, 0) = ISNULL(c.ClaimId, 0)                            
   LEFT JOIN [Provider] SQAP ON sa.ShadowQAProviderID = SQAP.ProviderID                            
   LEFT JOIN ProviderFeedback SQAPf ON sa.ShadowQAProviderFeedbackID = SQAPf.ProviderFeedbackId                            
   LEFT JOIN Payor SQAPa ON SQAPa.PayorId = sa.ShadowQAPayorId                            
   LEFT JOIN List l ON l.ListId = CC.ListId                            
   WHERE wi.ClinicalCaseId = @ClinicalCaseId AND c.rn = 1                           
   ORDER BY c.ClaimOrder                           
                            
  END                            
  ELSE IF @Role = 'Coder'                       
   AND @ChartType = 'ReadyForPosting'                        
  BEGIN                            
    SELECT DISTINCT TOP 1 @ClinicalCaseId =                             
 wi.ClinicalCaseId                             
 FROM WorkItem wi                            
 JOIN ProjectUser pu                            
 ON pu.ProjectId = wi.ProjectId                            
 WHERE wi.ProjectId = @ProjectID                            
    AND wi.StatusId = 15                            
    AND wi.AssignedTo = @UserId                            
    AND pu.UserId = @UserId                            
    AND pu.RoleId = 1                            
    AND pu.IsActive = 1                            
 ORDER BY wi.ClinicalCaseId                             
                            
;WITH ClaimCTE AS                               
 (                              
 SELECT DISTINCT wp.ClinicalCaseId                            
 ,wp.VersionId                            
 ,wp.ProviderId                            
 ,wp.PayorId                            
 ,wp.ProviderFeedbackId                             
 ,wp.ClaimId                              
 , p.[Name] AS ProviderText                            
 , pa.[Name] AS PayorText                   
 , pf.Feedback AS ProviderFeedbackText                            
 ,DENSE_RANK() OVER (                            
 PARTITION BY wp.ClinicalCaseId ORDER BY ISNULL(wp.ClaimId, 0)                            
 ) AS ClaimOrder         
 , DENSE_RANK() OVER(PARTITION BY wp.ClinicalCaseId ORDER BY wp.VersionId DESC) as rn        
 FROM WorkItemProvider wp  
 LEFT JOIN Claim c ON c.ClinicalCaseId = wp.ClinicalCaseId  
 AND c.VersionId = wp.VersionId  
 AND ISNULL(c.ClaimId, 0) = ISNULL(wp.ClaimId, 0)  
 LEFT JOIN Provider p ON p.ProviderId = wp.ProviderId                            
 LEFT JOIN Payor pa ON pa.PayorId = wp.PayorId                          
 LEFT JOIN ProviderFeedback pf ON pf.ProviderFeedbackId = ISNULL(wp.ProviderFeedbackId, 0)  
 --LEFT JOIN WorkItemAudit wa ON wa.ClinicalCaseId = c.ClinicalCaseId                            
 --AND wa.VersionId = c.VersionId          
 --LEFT JOIN (          
 -- SELECT MAX(VersionId) AS VersionId FROM [Version] WHERE ClinicalCaseId = @ClinicalCaseId    
 -- AND StatusID = 3  
 --) AS v ON v.VersionId = wa.VersionId  
 WHERE wp.ClinicalCaseId = @ClinicalCaseId  
 )                            
 ,   CoderVersionCTE                            
   AS (                            
    SELECT TOP 1 v.ClinicalCaseId                            
     ,v.VersionId                            
     ,v.VersionDate                            
     ,v.StatusId                            
     ,ROW_NUMBER() OVER (                            
      PARTITION BY v.ClinicalCaseId ORDER BY v.VersionId DESC                            
      ) AS rn                            
    FROM [Version] v                            
    JOIN WorkItem wi ON v.ClinicalCaseId = wi.ClinicalCaseId
	JOIN DxCode dx ON dx.ClinicalCaseId = wi.ClinicalCaseId AND dx.VersionId = v.VersionId
    WHERE wi.ClinicalCaseId = @ClinicalCaseId                        
     AND v.StatusId IN (3, 6, 10, 12)  
  ORDER BY v.VersionId DESC                            
    )                            
   SELECT DISTINCT CC.ClinicalCaseID                            
    ,PatientMRN                            
    ,PatientFirstName + ' ' + PatientLastName AS [Name]                            
    ,CAST(DateOfService AS VARCHAR(20)) AS DateOfService                            
    ,isnull(CC.ProviderId, 0)                            
    ,cp.CPTCode                            
    ,dx.DxCode                            
    ,c.ProviderId                            
    ,c.PayorId                            
    ,c.ProviderFeedbackId                            
    ,w.NoteTitle                            
    ,ProviderText                            
    ,PayorText                            
    ,ProviderFeedbackText                            
    ,dbo.fn_GetUserName(AssignedTo) AS CodedBy                            
  ,dbo.fn_GetUserName(QABy) AS QABy                            
    ,dbo.fn_GetUserName(ShadowQABy) AS ShadowQABy                            
    ,l.[Name] AS ListName                            
    ,c.ClaimId                            
    ,c.ClaimOrder                            
   FROM ClaimCTE c                            
   INNER JOIN ClinicalCase cc ON cc.ClinicalCaseId = c.ClinicalCaseId                            
   INNER JOIN WorkItem W ON W.ClinicalCaseId = CC.ClinicalCaseId                            
   INNER JOIN CoderVersionCTE cv ON cv.ClinicalCaseID = cc.ClinicalCaseID        
   LEFT JOIN (                            
    SELECT DISTINCT cp.ClinicalCaseId                            
     ,cp.VersionId, cp.ClaimId                            
     ,SUBSTRING((                            
       SELECT '|' + cptc.cptCode + '^' + isnull(cptc.Modifier, '') + '^' + ISNULL(cptc.qty, '') + '^' + isnull(cptc.Links, '') AS [text()]                            
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
   LEFT JOIN List l ON l.ListId = CC.ListId                          
   WHERE W.ClinicalCaseId = @ClinicalCaseId AND c.rn = 1  
   ORDER BY c.ClaimOrder                            
                             
  END                            
  ELSE IF @Role = 'Coder'                            
   AND @ChartType = 'Block'                            
  BEGIN              
              
   DECLARE @CCIDs VARCHAR(max)                  
  DECLARE @CurrCCId INT               
            
   SELECT @CCIDs = COALESCE(@CCIDs + ',', '') + cast(w.Clinicalcaseid as varchar(10))                                       
   FROM ClinicalCase CC                                  
   INNER JOIN WorkItem W ON W.ClinicalCaseId = CC.ClinicalCaseId                                 
   INNER JOIN ProjectUser pu ON pu.ProjectId = w.ProjectId                                  
   INNER JOIN BlockHistory Bh ON Bh.ClinicalCaseId = CC.ClinicalcaseId                             
   WHERE CC.ProjectId = @ProjectID                                  
    AND (                             
     (                                  
      w.AssignedTo = @UserId                                  
      AND ISNULL(W.IsBlocked, 0) = 1                                  
      )                                  
     )                                  
    AND pu.UserId = @UserId                                  
    AND pu.RoleId = 1                                  
    AND pu.IsActive = 1                          
 AND W.statusid=2                               
 AND bh.BlockedInQueueKind='coding'                                 
   ORDER BY CC.ClinicalCaseID               
            
            
 IF @PrevOrNextCCID  = 0                  
 SELECT TOP 1  @CurrCCId = value FROM dbo.fnSplit(@CCIDs, ',')                   
ELSE                  
 SELECT @CurrCCId = @PrevOrNextCCID               
                           
   SELECT TOP 1 CC.ClinicalCaseID                            
    ,PatientMRN                            
    ,PatientFirstName + ' ' + PatientLastName AS [Name]                            
    --,CAST(DateOfService AS VARCHAR(20)) AS DateOfService                            
    --,isnull(CC.ProviderId, 0)            
  ,DateOfService                            
    ,isnull(CC.ProviderId, 0)  as ProviderId                                
    ,Bc.Name AS BlockCategory                            
    ,Bh.Remarks AS BlockRemarks                            
    ,Bh.CreateDate AS BlockedDate                            
    ,l.[Name] AS ListName                            
   FROM ClinicalCase CC                            
   INNER JOIN WorkItem W ON W.ClinicalCaseId = CC.ClinicalCaseId                            
   LEFT JOIN List l ON l.ListId = CC.ListId                            
   INNER JOIN ProjectUser pu ON pu.ProjectId = w.ProjectId                            
   INNER JOIN BlockHistory Bh ON Bh.ClinicalCaseId = CC.ClinicalcaseId                            
   INNER JOIN BlockCategory Bc ON Bh.BlockCategoryId = Bc.BlockCategoryId                            
   WHERE CC.ProjectId = @ProjectID                            
    AND (                            
     (                            
      w.AssignedTo = @UserId                            
      AND ISNULL(W.IsBlocked, 0) = 1                            
      )                            
     )                            
    AND pu.UserId = @UserId                            
    AND pu.RoleId = 1                            
    AND pu.IsActive = 1                    
 AND W.statusid=2                         
 AND bh.BlockedInQueueKind='coding'               
  AND CC.ClinicalCaseId = @CurrCCId                          
   ORDER BY CC.ClinicalCaseID           
               
   SELECT @CCIDs AS CCIDs              
                         
  END                            
  ELSE IF @Role = 'QA' AND @ChartType = 'Available'                                                                      
BEGIN                                                                      
                               
SELECT TOP 1 @ClinicalCaseId = W.ClinicalCaseId                               
FROM WorkItem W                              
JOIN ProjectUser pu                                                                    
ON pu.ProjectId = W.ProjectId                              
WHERE W.ProjectId = @ProjectID AND W.IsQA = 1 AND ((W.StatusId = 5 AND w.QABy = @UserId AND ISNULL(W.IsBlocked, 0) = 0) OR W.StatusId = 4)                               
AND pu.UserId = @UserId AND pu.RoleId = 2 AND pu.IsActive = 1                              
ORDER BY W.ClinicalCaseId                              
                              
 ;WITH ClaimCTE AS                               
 (                              
 SELECT DISTINCT wp.WorkItemProviderId, wp.ClinicalCaseId, wp.VersionId, wp.ProviderId, wp.PayorId,  wp.ProviderFeedbackId, wp.ClaimId  , DENSE_RANK() OVER(PARTITION BY wp.ClinicalCaseId ORDER BY ISNULL(wp.ClaimId, 0) ) AS ClaimOrder  FROM               
  
    
      
        
          
            
 WorkItemProvider wp  LEFT JOIN Claim c  ON wp.ClinicalCaseId = c.ClinicalCaseId AND wp.VersionId = c.VersionId  WHERE wp.ClinicalCaseId = @ClinicalCaseId                              
 )                              
 SELECT DISTINCT                              
  dbo.fn_GetUserName(AssignedTo) AS CodedBy                                                      
 ,dbo.fn_GetUserName(QABy) AS QABy                                    
 ,dbo.fn_GetUserName(ShadowQABy) AS ShadowQABy,                                                                 
 W.ClinicalCaseID                                       
 ,PatientMRN                                                                      
 ,PatientFirstName + ' ' + PatientLastName AS [Name]                                                                      
 ,CAST(DateOfService AS VARCHAR(10)) AS DateOfService                                                
 ,isnull(c.ProviderId, 0) ProviderId                                                                                         
 ,c.PayorId                                                                      
 ,c.ProviderFeedbackId                                                           
 ,W.NoteTitle                   
 ,dx.DxCode                                                                      
 ,cp.CPTCode                                                       
 ,'' AS Modifier --Need to remove later                              
 ,c.ClaimId                              
 ,c.ClaimOrder                            
 ,p.Name AS ProviderText                        
 ,pa.Name AS PayorText                            
 ,pf.Feedback AS ProviderFeedbackText                            
 ,l.[Name] AS ListName                            
 INTO #QAAvailable               
 FROM ClaimCTE c                              
 LEFT JOIN ClinicalCase cc                              
 ON c.ClinicalCaseId =CC.ClinicalCaseId                              
 LEFT JOIN WorkItem W                             
 ON W.ClinicalCaseId = W.ClinicalCaseId                            
 LEFT JOIN Provider p                            
 ON p.ProviderId = c.ProviderId                            
 LEFT JOIN Payor pa                            
 ON pa.PayorId = c.PayorId                            
 LEFT JOIN ProviderFeedback pf                            
 ON pf.ProviderFeedbackId = c.ProviderFeedbackId                            
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
 ) cp                                                                     
 ON W.ClinicalCaseId = cp.ClinicalCaseId AND cp.VersionId = c.VersionId AND cp.ClaimId = ISNULL(c.ClaimId, 0)                                                                  
 LEFT JOIN ProjectUser pu                                                                    
 ON pu.ProjectId = W.ProjectId                            
 LEFT JOIN List l ON l.ListId = CC.ListId                          
 WHERE W.ClinicalCaseId = @ClinicalCaseId                              
 ORDER BY c.ClaimOrder                              
                                                                      
                                                                      
 UPDATE WorkItem set StatusId = 5, QABy = @UserId, QADate = GETUTCDATE()                               
 WHERE ClinicalCaseId = @ClinicalCaseId                              
 --WHERE ClinicalCaseId = (SELECT ClinicalCaseId FROM #QAAvailable)                              
                               
 --IF NOT EXISTS(SELECT 1 FROM [Version] WHERE UserID = @UserId AND StatusId = 5) AND EXISTS(SELECT 1 FROM #QAAvailable)                              
 --BEGIN                              
  INSERT INTO [Version] (ClinicalcaseID,VersionDate,userID,Statusid)                              
  SELECT @ClinicalCaseId,GETUTCDATE(),@UserId,5                              
  --SELECT (SELECT ClinicalCaseId FROM #QAAvailable),GETUTCDATE(),@UserId,5                              
 --END                              
                                                                      
 SELECT * FROM #QAAvailable                                                                    
                                                                      
END                             
  ELSE IF @Role = 'QA'                            
   AND @ChartType = 'Block'                            
  BEGIN                            
            
    SELECT @CCIDs = COALESCE(@CCIDs + ',', '') + cast(W.Clinicalcaseid as varchar(10))                     
 FROM WorkItem W                              
 JOIN ProjectUser pu                                                                    
 ON pu.ProjectId = W.ProjectId                              
 WHERE W.ProjectId = @ProjectID AND (                            
   (                            
    W.StatusId = 5                            
    AND W.QABy = @UserId                            
    AND ISNULL(W.IsBlocked, 0) = 1                            
    )                           
   )                            
 AND pu.UserId = @UserId                            
  AND pu.RoleId = 2                            
  AND pu.IsActive = 1                              
 ORDER BY W.ClinicalCaseId                              
            
                
 IF @PrevOrNextCCID  = 0                  
 SELECT TOP 1  @CurrCCId = value FROM dbo.fnSplit(@CCIDs, ',')                   
ELSE                  
 SELECT @CurrCCId = @PrevOrNextCCID                
                               
 ;WITH ClaimCTE AS                               
 (                              
 SELECT DISTINCT wp.WorkItemProviderId, wp.ClinicalCaseId, wp.VersionId, wp.ProviderId, wp.PayorId,  wp.ProviderFeedbackId, wp.ClaimId  , DENSE_RANK() OVER(PARTITION BY wp.ClinicalCaseId ORDER BY ISNULL(wp.ClaimId, 0) ) AS ClaimOrder  FROM               
  
     
     
        
          
            
 WorkItemProvider wp  LEFT JOIN Claim c  ON wp.ClinicalCaseId = c.ClinicalCaseId  AND wp.VersionId = c.VersionId  WHERE wp.ClinicalCaseId = @CurrCCId                              
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
 ,W.ProjectId                           
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
  FROM dbo.DxCode dx WHERE dx.ClinicalCaseId = @CurrCCId                              
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
  FROM dbo.cptcode cp WHERE cp.ClinicalCaseId = @CurrCCId                             
 ) cp                                              ON W.ClinicalCaseId = cp.ClinicalCaseId AND cp.VersionId = c.VersionId AND cp.ClaimId = ISNULL(c.ClaimId, 0)                           
   WHERE c.ClinicalCaseId = @CurrCCId AND bh.BlockedInQueueKind = 'QA'              
   ORDER BY c.ClaimOrder              
               
    SELECT @CCIDs AS CCIDs                            
    --UPDATE WorkItem set StatusId = 5, QABy = @UserId, QADate = GETUTCDATE() WHERE ClinicalCaseId = (                                
    --SELECT ClinicalCaseId FROM #QABlocked                                
    --)                                
    --SELECT * FROM #QABlocked                                
  END                            
  ELSE IF @Role = 'QA'                            
   AND @ChartType = 'RebuttalOfCoder'                            
  BEGIN                            
   SELECT TOP 1 @ClinicalCaseId = wi.ClinicalCaseId                            
   FROM WorkItem wi                            
   JOIN ProjectUser pu ON pu.ProjectId = wi.ProjectId                            
   WHERE wi.ProjectId = @ProjectID                            
    AND StatusId = 12                            
    AND wi.QABy = @UserId                            
    AND pu.UserId = @UserId                            
    AND pu.RoleId = 2                            
    AND IsActive = 1                            
   ORDER BY wi.ClinicalCaseId;                            
                            
   WITH ClaimCTE                            
   AS (                            
    SELECT DISTINCT wp.WorkItemProviderId          
     ,wp.ClinicalCaseId                            
     ,wp.VersionId                            
     ,wp.ProviderId                            
     ,wp.PayorId                            
     ,wp.ProviderFeedbackId                            
     ,wp.ClaimId                            
     ,DENSE_RANK() OVER (PARTITION BY wp.ClinicalCaseId ORDER BY ISNULL(wp.ClaimId, 0)) AS ClaimOrder                    
  ,DENSE_RANK() OVER(PARTITION BY wp.ClinicalCaseId ORDER BY wp.VersionId DESC) as rn                    
    FROM WorkItemProvider wp                            
    LEFT JOIN Claim c ON wp.ClinicalCaseId = c.ClinicalCaseId       
     AND wp.VersionId = c.VersionId                            
    WHERE wp.ClinicalCaseId = @ClinicalCaseId                            
    )                            
    ,QAVersionCTE                            
   AS (                            
    SELECT v.ClinicalCaseId                            
     ,v.VersionId                            
,v.VersionDate                            
     ,v.StatusId                            
     ,ROW_NUMBER() OVER (                            
      PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC                            
      ) AS rn                            
    FROM [Version] v                            
    JOIN WorkItem wi ON v.ClinicalCaseId = wi.ClinicalCaseId                            
    WHERE wi.ClinicalCaseId = @ClinicalCaseId                            
     AND wi.StatusId = 12                            
     --AND v.StatusId = 7    
  AND v.StatusId IN (6, 7, 10)    
    )                            
    ,CoderVersionCTE                            
   AS (                            
    SELECT v.ClinicalCaseId                            
     ,v.VersionId                            
     ,v.VersionDate                            
     ,v.StatusId                            
     ,c.ClaimId                            
     ,c.ClaimOrder                            
     ,ROW_NUMBER() OVER (                            
      PARTITION BY v.ClinicalCaseId ORDER BY v.VersionId DESC                            
) AS rn                            
    FROM [Version] v                            
    JOIN WorkItem wi ON v.ClinicalCaseId = wi.ClinicalCaseId                            
    JOIN ClaimCTE c ON c.ClinicalCaseId = wi.ClinicalCaseId                            
    WHERE wi.ClinicalCaseId = @ClinicalCaseId                       
     AND wi.StatusId = 12                               AND v.StatusId = 3                            
    )                            
    ,RebuttedVersionCTE                    
   AS (                            
    SELECT v.ClinicalCaseId                            
     ,v.VersionId                            
     ,v.VersionDate                            
     ,v.StatusId                            
     ,ROW_NUMBER() OVER (                            
      PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC                            
      ) AS rn                            
    FROM [Version] v                            
    JOIN WorkItem wi ON v.ClinicalCaseId = wi.ClinicalCaseId                            
    WHERE wi.ClinicalCaseId = @ClinicalCaseId                            
     AND wi.StatusId = 12                            
     AND v.StatusId = 12                            
    )                            
    ,AuditCTE                            
   AS (                            
    SELECT wa.[ClinicalCaseId]                            
     ,wa.ClaimId                            
     ,MAX(CASE                             
       WHEN FieldName = 'PayorId'                            
        THEN FieldValue                            
       END) AS QAPayorId                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderID'                            
        THEN FieldValue                            
       END) AS QAProviderID                            
     ,MAX(CASE                             
       WHEN FieldName = 'CPTCode'                            
        THEN FieldValue                            
       END) AS QACPTCode                            
     ,MAX(CASE                             
       WHEN FieldName = 'Mod'                            
        THEN FieldValue                            
       END) AS QAMod                            
     ,MAX(CASE                             
       WHEN FieldName = 'Dx'                            
        THEN FieldValue                            
       END) AS QADx                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderFeedbackID'                            
        THEN FieldValue                            
       END) AS QAProviderFeedbackID                            
     ,MAX(CASE                             
    WHEN FieldName = 'PayorId'                            
        THEN Remark                            
       END) AS QAPayorIdRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderID'                            
        THEN Remark                            
       END) AS QAProviderIDRemark                         
    ,MAX(CASE                             
       WHEN FieldName = 'CPTCode'                            
        THEN Remark                            
       END) AS QACPTCodeRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'Mod'                            
        THEN Remark                            
       END) AS QAModRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'Dx'                            
        THEN Remark                            
       END) AS QADxRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderFeedbackID'                            
        THEN Remark                            
       END) AS QAProviderFeedbackIDRemark                        
    ,wa.ErrorTypeId AS QAErrorTypeId                        
    FROM [dbo].[WorkItemAudit] wa                            
    JOIN QAVersionCTE qv ON qv.ClinicalCaseId = wa.ClinicalCaseId                            
     AND wa.VersionId = qv.VersionId    
  WHERE qv.rn = 1    
    GROUP BY wa.ClinicalCaseId                            
     ,wa.ClaimId                        
  ,wa.ErrorTypeId                        
    )                            
    ,RebuttedCTE                            
   AS (                            
    SELECT wa.[ClinicalCaseId]                            
     ,wa.ClaimId                            
     ,MAX(CASE                             
       WHEN FieldName = 'PayorId'                            
        THEN FieldValue                            
       END) AS RebuttedPayorId                            
     ,MAX(CASE            
       WHEN FieldName = 'ProviderID'                            
        THEN FieldValue                            
       END) AS RebuttedProviderID                            
     ,MAX(CASE                             
       WHEN FieldName = 'CPTCode'                            
        THEN FieldValue                            
       END) AS RebuttedCPTCode                            
     ,MAX(CASE                             
       WHEN FieldName = 'Mod'                            
        THEN FieldValue                            
       END) AS RebuttedMod                            
     ,MAX(CASE                             
       WHEN FieldName = 'Dx'                            
        THEN FieldValue                            
       END) AS RebuttedDx                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderFeedbackID'                            
        THEN FieldValue                            
       END) AS RebuttedProviderFeedbackID                            
     ,MAX(CASE                             
       WHEN FieldName = 'PayorId'                            
        THEN Remark                            
       END) AS RebuttedPayorIdRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderID'                            
        THEN Remark                            
       END) AS RebuttedProviderIDRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'CPTCode'                            
        THEN Remark                            
END) AS RebuttedCPTCodeRemark                            
     ,MAX(CASE             
       WHEN FieldName = 'Mod'                            
        THEN Remark                            
       END) AS RebuttedModRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'Dx'                            
        THEN Remark                            
       END) AS RebuttedDxRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderFeedbackID'                            
        THEN Remark                            
       END) AS RebuttedProviderFeedbackIDRemark                            
    FROM [dbo].[WorkItemAudit] wa                            
    JOIN RebuttedVersionCTE rv ON rv.ClinicalCaseId = wa.ClinicalCaseId                            
     AND wa.VersionId = rv.VersionId    
  WHERE rv.rn = 1    
    GROUP BY wa.ClinicalCaseId                            
     ,wa.ClaimId                            
    )                            
   SELECT DISTINCT cc.ClinicalCaseID                            
    ,PatientMRN                            
    ,PatientFirstName + ' ' + PatientLastName AS [Name]                            
    ,CAST(DateOfService AS VARCHAR(10)) AS DateOfService                            
    ,isnull(isnull(CC.ProviderId, 0), 0)                            
    ,dbo.fn_GetUserName(AssignedTo) AS CodedBy                            
    ,dbo.fn_GetUserName(QABy) AS QABy                            
    ,dbo.fn_GetUserName(ShadowQABy) AS ShadowQABy                            
    ,c.ProviderId                            
    ,c.PayorId                            
    ,c.ProviderFeedbackId                            
    ,wi.NoteTitle                            
    ,dx.DxCode                            
    ,cpt.CPTCode                            
    ,QAPayorId                            
    ,QAProviderID                            
    ,QACPTCode                            
    ,QAMod                            
    ,QADx                            
    ,QAProviderFeedbackID                            
    ,QAPayorIdRemark                            
    ,QAProviderIDRemark                            
    ,QACPTCodeRemark                            
    ,QAModRemark                            
    ,QADxRemark                            
    ,QAProviderFeedbackIDRemark                            
    ,RebuttedPayorId                            
    ,RebuttedProviderID                            
    ,RebuttedCPTCode                            
    ,RebuttedMod                            
    ,RebuttedDx                            
    ,RebuttedProviderFeedbackID                            
    ,RebuttedPayorIdRemark                            
    ,RebuttedProviderIDRemark                            
    ,RebuttedCPTCodeRemark                            
    ,RebuttedModRemark                            
    ,RebuttedDxRemark                            
    ,RebuttedProviderFeedbackIDRemark                            
    ,c.ClaimId                            
    ,c.ClaimOrder                            
    ,pf.Feedback AS ProviderFeedbackText                            
    ,p.Name AS ProviderText                            
    ,pay.Name AS PayorText                            
    ,QAP.Name AS QAProviderText                            
    ,QAPa.Name AS QAPayorText                            
    ,QAPf.Feedback AS QAProviderFeedbackText                            
    ,l.[Name] AS ListName                        
    ,QAErrorTypeId                      
   FROM ClaimCTE c                            
   JOIN ClinicalCase cc ON cc.ClinicalCaseId = c.ClinicalCaseId                            
   INNER JOIN WorkItem wi ON wi.ClinicalCaseId = cc.ClinicalCaseId    
   LEFT JOIN CoderVersionCTE cv                                                           
      ON cv.ClinicalCaseId = cc.ClinicalCaseId    
   LEFT JOIN (                            
    SELECT DISTINCT dx.ClinicalCaseId                            
     ,dx.VersionId                 
     ,dx.ClaimId                            
     ,SUBSTRING((                            
       SELECT ',' + dxc.DxCode AS [text()]                            
       FROM dbo.DxCode dxc                            
       WHERE dx.ClinicalCaseId = dxc.ClinicalCaseId                            
        AND dxc.VersionId = dx.VersionId                            
        AND ISNULL(dx.ClaimId, 0) = ISNULL(dxc.ClaimId, 0)                
       ORDER BY dxc.ClinicalCaseId                            
       FOR XML PATH('')                            
       ), 2, 1000) [DxCode]                            
    FROM dbo.DxCode dx                            
    WHERE ClinicalCaseId = @ClinicalCaseId                            
    ) dx ON dx.ClinicalCaseId = c.ClinicalCaseId                            
    AND dx.VersionID = cv.VersionID                            
    AND ISNULL(dx.ClaimId, 0) = ISNULL(c.ClaimId, 0)                            
   LEFT JOIN (                            
    SELECT DISTINCT cp.ClinicalCaseId                            
     ,cp.VersionId                            
     ,cp.ClaimId                            
     ,SUBSTRING((                            
       SELECT '|' + cptc.cptCode + '^' + isnull(cptc.Modifier, '') + '^' + ISNULL(cptc.qty, '') + '^' + isnull(cptc.Links, '') AS [text()]                            
       FROM dbo.cptcode cptc                          
       WHERE cp.ClinicalCaseId = cptc.ClinicalCaseId                            
        AND cp.VersionID = cptc.VersionID                            
        AND ISNULL(cp.ClaimId, 0) = ISNULL(cptc.ClaimId, 0)                            
       ORDER BY cptc.ClinicalCaseId                            
       FOR XML PATH('')                            
       ), 2, 1000) [CptCode]                            
    FROM dbo.cptcode cp                            
    WHERE ClinicalCaseId = @ClinicalCaseId                    ) cpt ON cc.ClinicalCaseId = cpt.ClinicalCaseId                            
    AND cpt.VersionId = cv.VersionId                            
    AND ISNULL(cpt.ClaimId, 0) = ISNULL(c.ClaimId, 0)                            
   LEFT JOIN AuditCTE a ON a.ClinicalCaseId = c.ClinicalCaseId AND ISNULL(a.ClaimId, 0) = ISNULL(c.ClaimId, 0)                            
   LEFT JOIN RebuttedCTE r ON r.ClinicalCaseId = c.ClinicalCaseId AND ISNULL(r.ClaimId, 0) = ISNULL(c.ClaimId, 0)                          
   LEFT JOIN Provider p ON p.ProviderId = c.ProviderId                            
   LEFT JOIN Payor pay ON pay.PayorId = c.PayorId                            
   LEFT JOIN ProviderFeedback pf ON pf.ProviderFeedbackId = c.ProviderFeedbackId                            
   LEFT JOIN [Provider] QAP ON a.QAProviderID = QAP.ProviderID                            
   LEFT JOIN ProviderFeedback QAPf ON a.QAProviderFeedbackID = QAPf.ProviderFeedbackId                            
   LEFT JOIN Payor QAPa ON QAPa.PayorId = a.QAPayorId                            
   --LEFT JOIN ProjectUser pu                                                                    
   --ON pu.ProjectId = wi.ProjectId                            
   LEFT JOIN List l ON l.ListId = CC.ListId                            
   WHERE wi.ClinicalCaseId = @ClinicalCaseId AND c.rn = 1                            
   ORDER BY c.ClaimOrder                            
                                                                      
  END                            
  ELSE IF @Role = 'QA'                            
   AND @ChartType = 'ShadowQARejected' --RebuttalOfShadowQA                                                                    
  BEGIN                            
    SELECT TOP 1 @ClinicalCaseId = wi.ClinicalCaseId                            
   FROM WorkItem wi                            
   JOIN ProjectUser pu ON pu.ProjectId = wi.ProjectId                            
   WHERE wi.ProjectId = @ProjectID                            
    AND wi.StatusId = 11                           
 AND wi.QABy = @UserId                          
    AND pu.UserId = @UserId                            
    AND pu.RoleId = 2                            
    AND pu.IsActive = 1                           
   ORDER BY wi.ClinicalCaseId;                            
                            
   ;WITH ClaimCTE                            
   AS (                            
    SELECT DISTINCT wa.ClinicalCaseId                            
     ,wa.VersionId                            
     ,wp.ProviderId                            
     ,wp.PayorId                            
     ,wp.ProviderFeedbackId                             
     ,wa.ClaimId                            
     ,DENSE_RANK() OVER (                            
      PARTITION BY wa.ClinicalCaseId ORDER BY ISNULL(wa.ClaimId, 0)                            
      ) AS ClaimOrder          
    FROM WorkItemAudit wa                            
    LEFT JOIN Claim c ON wa.ClinicalCaseId = c.ClinicalCaseId                            
     AND wa.VersionId = c.VersionId          
 JOIN (          
  SELECT MAX(VersionId) AS VersionId FROM [Version] WHERE ClinicalCaseId = @ClinicalCaseId AND StatusId NOT IN (18, 19)
 ) AS v ON v.VersionId = wa.VersionId          
 JOIN WorkItemProvider wp ON wp.ClinicalCaseId = wa.ClinicalCaseId AND ISNULL(wp.ClaimId, 0) = ISNULL(wa.ClaimId, 0)          
    WHERE wp.ClinicalCaseId = @ClinicalCaseId                            
    )                            
    , QAVersionCTE                            
   AS (                            
    SELECT v.ClinicalCaseId                            
     ,v.VersionId                            
     ,v.VersionDate                            
     ,v.StatusId                            
     ,ROW_NUMBER() OVER (                            
      PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC                            
      ) AS rn                            
    FROM [Version] v                            
    JOIN WorkItem wi ON v.ClinicalCaseId = wi.ClinicalCaseId                            
    WHERE wi.ClinicalCaseId = @ClinicalCaseId    
     AND wi.StatusId = 11                            
     AND v.StatusId IN (6,7,13)                            
    )                            
    ,CoderVersionCTE                            
   AS (                            
    SELECT v.ClinicalCaseId                            
     ,v.VersionId                            
     ,v.VersionDate                            
     ,v.StatusId                            
     ,ROW_NUMBER() OVER (                            
      PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC                            
      ) AS rn                            
    FROM [Version] v                            
    JOIN WorkItem wi ON v.ClinicalCaseId = wi.ClinicalCaseId                            
    WHERE wi.ClinicalCaseId = @ClinicalCaseId    
     AND wi.StatusId = 11                            
     AND v.StatusId = 3                            
    )                            
    ,ShadowQAVersionCTE                            
   AS (                   
    SELECT v.ClinicalCaseId                            
     ,v.VersionId                            
     ,v.VersionDate                            
     ,v.StatusId                            
     ,ROW_NUMBER() OVER (                            
      PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC                            
      ) AS rn                            
    FROM [Version] v                            
    JOIN WorkItem wi ON v.ClinicalCaseId = wi.ClinicalCaseId                            
    WHERE wi.ClinicalCaseId = @ClinicalCaseId    
     AND wi.StatusId = 11                            
     AND v.StatusId = 11                            
    )                            
    ,RebuttedVersionCTE                            
   AS (                            
    SELECT v.ClinicalCaseId                            
     ,v.VersionId                            
     ,v.VersionDate                            
     ,v.StatusId                            
     ,ROW_NUMBER() OVER (                            
      PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC                            
      ) AS rn                            
    FROM [Version] v                            
    JOIN WorkItem wi ON v.ClinicalCaseId = wi.ClinicalCaseId                            
    WHERE wi.ClinicalCaseId = @ClinicalCaseId    
     AND wi.StatusId = 11                            
     AND v.StatusId = 13                            
    )                            
    ,RebuttedCTE                            
   AS (                            
    SELECT wa.[ClinicalCaseId]                            
     ,wa.ClaimId                            
     ,MAX(CASE                             
       WHEN FieldName = 'PayorId'                            
        THEN FieldValue                            
       END) AS RebuttedPayorId                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderID'                            
        THEN FieldValue                            
       END) AS RebuttedProviderID             
     ,MAX(CASE                             
       WHEN FieldName = 'CPTCode'                            
        THEN FieldValue                            
       END) AS RebuttedCPTCode                            
     ,MAX(CASE                             
       WHEN FieldName = 'Mod'                            
        THEN FieldValue                            
       END) AS RebuttedMod                            
     ,MAX(CASE                             
       WHEN FieldName = 'Dx'                            
        THEN FieldValue                            
       END) AS RebuttedDx                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderFeedbackID'                            
        THEN FieldValue                            
       END) AS RebuttedProviderFeedbackID                            
     ,MAX(CASE                             
       WHEN FieldName = 'PayorId'                            
        THEN Remark                            
       END) AS RebuttedPayorIdRemark                            
     ,MAX(CASE                            
       WHEN FieldName = 'ProviderID'                            
        THEN Remark                            
       END) AS RebuttedProviderIDRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'CPTCode'                            
        THEN Remark END) AS RebuttedCPTCodeRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'Mod'                            
        THEN Remark                            
       END) AS RebuttedModRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'Dx'                            
        THEN Remark                            
       END) AS RebuttedDxRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderFeedbackID'                            
        THEN Remark                            
       END) AS RebuttedProviderFeedbackIDRemark                            
    FROM [dbo].[WorkItemAudit] wa                            
    JOIN RebuttedVersionCTE rv ON rv.ClinicalCaseId = wa.ClinicalCaseId                            
     AND wa.VersionId = rv.VersionId          
  WHERE rv.rn = 1                            
    GROUP BY wa.ClinicalCaseId                            
     ,wa.ClaimId                            
    )                            
    ,QAAcceptedCTE                            
   AS (                            
    SELECT wa.[ClinicalCaseId]                            
     ,wa.ClaimId                            
     ,MAX(CASE                             
       WHEN FieldName = 'PayorId' THEN FieldValue                            
       END) AS QAPayorId                            
 ,MAX(CASE                             
       WHEN FieldName = 'ProviderID'                            
        THEN FieldValue                            
       END) AS QAProviderID                            
     ,MAX(CASE                             
       WHEN FieldName = 'CPTCode'                            
        THEN FieldValue                            
       END) AS QACPTCode                            
     ,MAX(CASE                             
       WHEN FieldName = 'Mod'                            
        THEN FieldValue                            
       END) AS QAMod                            
     ,MAX(CASE                             
       WHEN FieldName = 'Dx'                            
        THEN FieldValue                            
       END) AS QADx                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderFeedbackID'                            
     THEN FieldValue                            
       END) AS QAProviderFeedbackID                            
     ,MAX(CASE                             
       WHEN FieldName = 'PayorId'                            
        THEN Remark              
       END) AS QAPayorIdRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderID'                            
        THEN Remark                            
       END) AS QAProviderIDRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'CPTCode'                            
        THEN Remark                      
       END) AS QACPTCodeRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'Mod'                            
        THEN Remark                            
       END) AS QAModRemark                            
     ,MAX(CASE                     
       WHEN FieldName = 'Dx'                            
        THEN Remark                            
       END) AS QADxRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderFeedbackID'                            
        THEN Remark                            
       END) AS QAProviderFeedbackIDRemark                            
     ,wa.ErrorTypeId AS QAErrorTypeId                      
    FROM [dbo].[WorkItemAudit] wa                            
    JOIN QAVersionCTE qv ON qv.ClinicalCaseId = wa.ClinicalCaseId                            
AND wa.VersionId = qv.VersionId          
  WHERE qv.rn = 1 AND wa.IsAccepted = 1    
    GROUP BY wa.ClinicalCaseId                            
     ,wa.ErrorTypeId                  
     ,wa.ClaimId                            
    )  
 ,QARejectedCTE                            
   AS (                            
    SELECT wa.[ClinicalCaseId]                            
     ,wa.ClaimId                            
     ,MAX(CASE                             
       WHEN FieldName = 'PayorId' THEN FieldValue                            
       END) AS QAPayorId                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderID'                            
        THEN FieldValue                            
       END) AS QAProviderID                            
     ,MAX(CASE                             
       WHEN FieldName = 'CPTCode'                            
        THEN FieldValue                            
       END) AS QACPTCode                            
     ,MAX(CASE                             
       WHEN FieldName = 'Mod'                            
        THEN FieldValue                            
       END) AS QAMod                            
     ,MAX(CASE                             
       WHEN FieldName = 'Dx'                            
        THEN FieldValue                            
       END) AS QADx                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderFeedbackID'                                 THEN FieldValue                            
       END) AS QAProviderFeedbackID                            
     ,MAX(CASE                             
       WHEN FieldName = 'PayorId'                            
        THEN Remark              
       END) AS QAPayorIdRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderID'                            
        THEN Remark                            
       END) AS QAProviderIDRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'CPTCode'                            
        THEN Remark                      
       END) AS QACPTCodeRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'Mod'                            
        THEN Remark                            
       END) AS QAModRemark                            
     ,MAX(CASE                     
       WHEN FieldName = 'Dx'                            
        THEN Remark                            
       END) AS QADxRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderFeedbackID'                            
        THEN Remark                            
       END) AS QAProviderFeedbackIDRemark                            
     ,wa.ErrorTypeId AS QAErrorTypeId                      
    FROM [dbo].[WorkItemAudit] wa                            
    JOIN QAVersionCTE qv ON qv.ClinicalCaseId = wa.ClinicalCaseId                            
AND wa.VersionId = qv.VersionId          
  WHERE qv.rn = 1 AND wa.IsAccepted = 0    
    GROUP BY wa.ClinicalCaseId                            
     ,wa.ErrorTypeId                  
     ,wa.ClaimId                            
    )                            
    ,ShadowQAAcceptedCTE                            
   AS (                            
    SELECT wa.[ClinicalCaseId]                            
     ,wa.ClaimId                            
     ,MAX(CASE                             
       WHEN FieldName = 'PayorId'                            
        THEN FieldValue                            
       END) AS ShadowQAPayorId                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderID'                            
        THEN FieldValue                            
       END) AS ShadowQAProviderID                            
     ,MAX(CASE                             
       WHEN FieldName = 'CPTCode'                            
        THEN FieldValue                            
       END) AS ShadowQACPTCode                            
     ,MAX(CASE                             
 WHEN FieldName = 'Mod'                            
        THEN FieldValue                            
       END) AS ShadowQAMod                            
     ,MAX(CASE                             
       WHEN FieldName = 'Dx'                            
        THEN FieldValue                            
       END) AS ShadowQADx                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderFeedbackID'                            
        THEN FieldValue                            
       END) AS ShadowQAProviderFeedbackID                            
     ,MAX(CASE                             
       WHEN FieldName = 'PayorId'                            
        THEN Remark                            
       END) AS ShadowQAPayorIdRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderID'                         
        THEN Remark                            
       END) AS ShadowQAProviderIDRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'CPTCode'                            
        THEN Remark                            
       END) AS ShadowQACPTCodeRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'Mod'                            
        THEN Remark                            
       END) AS ShadowQAModRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'Dx'                            
        THEN Remark                            
       END) AS ShadowQADxRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderFeedbackID'                            
        THEN Remark                            
       END) AS ShadowQAProviderFeedbackIDRemark                            
     ,wa.ErrorTypeId AS ShadowQAErrorTypeId                      
    FROM [dbo].[WorkItemAudit] wa                            
    JOIN ShadowQAVersionCTE qv ON qv.ClinicalCaseId = wa.ClinicalCaseId                            
     AND wa.VersionId = qv.VersionId          
  WHERE qv.rn = 1 AND wa.IsAccepted = 1    
    GROUP BY wa.ClinicalCaseId                            
     ,wa.ErrorTypeId                            
     ,wa.ClaimId                            
    )  
 ,ShadowQARejectedCTE                            
   AS (                            
    SELECT wa.[ClinicalCaseId]                            
     ,wa.ClaimId                            
     ,MAX(CASE                             
       WHEN FieldName = 'PayorId'                            
        THEN FieldValue                            
       END) AS ShadowQAPayorId                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderID'                            
        THEN FieldValue                            
       END) AS ShadowQAProviderID                            
     ,MAX(CASE                             
       WHEN FieldName = 'CPTCode'                            
        THEN FieldValue                            
       END) AS ShadowQACPTCode                            
     ,MAX(CASE                             
 WHEN FieldName = 'Mod'                            
        THEN FieldValue                            
       END) AS ShadowQAMod                            
     ,MAX(CASE                             
       WHEN FieldName = 'Dx'                            
        THEN FieldValue                            
       END) AS ShadowQADx                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderFeedbackID'                            
        THEN FieldValue                            
       END) AS ShadowQAProviderFeedbackID                            
     ,MAX(CASE                             
       WHEN FieldName = 'PayorId'                            
        THEN Remark                            
       END) AS ShadowQAPayorIdRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderID'                         
        THEN Remark                            
       END) AS ShadowQAProviderIDRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'CPTCode'                            
        THEN Remark                            
       END) AS ShadowQACPTCodeRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'Mod'                            
        THEN Remark                            
       END) AS ShadowQAModRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'Dx'                            
        THEN Remark                            
       END) AS ShadowQADxRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderFeedbackID'                            
        THEN Remark                            
       END) AS ShadowQAProviderFeedbackIDRemark                            
     ,wa.ErrorTypeId AS ShadowQAErrorTypeId                      
    FROM [dbo].[WorkItemAudit] wa                            
    JOIN ShadowQAVersionCTE qv ON qv.ClinicalCaseId = wa.ClinicalCaseId                            
     AND wa.VersionId = qv.VersionId          
  WHERE qv.rn = 1 AND wa.IsAccepted = 0    
    GROUP BY wa.ClinicalCaseId                            
     ,wa.ErrorTypeId                            
     ,wa.ClaimId                            
    )    
   SELECT DISTINCT cc.ClinicalCaseID                            
    ,PatientMRN                            
 ,PatientFirstName + ' ' + PatientLastName AS [Name]                            
    ,CAST(DateOfService AS VARCHAR(10)) AS DateOfService                            
    ,isnull(CC.ProviderId, 0)                            
    ,dbo.fn_GetUserName(AssignedTo) AS CodedBy                            
    ,dbo.fn_GetUserName(QABy) AS QABy                            
    ,dbo.fn_GetUserName(ShadowQABy) AS ShadowQABy                            
    ,c.ProviderId                            
    ,p.Name AS ProviderText                            
    ,c.PayorId                            
    ,pa.Name AS PayorText                            
    ,c.ProviderFeedbackId                            
    ,Pf.Feedback AS FeedbackText                            
    ,wi.NoteTitle                            
    ,dx.DxCode                            
    ,cpt.CPTCode  
 ,ISNULL(qaa.QAPayorId, qar.QAPayorId) AS QAPayorId  
    ,ISNULL(qaa.QAProviderID, qar.QAProviderID) AS QAProviderID  
 ,ISNULL(qaa.QACPTCode, '') + '|' + ISNULL(qar.QACPTCode, '') AS QACPTCode    
    ,NULL AS QAMod                           
 ,ISNULL(qaa.QADx, '') + '|' + ISNULL(qar.QADx, '') AS QADx    
    ,ISNULL(qaa.QAProviderFeedbackID, qar.QAProviderFeedbackID) AS QAProviderFeedbackID                            
    ,ISNULL(qaa.QAPayorIdRemark, qar.QAPayorIdRemark) AS QAPayorIdRemark                            
    ,ISNULL(qaa.QAProviderIDRemark, qar.QAProviderIDRemark) AS QAProviderIDRemark                            
    ,ISNULL(qaa.QACPTCodeRemark, '') + '|' + ISNULL(qar.QACPTCodeRemark, '') AS QACPTCodeRemark                            
    ,NULL AS QAModRemark                            
    ,ISNULL(qaa.QADxRemark, '') + '|' + ISNULL(qar.QADxRemark, '') AS QADxRemark                            
    ,ISNULL(qaa.QAProviderFeedbackIDRemark, qar.QAProviderFeedbackIDRemark) AS QAProviderFeedbackIDRemark    
                           
    ,ISNULL(QAPa.[Name], QAPar.[Name]) AS QAPayorText                
    ,ISNULL(QAP.[Name], QAPr.[Name]) AS QAProviderText                      
    ,ISNULL(QAPf.Feedback, QAPfr.Feedback) AS QAFeedbackText                       
    --ShadowQA Fields                                                    
    ,ISNULL(sa.ShadowQAPayorId, sar.ShadowQAPayorId) AS ShadowQAPayorId                            
    ,ISNULL(SQAPa.[Name], SQAPar.[Name]) AS ShadowQAPayorText                            
    ,ISNULL(sa.ShadowQAProviderID, sar.ShadowQAProviderID) AS ShadowQAProviderID                            
    ,ISNULL(SQAP.[Name], SQAPr.[Name]) AS ShadowQAProviderText                            
    ,ISNULL(sa.ShadowQACPTCode, '') + '|' + ISNULL(sar.ShadowQACPTCode, '') AS ShadowQACPTCode                            
    ,NULL AS ShadowQAMod                            
    ,ISNULL(sa.ShadowQADx, '') + '|' + ISNULL(sar.ShadowQADx, '') AS ShadowQADx                            
    ,ISNULL(sa.ShadowQAProviderFeedbackID, sar.ShadowQAProviderFeedbackID) AS ShadowQAProviderFeedbackID                            
    ,ISNULL(SQAPf.Feedback, SQAPfr.Feedback) AS ShadowQAFeedbackText                            
    ,ISNULL(sa.ShadowQAPayorIdRemark, sar.ShadowQAPayorIdRemark) AS ShadowQAPayorIdRemark                            
    ,ISNULL(sa.ShadowQAProviderIDRemark, sar.ShadowQAProviderIDRemark) AS ShadowQAProviderIDRemark                            
    ,ISNULL(sa.ShadowQACPTCodeRemark, '') + '|' + ISNULL(sar.ShadowQACPTCodeRemark, '') AS ShadowQACPTCodeRemark                            
    ,NULL AS ShadowQAModRemark                            
    ,ISNULL(sa.ShadowQADxRemark, '') + '|' + ISNULL(sar.ShadowQADxRemark, '') AS ShadowQADxRemark                            
    ,ISNULL(sa.ShadowQAProviderFeedbackIDRemark, sar.ShadowQAProviderFeedbackIDRemark) AS ShadowQAProviderFeedbackIDRemark                            
    ,RebuttedPayorId                            
    ,RebuttedProviderID                            
    ,RebuttedCPTCode                            
    ,RebuttedMod                            
    ,RebuttedDx                            
    ,RebuttedProviderFeedbackID                            
    ,RebuttedPayorIdRemark                            
    ,RebuttedProviderIDRemark                            
    ,RebuttedCPTCodeRemark                            
    ,RebuttedModRemark                            
    ,RebuttedDxRemark                            
    ,RebuttedProviderFeedbackIDRemark                            
    ,ISNULL(qaa.QAErrorTypeId, qar.QAErrorTypeId) AS QAErrorTypeId                      
    ,ISNULL(sa.ShadowQAErrorTypeId, sar.ShadowQAErrorTypeId) AS ShadowQAErrorTypeId                            
    ,l.[Name] AS ListName                            
    ,c.ClaimId                            
    ,c.ClaimOrder                            
   FROM ClaimCTE c                            
   JOIN ClinicalCase cc ON cc.ClinicalCaseId = c.ClinicalCaseId                          
   INNER JOIN WorkItem wi ON wi.ClinicalCaseId = cc.ClinicalCaseId                             
   LEFT JOIN [Provider] P ON c.ProviderId = P.ProviderID                            
   LEFT JOIN ProviderFeedback Pf ON c.ProviderFeedbackId = Pf.ProviderFeedbackId                            
   LEFT JOIN Payor Pa ON c.PayorId = pa.PayorId                            
   LEFT JOIN QAVersionCTE qv ON qv.ClinicalCaseId = cc.ClinicalCaseId                            
   LEFT JOIN CoderVersionCTE cv ON cv.ClinicalCaseId = cc.ClinicalCaseId                                           
   LEFT JOIN (                            
    SELECT DISTINCT dx.ClinicalCaseId                            
     ,dx.VersionId                            
 ,dx.ClaimId                            
     ,SUBSTRING((                            
       SELECT ',' + dxc.DxCode AS [text()]                            
       FROM dbo.DxCode dxc                       
       WHERE dx.ClinicalCaseId = dxc.ClinicalCaseId                            
        AND dxc.VersionId = dx.VersionId                            
        AND ISNULL(dx.ClaimId, 0) = ISNULL(dxc.ClaimId, 0)                            
       ORDER BY dxc.ClinicalCaseId                            
       FOR XML PATH('')                            
       ), 2, 1000) [DxCode]                            
    FROM dbo.DxCode dx                            
    WHERE ClinicalCaseId = @ClinicalCaseId                            
    ) dx ON dx.ClinicalCaseId = c.ClinicalCaseId                            
    AND dx.VersionID = cv.VersionID                            
    AND ISNULL(dx.ClaimId, 0) = ISNULL(c.ClaimId, 0)                            
   LEFT JOIN (                            
    SELECT DISTINCT cp.ClinicalCaseId                            
     ,cp.VersionId                            
     ,cp.ClaimId                  
     ,SUBSTRING((                            
       SELECT '|' + cptc.cptCode + '^' + isnull(cptc.Modifier, '') + '^' + ISNULL(cptc.qty, '') + '^' + isnull(cptc.Links, '') AS [text()]                            
       FROM dbo.cptcode cptc                            
       WHERE cp.ClinicalCaseId = cptc.ClinicalCaseId                            
        AND cp.VersionID = cptc.VersionID                            
        AND ISNULL(cp.ClaimId, 0) = ISNULL(cptc.ClaimId, 0)                            
       ORDER BY cptc.ClinicalCaseId                            
       FOR XML PATH('')                            
       ), 2, 1000) [CptCode]                            
    FROM dbo.cptcode cp                            
    WHERE ClinicalCaseId = @ClinicalCaseId                            
    ) cpt ON cc.ClinicalCaseId = cpt.ClinicalCaseId                            
    AND cpt.VersionId = cv.VersionId                            
    AND ISNULL(cpt.ClaimId, 0) = ISNULL(c.ClaimId, 0)                            
   LEFT JOIN QAAcceptedCTE qaa ON qaa.ClinicalCaseId = c.ClinicalCaseId AND ISNULL(qaa.ClaimId, 0) = ISNULL(c.ClaimId, 0)  
   LEFT JOIN [Provider] QAP ON qaa.QAProviderID = QAP.ProviderID  
   LEFT JOIN ProviderFeedback QAPf ON qaa.QAProviderFeedbackID = QAPf.ProviderFeedbackId  
   LEFT JOIN Payor QAPa ON QAPa.PayorId = qaa.QAPayorId                               
   LEFT JOIN QARejectedCTE qar ON qar.ClinicalCaseId = c.ClinicalCaseId AND ISNULL(qar.ClaimId, 0) = ISNULL(c.ClaimId, 0)  
   LEFT JOIN [Provider] QAPr ON qar.QAProviderID = QAPr.ProviderID  
   LEFT JOIN ProviderFeedback QAPfr ON qar.QAProviderFeedbackID = QAPfr.ProviderFeedbackId  
   LEFT JOIN Payor QAPar ON QAPar.PayorId = qar.QAPayorId  
   LEFT JOIN ShadowQAAcceptedCTE sa ON sa.ClinicalCaseId = c.ClinicalCaseId AND ISNULL(sa.ClaimId, 0) = ISNULL(c.ClaimId, 0)  
   LEFT JOIN ShadowQARejectedCTE sar ON sar.ClinicalCaseId = c.ClinicalCaseId AND ISNULL(sar.ClaimId, 0) = ISNULL(c.ClaimId, 0)  
   LEFT JOIN [Provider] SQAP ON sa.ShadowQAProviderID = SQAP.ProviderID                            
   LEFT JOIN ProviderFeedback SQAPf ON sa.ShadowQAProviderFeedbackID = SQAPf.ProviderFeedbackId                            
   LEFT JOIN Payor SQAPa ON SQAPa.PayorId = sa.ShadowQAPayorId  
   LEFT JOIN [Provider] SQAPr ON sar.ShadowQAProviderID = SQAP.ProviderID                            
   LEFT JOIN ProviderFeedback SQAPfr ON sar.ShadowQAProviderFeedbackID = SQAPf.ProviderFeedbackId                            
   LEFT JOIN Payor SQAPar ON SQAPa.PayorId = sar.ShadowQAPayorId  
   LEFT JOIN RebuttedCTE r ON r.ClinicalCaseId = c.ClinicalCaseId AND ISNULL(r.ClaimId, 0) = ISNULL(c.ClaimId, 0)                            
   LEFT JOIN List l ON l.ListId = CC.ListId                            
   WHERE c.ClinicalCaseId = @ClinicalCaseId                          
   ORDER BY c.ClaimOrder                            
  END                            
  --ELSE IF @Role = 'QA'                            
  -- AND @ChartType = 'IsBlocked'                            
  --BEGIN                            
  --  ;                            
                            
  -- WITH VersionCTE                            
  -- AS (                            
  --  SELECT v.ClinicalCaseId                            
  --   ,v.VersionId                            
  --   ,v.VersionDate                            
  --   ,ROW_NUMBER() OVER (                            
  --    PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC                    
  --    ) AS rn                            
  --  FROM [Version] v                            
  --  JOIN WorkItem wi ON v.ClinicalCaseId = wi.ClinicalCaseId                            
  --  WHERE wi.ProjectId = @ProjectID                            
  --   AND wi.StatusId = 2                            
  --   AND wi.IsBlocked = 1                            
  --   --AND v.StatusId = 13                                                                    
  --  )                            
  -- SELECT TOP 1 dbo.fn_GetUserName(AssignedTo) AS CodedBy                            
  --  ,dbo.fn_GetUserName(QABy) AS QABy                
  --  ,dbo.fn_GetUserName(ShadowQABy) AS ShadowQABy                            
  --  ,cc.ClinicalCaseID                      
  --  ,PatientMRN                            
  --  ,PatientFirstName + ' ' + PatientLastName AS [Name]                            
  --  ,CAST(DateOfService AS VARCHAR(10)) AS DateOfService                            
  --  ,isnull(CC.ProviderId, 0)                            
  --  ,wp.PayorId                            
  --  ,wp.ProviderFeedbackId                            
  --  ,wi.NoteTitle                            
  --  ,dx.DxCode                            
  --  ,cpt.CPTCode                            
  --  ,cpt.Modifier                   
  --  ,Cq.Question                            
  --  ,l.[Name] AS ListName                            
  -- FROM ClinicalCase cc                            
  -- INNER JOIN WorkItem wi ON wi.ClinicalCaseId = cc.ClinicalCaseId                            
  -- INNER JOIN CoderQuestion Cq ON Cq.ClinicalCaseId = CC.ClinicalCaseId                            
  -- LEFT JOIN WorkItemProvider wp ON wp.ClinicalCaseId = CC.ClinicalCaseId                            
  -- LEFT JOIN VersionCTE v ON v.ClinicalCaseId = cc.ClinicalCaseId                            
  -- LEFT JOIN DxCode dx ON cc.ClinicalCaseId = dx.ClinicalCaseId                            
  --  AND dx.VersionId = v.VersionId                            
  -- LEFT JOIN CptCode cpt ON cc.ClinicalCaseId = cpt.ClinicalCaseId                            
  --  AND cpt.VersionId = v.VersionId                            
  -- JOIN ProjectUser pu ON pu.ProjectId = wi.ProjectId                            
  -- LEFT JOIN List l ON l.ListId = CC.ListId                            
  -- WHERE cc.ProjectId = @ProjectID                            
  --  AND wi.StatusId = 2                            
  --  AND wi.IsBlocked = 1                            
  --  AND pu.UserId = @UserId                            
  --  AND pu.RoleId = 2                            
  --  AND pu.IsActive = 1                            
  --  AND v.rn = 1                            
  -- ORDER BY v.VersionDate                            
  --END                            
  ELSE IF @Role = 'ShadowQA'                            
   AND @ChartType = 'RebuttalOfQA'                            
  BEGIN                        
    SELECT TOP 1 @ClinicalCaseId = wi.ClinicalCaseId                            
   FROM WorkItem wi                            
   JOIN ProjectUser pu ON pu.ProjectId = wi.ProjectId                            
   WHERE wi.ProjectId = @ProjectID                            
    AND wi.StatusId = 13                          
 AND wi.ShadowQABy = @UserId                          
    AND pu.UserId = @UserId                            
    AND pu.RoleId = 3                            
    AND pu.IsActive = 1                             
   ORDER BY wi.ClinicalCaseId;                            
                            
   ;WITH ClaimCTE                            
   AS (                            
    SELECT DISTINCT wa.WorkItemAuditId                            
     ,wa.ClinicalCaseId                            
     ,wa.VersionId                            
     ,wp.ProviderId                            
     ,wp.PayorId                            
     ,wp.ProviderFeedbackId                             
     ,wa.ClaimId                            
     ,DENSE_RANK() OVER (                            
      PARTITION BY wa.ClinicalCaseId ORDER BY ISNULL(wa.ClaimId, 0)                            
      ) AS ClaimOrder          
    FROM WorkItemAudit wa                            
    LEFT JOIN Claim c ON wa.ClinicalCaseId = c.ClinicalCaseId                            
     AND wa.VersionId = c.VersionId          
 JOIN (          
  SELECT MAX(VersionId) AS VersionId FROM [Version] WHERE ClinicalCaseId = @ClinicalCaseId    
  AND StatusID NOT IN (18, 19)    
 ) AS v ON v.VersionId = wa.VersionId          
 JOIN WorkItemProvider wp ON wp.ClinicalCaseId = wa.ClinicalCaseId AND ISNULL(wp.ClaimId, 0) = ISNULL(wa.ClaimId, 0)          
    WHERE wp.ClinicalCaseId = @ClinicalCaseId                           
    )                            
    , ShadowQAVersionCTE                          
   AS (                            
    SELECT v.ClinicalCaseId                            
     ,v.VersionId                            
     ,v.VersionDate                            
     ,v.StatusId                       ,ROW_NUMBER() OVER (                            
   PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC                            
      ) AS rn                            
    FROM [Version] v                            
    JOIN WorkItem wi ON v.ClinicalCaseId = wi.ClinicalCaseId                            
    WHERE wi.ClinicalCaseId = @ClinicalCaseId                            
     AND wi.StatusId = 13                            
     AND v.StatusId = 11                        
    )                            
    ,QAVersionCTE                            
   AS (                            
    SELECT v.ClinicalCaseId                            
     ,v.VersionId                            
     ,v.VersionDate                            
     ,v.StatusId                            
     ,ROW_NUMBER() OVER (                            
      PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC                            
      ) AS rn                            
    FROM [Version] v                
    JOIN WorkItem wi ON v.ClinicalCaseId = wi.ClinicalCaseId                            
    WHERE wi.ClinicalCaseId = @ClinicalCaseId                            
     AND wi.StatusId = 13    
  AND v.StatusId = 13    
   --  AND v.StatusId IN (    
   --   10    
   --,11    
   --   )                            
    )                            
    ,CoderVersionCTE                            
   AS (                            
    SELECT v.ClinicalCaseId                            
     ,v.VersionId                            
     ,v.VersionDate                            
     ,v.StatusId                            
     ,ROW_NUMBER() OVER (                            
      PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC                            
      ) AS rn                            
    FROM [Version] v                            
    JOIN WorkItem wi ON v.ClinicalCaseId = wi.ClinicalCaseId                            
    WHERE wi.ClinicalCaseId = @ClinicalCaseId    
     AND wi.StatusId = 13                            
     AND v.StatusId = 3                            
    )                            
    ,QARebuttedVersionCTE                            
   AS (                            
    SELECT v.ClinicalCaseId                            
     ,v.VersionId                            
     ,v.VersionDate                            
     ,v.StatusId                            
     ,ROW_NUMBER() OVER (                            
 PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC                            
      ) AS rn                            
    FROM [Version] v                            
    JOIN WorkItem wi ON v.ClinicalCaseId = wi.ClinicalCaseId                            
    WHERE wi.ClinicalCaseId = @ClinicalCaseId    
     AND wi.StatusId = 13                            
     AND v.StatusId = 13                            
    )                            
    ,QARemarksCTE                            
   AS (                            
    SELECT wa.[ClinicalCaseId]                            
     ,wa.ClaimId        
  ,wa.VersionId        
     ,MAX(CASE                             
       WHEN FieldName = 'PayorId'                            
        THEN FieldValue                            
       END) AS QAPayorId                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderID'                            
 THEN FieldValue                            
       END) AS QAProviderID                            
     ,MAX(CASE                             
       WHEN FieldName = 'CPTCode'                            
        THEN FieldValue                            
       END) AS QACPTCode                            
     ,MAX(CASE                             
       WHEN FieldName = 'Mod'                            
        THEN FieldValue                            
       END) AS QAMod                            
     ,MAX(CASE                          
       WHEN FieldName = 'Dx'                            
        THEN FieldValue                            
       END) AS QADx                        
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderFeedbackID'                            
        THEN FieldValue                            
       END) AS QAProviderFeedbackID                            
     ,MAX(CASE                             
       WHEN FieldName = 'PayorId'                            
        THEN Remark                            
       END) AS QAPayorIdRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderID'                            
        THEN Remark                            
       END) AS QAProviderIDRemark                         
     ,MAX(CASE                             
       WHEN FieldName = 'CPTCode'    
        THEN Remark                          
       END) AS QACPTCodeRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'Mod'                            
        THEN Remark                            
       END) AS QAModRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'Dx'    
        THEN Remark                            
       END) AS QADxRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderFeedbackID'                            
        THEN Remark                            
       END) AS QAProviderFeedbackIDRemark                        
     ,wa.ErrorTypeId AS QAErrorTypeId                      
    FROM [dbo].[WorkItemAudit] wa                            
    JOIN QAVersionCTE qv ON qv.ClinicalCaseId = wa.ClinicalCaseId                            
     AND wa.VersionId = qv.VersionId        
  WHERE qv.rn = 1 AND wa.IsAccepted = 1    
    GROUP BY wa.ClinicalCaseId                            
      ,wa.ClaimId                        
   ,wa.ErrorTypeId        
   ,wa.VersionId        
    )     
 ,QARejectedRemarksCTE                            
   AS (                            
    SELECT wa.[ClinicalCaseId]                            
     ,wa.ClaimId        
  ,wa.VersionId        
     ,MAX(CASE                             
       WHEN FieldName = 'PayorId'                            
        THEN FieldValue                            
       END) AS QAPayorId                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderID'                            
 THEN FieldValue                            
       END) AS QAProviderID                            
     ,MAX(CASE                             
       WHEN FieldName = 'CPTCode'                            
        THEN FieldValue                            
       END) AS QACPTCode                            
     ,MAX(CASE                             
       WHEN FieldName = 'Mod'                            
        THEN FieldValue                            
       END) AS QAMod                            
     ,MAX(CASE                             
       WHEN FieldName = 'Dx'                            
        THEN FieldValue                            
       END) AS QADx                        
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderFeedbackID'                            
        THEN FieldValue                            
       END) AS QAProviderFeedbackID                            
     ,MAX(CASE                             
       WHEN FieldName = 'PayorId'                            
        THEN Remark                            
       END) AS QAPayorIdRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderID'                            
        THEN Remark                            
       END) AS QAProviderIDRemark                         
     ,MAX(CASE                             
       WHEN FieldName = 'CPTCode'    
        THEN Remark                          
       END) AS QACPTCodeRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'Mod'                            
        THEN Remark                            
       END) AS QAModRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'Dx'    
        THEN Remark                            
       END) AS QADxRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderFeedbackID'                            
        THEN Remark                            
       END) AS QAProviderFeedbackIDRemark                        
     ,wa.ErrorTypeId AS QAErrorTypeId    
    FROM [dbo].[WorkItemAudit] wa                            
    JOIN QAVersionCTE qv ON qv.ClinicalCaseId = wa.ClinicalCaseId                            
     AND wa.VersionId = qv.VersionId        
  WHERE qv.rn = 1 AND wa.IsAccepted = 0    
    GROUP BY wa.ClinicalCaseId                            
      ,wa.ClaimId                        
   ,wa.ErrorTypeId        
   ,wa.VersionId    
    )  
 ,ShadowQAAcceptedCTE                            
   AS (                            
    SELECT wa.[ClinicalCaseId]                            
     ,wa.ClaimId        
  ,wa.VersionId        
     ,MAX(CASE                             
       WHEN FieldName = 'PayorId'                            
        THEN FieldValue                            
       END) AS ShadowQAPayorId                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderID'                            
        THEN FieldValue                            
       END) AS ShadowQAProviderID                            
     ,MAX(CASE                             
       WHEN FieldName = 'CPTCode'                            
        THEN FieldValue                            
       END) AS ShadowQACPTCode                            
     ,MAX(CASE                             
       WHEN FieldName = 'Mod'                            
        THEN FieldValue                            
       END) AS ShadowQAMod                            
     ,MAX(CASE                             
       WHEN FieldName = 'Dx'                            
        THEN FieldValue                            
       END) AS ShadowQADx                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderFeedbackID'                            
        THEN FieldValue                            
       END) AS ShadowQAProviderFeedbackID                            
     ,MAX(CASE                             
       WHEN FieldName = 'PayorId'                            
        THEN Remark                            
       END) AS ShadowQAPayorIdRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderID'                            
        THEN Remark                            
       END) AS ShadowQAProviderIDRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'CPTCode'                            
        THEN Remark                            
       END) AS ShadowQACPTCodeRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'Mod'                            
        THEN Remark                            
       END) AS ShadowQAModRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'Dx'                            
        THEN Remark                            
       END) AS ShadowQADxRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderFeedbackID'                            
        THEN Remark                            
       END) AS ShadowQAProviderFeedbackIDRemark                        
    ,wa.ErrorTypeId AS ShadowQAErrorTypeId        
 ,wa.IsAccepted        
    FROM [dbo].[WorkItemAudit] wa                            
    JOIN ShadowQAVersionCTE qv ON qv.ClinicalCaseId = wa.ClinicalCaseId                            
     AND wa.VersionId = qv.VersionId        
  WHERE qv.rn = 1 AND wa.IsAccepted = 1  
 GROUP BY wa.ClinicalCaseId                            
     ,wa.ClaimId                        
 ,wa.ErrorTypeId                    
 ,wa.VersionId        
 ,wa.IsAccepted        
   )  
    ,ShadowQARejectedCTE                            
   AS (                            
    SELECT wa.[ClinicalCaseId]                            
     ,wa.ClaimId        
  ,wa.VersionId        
     ,MAX(CASE                             
       WHEN FieldName = 'PayorId'                            
        THEN FieldValue                            
       END) AS ShadowQAPayorId                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderID'                            
        THEN FieldValue                            
       END) AS ShadowQAProviderID                            
     ,MAX(CASE                             
       WHEN FieldName = 'CPTCode'                            
        THEN FieldValue                            
       END) AS ShadowQACPTCode                            
     ,MAX(CASE                             
       WHEN FieldName = 'Mod'                            
        THEN FieldValue                            
       END) AS ShadowQAMod                            
     ,MAX(CASE                             
       WHEN FieldName = 'Dx'                            
        THEN FieldValue                            
       END) AS ShadowQADx                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderFeedbackID'                            
        THEN FieldValue                            
       END) AS ShadowQAProviderFeedbackID                            
     ,MAX(CASE                             
       WHEN FieldName = 'PayorId'                            
        THEN Remark                            
       END) AS ShadowQAPayorIdRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderID'                            
        THEN Remark                            
       END) AS ShadowQAProviderIDRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'CPTCode'                            
        THEN Remark                            
       END) AS ShadowQACPTCodeRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'Mod'                            
        THEN Remark                            
       END) AS ShadowQAModRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'Dx'                            
        THEN Remark                            
       END) AS ShadowQADxRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderFeedbackID'                            
        THEN Remark                            
       END) AS ShadowQAProviderFeedbackIDRemark                        
    ,wa.ErrorTypeId AS ShadowQAErrorTypeId        
 ,wa.IsAccepted        
    FROM [dbo].[WorkItemAudit] wa                            
    JOIN ShadowQAVersionCTE qv ON qv.ClinicalCaseId = wa.ClinicalCaseId                            
     AND wa.VersionId = qv.VersionId        
  WHERE qv.rn = 1 AND wa.IsAccepted = 0  
 GROUP BY wa.ClinicalCaseId                            
     ,wa.ClaimId                        
 ,wa.ErrorTypeId                        
 ,wa.VersionId        
 ,wa.IsAccepted        
   )                            
    ,QARebuttedCTE                            
   AS (                            
    SELECT wa.[ClinicalCaseId]                            
     ,wa.ClaimId                            
     ,MAX(CASE                             
       WHEN FieldName = 'PayorId'                            
        THEN FieldValue                            
       END) AS QARebuttedPayorId                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderID'                            
        THEN FieldValue                       
       END) AS QARebuttedProviderID                            
     ,MAX(CASE                             
       WHEN FieldName = 'CPTCode'                            
        THEN FieldValue                            
       END) AS QARebuttedCPTCode                            
     ,MAX(CASE                             
       WHEN FieldName = 'Mod'                            
        THEN FieldValue                            
       END) AS QARebuttedMod                            
     ,MAX(CASE                             
       WHEN FieldName = 'Dx'                            
        THEN FieldValue                            
      END) AS QARebuttedDx                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderFeedbackID'                            
        THEN FieldValue                            
       END) AS QARebuttedProviderFeedbackID                            
     ,MAX(CASE                             
       WHEN FieldName = 'PayorId'                            
    THEN Remark                            
       END) AS QARebuttedPayorIdRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderID'                            
        THEN Remark                            
       END) AS QARebuttedProviderIDRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'CPTCode'                  
        THEN Remark                            
       END) AS QARebuttedCPTCodeRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'Mod'                            
        THEN Remark                            
       END) AS QARebuttedModRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'Dx'                            
        THEN Remark                            
       END) AS QARebuttedDxRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderFeedbackID'                            
        THEN Remark                            
       END) AS QARebuttedProviderFeedbackIDRemark                            
    FROM [dbo].[WorkItemAudit] wa                            
    JOIN QARebuttedVersionCTE rv ON rv.ClinicalCaseId = wa.ClinicalCaseId                            
     AND wa.VersionId = rv.VersionId        
  WHERE rv.rn = 1        
    GROUP BY wa.ClinicalCaseId                            
     ,wa.ClaimId                            
    )                            
   SELECT DISTINCT cc.ClinicalCaseID                            
    ,PatientMRN                            
    ,PatientFirstName + ' ' + PatientLastName AS [Name]                            
    ,CAST(DateOfService AS VARCHAR(10)) AS DateOfService                            
    ,isnull(CC.ProviderId, 0)                            
    ,dbo.fn_GetUserName(AssignedTo) AS CodedBy                            
    ,dbo.fn_GetUserName(QABy) AS QABy                            
    ,dbo.fn_GetUserName(ShadowQABy) AS ShadowQABy                            
    ,c.PayorId                            
    ,c.ProviderId                            
    ,c.ProviderFeedbackId                            
    ,wi.NoteTitle                            
    ,dx.DxCode                            
    ,cpt.CPTCode                            
    ,ISNULL(qr.QAPayorId, qrr.QAPayorId) AS QAPayorId                            
    ,ISNULL(qr.QAProviderID, qrr.QAProviderID) AS QAProviderID                            
    --,cptQA.CptCode AS QACPTCode                            
 ,ISNULL(qr.QACPTCode, '') + '|' + ISNULL(qrr.QACPTCode, '') AS QACPTCode    
    ,NULL AS QAMod                            
    --,dxQA.DxCode AS QADx                            
 ,ISNULL(qr.QADx, '') + '|' + ISNULL(qrr.QADx, '') AS QADx    
    ,ISNULL(qr.QAProviderFeedbackID, qrr.QAProviderFeedbackID) AS QAProviderFeedbackID                            
    ,ISNULL(qr.QAPayorIdRemark, qrr.QAPayorIdRemark) AS QAPayorIdRemark                    
    ,ISNULL(qr.QAProviderIDRemark, qrr.QAProviderIDRemark) AS QAProviderIDRemark                            
    ,ISNULL(qr.QACPTCodeRemark, '') + '|' + ISNULL(qrr.QACPTCodeRemark, '') AS QACPTCodeRemark                            
    ,NULL AS QAModRemark                            
    ,ISNULL(qr.QADxRemark, '') + '|' + ISNULL(qrr.QADxRemark, '') AS QADxRemark                            
    ,ISNULL(qr.QAProviderFeedbackIDRemark, qrr.QAProviderFeedbackIDRemark) AS QAProviderFeedbackIDRemark    
    ,ISNULL(sqaa.ShadowQAPayorId, sqar.ShadowQAPayorId) AS ShadowQAPayorId                            
    ,ISNULL(sqaa.ShadowQAProviderID, sqar.ShadowQAProviderID) AS ShadowQAProviderID                            
    ,ISNULL(sqaa.ShadowQACPTCode, '') + '|' + ISNULL(sqar.ShadowQACPTCode, '') AS ShadowQACPTCode  
    ,NULL AS ShadowQAMod  
    ,ISNULL(sqaa.ShadowQADx, '') + '|' + ISNULL(sqar.ShadowQADx, '') AS ShadowQADx  
    ,ISNULL(sqaa.ShadowQAProviderFeedbackID, sqar.ShadowQAProviderFeedbackID) AS ShadowQAProviderFeedbackID  
    ,ISNULL(sqaa.ShadowQAPayorIdRemark, sqar.ShadowQAPayorIdRemark) AS ShadowQAPayorIdRemark  
    ,ISNULL(sqaa.ShadowQAProviderIDRemark, sqar.ShadowQAProviderIDRemark) AS ShadowQAProviderIDRemark  
    ,ISNULL(sqaa.ShadowQACPTCodeRemark, '') + '|' + ISNULL(sqar.ShadowQACPTCodeRemark, '') AS ShadowQACPTCodeRemark                            
    ,NULL AS ShadowQAModRemark                            
    ,ISNULL(sqaa.ShadowQADxRemark, '') + '|' + ISNULL(sqar.ShadowQADxRemark, '') AS ShadowQADxRemark                            
    ,ISNULL(sqaa.ShadowQAProviderFeedbackIDRemark, sqar.ShadowQAProviderFeedbackIDRemark) AS ShadowQAProviderFeedbackIDRemark                            
    ,QARebuttedPayorId                            
    ,QARebuttedProviderID                            
    ,QARebuttedCPTCode            
    ,QARebuttedMod                            
    ,QARebuttedDx                            
    ,QARebuttedProviderFeedbackID                            
    ,QARebuttedPayorIdRemark                            
    ,QARebuttedProviderIDRemark                            
    ,QARebuttedCPTCodeRemark                            
    ,QARebuttedModRemark                            
    ,QARebuttedDxRemark                            
    ,QARebuttedProviderFeedbackIDRemark                            
    ,Pr.[Name] AS ProviderText                            
    ,P.[Name] AS PayorText                            
    ,Pf.Feedback AS ProviderFeedbackText                            
    ,ISNULL(QAP.[Name], QAP1.[Name]) AS QAPayorText  
    ,ISNULL(QAPr.[Name], QAPr1.[Name]) AS QAProviderText  
 ,ISNULL(QPf.Feedback, QPfr.Feedback) AS QAProviderFeedbackText  
 ,ISNULL(SQAPa.[Name], SQAPr.[Name]) AS ShadowQAPayorText  
    ,ISNULL(SQAPra.[Name], SQAPrr.[Name]) AS ShadowQAProviderText  
 ,ISNULL(SQAPfa.Feedback, SQAPfr.Feedback) AS ShadowQAProviderFeedbackText  
    ,l.[Name] AS ListName                            
    ,c.ClaimId                            
    ,c.ClaimOrder                        
   ,ISNULL(qr.QAErrorTypeId, qrr.QAErrorTypeId) AS QAErrorTypeId                      
   ,ISNULL(sqaa.ShadowQAErrorTypeId, sqar.ShadowQAErrorTypeId) AS ShadowQAErrorTypeId                      
   FROM ClaimCTE c                            
   JOIN ClinicalCase cc ON cc.ClinicalCaseId = c.ClinicalCaseId                          
   INNER JOIN WorkItem wi ON wi.ClinicalCaseId = cc.ClinicalCaseId    
   LEFT JOIN ShadowQAVersionCTE qv ON qv.ClinicalCaseId = cc.ClinicalCaseId                            
   LEFT JOIN CoderVersionCTE cv ON cv.ClinicalCaseId = cc.ClinicalCaseId        
   LEFT JOIN QARemarksCTE qr ON qr.ClinicalCaseId = wi.ClinicalCaseId AND ISNULL(qr.ClaimId, 0) = ISNULL(c.ClaimId, 0)        
   LEFT JOIN QARejectedRemarksCTE qrr ON qrr.ClinicalCaseId = wi.ClinicalCaseId AND ISNULL(qrr.ClaimId, 0) = ISNULL(c.ClaimId, 0)        
   LEFT JOIN (                            
    SELECT DISTINCT dx.ClinicalCaseId                                 ,dx.VersionId                            
     ,dx.ClaimId                            
     ,SUBSTRING((                            
       SELECT ',' + dxc.DxCode AS [text()]                            
       FROM dbo.DxCode dxc                            
       WHERE dx.ClinicalCaseId = dxc.ClinicalCaseId                            
        AND dxc.VersionId = dx.VersionId                            
        AND ISNULL(dx.ClaimId, 0) = ISNULL(dxc.ClaimId, 0)                            
       ORDER BY dxc.ClinicalCaseId                            
       FOR XML PATH('')                            
       ), 2, 1000) [DxCode]                            
    FROM dbo.DxCode dx                            
    WHERE ClinicalCaseId = @ClinicalCaseId                            
    ) dx ON dx.ClinicalCaseId = c.ClinicalCaseId                            
    AND dx.VersionID = cv.VersionID                            
    AND ISNULL(dx.ClaimId, 0) = ISNULL(c.ClaimId, 0)                            
   LEFT JOIN (                            
    SELECT DISTINCT cp.ClinicalCaseId                            
     ,cp.VersionId                            
     ,cp.ClaimId                            
     ,SUBSTRING((                            
       SELECT '|' + cptc.cptCode + '^' + isnull(cptc.Modifier, '') + '^' + ISNULL(cptc.qty, '') + '^' + isnull(cptc.Links, '') AS [text()]                            
       FROM dbo.cptcode cptc          
       WHERE cp.ClinicalCaseId = cptc.ClinicalCaseId                            
        AND cp.VersionID = cptc.VersionID                            
        AND ISNULL(cp.ClaimId, 0) = ISNULL(cptc.ClaimId, 0)                            
       ORDER BY cptc.ClinicalCaseId                            
       FOR XML PATH('')                            
       ), 2, 1000) [CptCode]                            
    FROM dbo.cptcode cp                            
    WHERE ClinicalCaseId = @ClinicalCaseId                            
    ) cpt ON cc.ClinicalCaseId = cpt.ClinicalCaseId                            
    AND cpt.VersionId = cv.VersionId                            
    AND ISNULL(cpt.ClaimId, 0) = ISNULL(c.ClaimId, 0)    
   LEFT JOIN ShadowQAAcceptedCTE sqaa ON sqaa.ClinicalCaseId = cc.ClinicalCaseId AND ISNULL(sqaa.ClaimId, 0) = ISNULL(c.ClaimId, 0)  
   LEFT JOIN ShadowQARejectedCTE sqar ON sqar.ClinicalCaseId = cc.ClinicalCaseId AND ISNULL(sqar.ClaimId, 0) = ISNULL(c.ClaimId, 0)  
   LEFT JOIN QARebuttedCTE r ON r.ClinicalCaseId = cc.ClinicalCaseId AND ISNULL(r.ClaimId, 0) = ISNULL(c.ClaimId, 0)                          
   LEFT JOIN Payor QAP ON QAP.PayorId = qr.QAPayorId  
   LEFT JOIN [Provider] QAPr ON QAPr.ProviderID = qr.QAProviderID    
   LEFT JOIN Payor QAP1 ON QAP1.PayorId = qrr.QAPayorId                            
   LEFT JOIN [Provider] QAPr1 ON QAPr1.ProviderID = qrr.QAProviderID  
   LEFT JOIN ProviderFeedback QAPf ON QAPf.ProviderFeedbackId = qrr.QAProviderFeedbackID                            
   LEFT JOIN Payor P ON P.PayorId = c.PayorId                            
   LEFT JOIN [Provider] Pr ON Pr.ProviderID = c.ProviderID                            
   LEFT JOIN ProviderFeedback Pf ON Pf.ProviderFeedbackId = c.ProviderFeedbackID  
   LEFT JOIN ProviderFeedback QPf ON QPf.ProviderFeedbackId = qr.QAProviderFeedbackID  
   LEFT JOIN ProviderFeedback QPfr ON QPfr.ProviderFeedbackId = qrr.QAProviderFeedbackID  
  
   LEFT JOIN Payor SQAPa ON SQAPa.PayorId = sqaa.ShadowQAPayorId  
   LEFT JOIN [Provider] SQAPra ON SQAPra.ProviderID = sqaa.ShadowQAProviderID  
   LEFT JOIN ProviderFeedback SQAPfa ON SQAPfa.ProviderFeedbackId = sqaa.ShadowQAProviderFeedbackID  
  
   LEFT JOIN Payor SQAPr ON SQAPr.PayorId = sqar.ShadowQAPayorId  
   LEFT JOIN [Provider] SQAPrr ON SQAPrr.ProviderID = sqar.ShadowQAProviderID  
   LEFT JOIN ProviderFeedback SQAPfr ON SQAPfr.ProviderFeedbackId = sqar.ShadowQAProviderFeedbackID  
  
   LEFT JOIN List l ON l.ListId = CC.ListId                            
   WHERE wi.ClinicalCaseId = @ClinicalCaseId                          
   ORDER BY c.ClaimOrder                             
  END                            
  ELSE IF @Role = 'ShadowQA'                            
   AND @ChartType = 'Available'                            
  BEGIN                            
SELECT DISTINCT TOP 1 @ClinicalCaseId =                             
 wi.ClinicalCaseId                             
 FROM WorkItem wi                            
 JOIN ProjectUser pu                            
 ON pu.ProjectId = wi.ProjectId                            
 WHERE wi.ProjectId = @ProjectID                            
    AND (                            
     (                            
      wi.StatusId = 9                            
      AND wi.ShadowQABy = @UserId                            
      AND ISNULL(wi.IsBlocked, 0) = 0                            
      )                            
     OR wi.StatusId = 8                            
     )                            
    AND pu.UserId = @UserId                            
    AND pu.RoleId = 3                            
    AND pu.IsActive = 1                           
 ORDER BY wi.ClinicalCaseId                             
                            
;WITH ClaimCTE AS                               
 (                              
 SELECT DISTINCT wp.WorkItemProviderId, wp.ClinicalCaseId, wp.VersionId, wp.ProviderId, wp.PayorId                            
 ,  wp.ProviderFeedbackId, wp.ClaimId                              
 , p.[Name] AS ProviderText                            
 , pa.[Name] AS PayorText                            
 , pf.Feedback AS ProviderFeedbackText                          
 , DENSE_RANK() OVER(PARTITION BY wp.ClinicalCaseId ORDER BY ISNULL(wp.ClaimId, 0) ) AS ClaimOrder                              
 FROM WorkItemProvider wp  LEFT JOIN Claim c  ON wp.ClinicalCaseId = c.ClinicalCaseId  AND wp.VersionId = c.VersionId                          
 LEFT JOIN Provider p ON p.ProviderId = wp.ProviderId                            
   LEFT JOIN Payor pa ON pa.PayorId = wp.PayorId                          
   LEFT JOIN ProviderFeedback pf ON pf.ProviderFeedbackId = wp.ProviderFeedbackId                          
 WHERE wp.ClinicalCaseId = @ClinicalCaseId                         
 )                            
 ,    QAVersionCTE                            
   AS (                            
    SELECT v.ClinicalCaseId                            
     ,v.VersionId                            
     ,v.VersionDate                            
     ,v.StatusId                            
     ,ROW_NUMBER() OVER (                            
      PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC                            
      ) AS rn                            
    FROM [Version] v                            
    JOIN WorkItem wi ON v.ClinicalCaseId = wi.ClinicalCaseId                            
    WHERE wi.ClinicalCaseId = @ClinicalCaseId    
     AND wi.StatusId IN (8,9)    
     AND v.StatusId IN (6, 7)                            
    )                            
    ,CoderVersionCTE                            
   AS (                            
    SELECT v.ClinicalCaseId                            
     ,v.VersionId                            
     ,v.VersionDate                            
     ,v.StatusId                            
     ,ROW_NUMBER() OVER (                            
      PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC                            
      ) AS rn                            
    FROM [Version] v                            
    JOIN WorkItem wi ON v.ClinicalCaseId = wi.ClinicalCaseId                            
    WHERE wi.ClinicalCaseId = @ClinicalCaseId    
     AND wi.StatusId IN (8, 9)    
     AND v.StatusId IN (3, 12)                            
    )  
 ,QAAcceptedRemarksCTE                            
   AS (                            
    SELECT wa.[ClinicalCaseId]                            
     ,wa.ClaimId                          
     ,MAX(CASE                             
       WHEN FieldName = 'PayorId'                            
        THEN FieldValue                            
       END) AS QAPayorId                            
     ,MAX(CASE WHEN FieldName = 'ProviderID'                            
        THEN FieldValue                            
       END) AS QAProviderID                            
     ,MAX(CASE                             
       WHEN FieldName = 'CPTCode'                            
        THEN FieldValue                            
       END) AS QACPTCode    
     ,MAX(CASE                             
       WHEN FieldName = 'Dx'                            
        THEN FieldValue                            
       END) AS QADx                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderFeedbackID'                            
        THEN FieldValue                            
       END) AS QAProviderFeedbackID                            
     ,MAX(CASE                             
       WHEN FieldName = 'PayorId'                            
        THEN Remark                            
       END) AS QAPayorIdRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderID'                            
        THEN Remark                            
       END) AS QAProviderIDRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'CPTCode'                            
        THEN Remark                            
       END) AS QACPTCodeRemark                       
     ,MAX(CASE                             
       WHEN FieldName = 'Dx'                            
        THEN Remark                            
       END) AS QADxRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderFeedbackID'                            
        THEN Remark                            
       END) AS QAProviderFeedbackIDRemark                        
    ,wa.ErrorTypeId AS QAErrorTypeId                      
    FROM [dbo].[WorkItemAudit] wa                            
    JOIN QAVersionCTE qv ON qv.ClinicalCaseId = wa.ClinicalCaseId                            
     AND wa.VersionId = qv.VersionId    
 WHERE wa.IsAccepted = 1  
    GROUP BY wa.ClinicalCaseId                            
     ,wa.ClaimId                        
     ,wa.ErrorTypeId                        
    )  
    ,QARejectedRemarksCTE                            
   AS (                            
    SELECT wa.[ClinicalCaseId]                            
     ,wa.ClaimId                          
     ,MAX(CASE                             
       WHEN FieldName = 'PayorId'                            
        THEN FieldValue                            
       END) AS QAPayorId                            
     ,MAX(CASE WHEN FieldName = 'ProviderID'                            
        THEN FieldValue                            
       END) AS QAProviderID                            
     ,MAX(CASE                             
       WHEN FieldName = 'CPTCode'                            
        THEN FieldValue                            
       END) AS QACPTCode    
     ,MAX(CASE                             
       WHEN FieldName = 'Dx'                            
        THEN FieldValue                            
       END) AS QADx                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderFeedbackID'                            
        THEN FieldValue                            
       END) AS QAProviderFeedbackID                            
     ,MAX(CASE                             
       WHEN FieldName = 'PayorId'                            
        THEN Remark                            
       END) AS QAPayorIdRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderID'                            
        THEN Remark                            
       END) AS QAProviderIDRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'CPTCode'                            
        THEN Remark                            
       END) AS QACPTCodeRemark                       
     ,MAX(CASE                             
       WHEN FieldName = 'Dx'                            
        THEN Remark                            
       END) AS QADxRemark                            
     ,MAX(CASE                             
       WHEN FieldName = 'ProviderFeedbackID'                            
        THEN Remark                            
       END) AS QAProviderFeedbackIDRemark                        
    ,wa.ErrorTypeId AS QAErrorTypeId                      
    FROM [dbo].[WorkItemAudit] wa                            
    JOIN QAVersionCTE qv ON qv.ClinicalCaseId = wa.ClinicalCaseId                            
     AND wa.VersionId = qv.VersionId    
 WHERE wa.IsAccepted = 0    
    GROUP BY wa.ClinicalCaseId                            
     ,wa.ClaimId                        
     ,wa.ErrorTypeId                        
    )                            
   SELECT DISTINCT cc.ClinicalCaseID                   
    ,PatientMRN                            
    ,PatientFirstName + ' ' + PatientLastName AS [Name]                            
    ,CAST(DateOfService AS VARCHAR(10)) AS DateOfService                            
    ,dbo.fn_GetUserName(AssignedTo) AS CodedBy                            
    ,dbo.fn_GetUserName(QABy) AS QABy                            
    ,dbo.fn_GetUserName(ShadowQABy) AS ShadowQABy                            
    ,ProviderText                            
    ,PayorText                            
    ,ProviderFeedbackText                            
    ,wi.NoteTitle                            
    ,dx.DxCode                            
    ,cpt.CPTCode  
 ,ISNULL(qar.QAPayorId, qrr.QAPayorId) AS QAPayorId                            
 ,ISNULL(qar.QAProviderID, qrr.QAProviderID) AS QAProviderID                            
 --,cptQA.CptCode AS QACPTCode                            
 ,ISNULL(qar.QACPTCode, '') + '|' + ISNULL(qrr.QACPTCode, '') AS QACPTCode    
 ,NULL AS QAMod                            
 --,dxQA.DxCode AS QADx                            
 ,ISNULL(qar.QADx, '') + '|' + ISNULL(qrr.QADx, '') AS QADx    
 ,ISNULL(qar.QAProviderFeedbackID, qrr.QAProviderFeedbackID) AS QAProviderFeedbackID                            
 ,ISNULL(qar.QAPayorIdRemark, qrr.QAPayorIdRemark) AS QAPayorIdRemark                            
 ,ISNULL(qar.QAProviderIDRemark, qrr.QAProviderIDRemark) AS QAProviderIDRemark                            
 ,ISNULL(qar.QACPTCodeRemark, '') + '|' + ISNULL(qrr.QACPTCodeRemark, '') AS QACPTCodeRemark                            
 ,NULL AS QAModRemark                            
 ,ISNULL(qar.QADxRemark, '') + '|' + ISNULL(qrr.QADxRemark, '') AS QADxRemark                            
 ,ISNULL(qar.QAProviderFeedbackIDRemark, qrr.QAProviderFeedbackIDRemark) AS QAProviderFeedbackIDRemark   
 ,ISNULL(QAP.[Name], QAP1.[Name]) AS QAPayorText  
 ,ISNULL(QAPr.[Name], QAPr1.[Name]) AS QAProviderText  
 ,ISNULL(QAPf.Feedback, QAPf1.Feedback) AS QAProviderFeedbackText   
 ,ISNULL(qar.QAErrorTypeId, qrr.QAErrorTypeId) AS QAErrorTypeId                          
    ,l.[Name] AS ListName                            
    ,c.ClaimId                            
    ,c.ClaimOrder  
   INTO #ShadowQAAvailable                            
   FROM ClaimCTE c                            
   JOIN ClinicalCase cc ON cc.ClinicalCaseId = c.ClinicalCaseId                          
   INNER JOIN WorkItem wi ON wi.ClinicalCaseId = cc.ClinicalCaseId         --LEFT JOIN WorkItemProvider wp ON wp.ClinicalCaseId = CC.ClinicalCaseId                  
   LEFT JOIN QAVersionCTE qv ON qv.ClinicalCaseId = cc.ClinicalCaseId                            
   LEFT JOIN CoderVersionCTE cv ON cv.ClinicalCaseId = cc.ClinicalCaseId                            
   LEFT JOIN (                            
    SELECT DISTINCT dx.ClinicalCaseId                            
     ,dx.VersionId                            
     ,dx.ClaimId                            
     ,SUBSTRING((                            
       SELECT ',' + dxc.DxCode AS [text()]                            
       FROM dbo.DxCode dxc                            
       WHERE dx.ClinicalCaseId = dxc.ClinicalCaseId                            
        AND dxc.VersionId = dx.VersionId                            
        AND ISNULL(dx.ClaimId, 0) = ISNULL(dxc.ClaimId, 0)                            
       ORDER BY dxc.ClinicalCaseId                            
       FOR XML PATH('')                            
       ), 2, 1000) [DxCode]                            
    FROM dbo.DxCode dx                            
    WHERE ClinicalCaseId = @ClinicalCaseId                            
    ) dx ON dx.ClinicalCaseId = c.ClinicalCaseId                            
    AND dx.VersionID = cv.VersionID                            
    AND ISNULL(dx.ClaimId, 0) = ISNULL(c.ClaimId, 0)                            
   LEFT JOIN (                            
    SELECT DISTINCT cp.ClinicalCaseId                            
     ,cp.VersionId                            
     ,cp.ClaimId                            
     ,SUBSTRING((                            
       SELECT '|' + cptc.cptCode + '^' + isnull(cptc.Modifier, '') + '^' + ISNULL(cptc.qty, '') + '^' + isnull(cptc.Links, '') AS [text()]                            
       FROM dbo.cptcode cptc                            
       WHERE cp.ClinicalCaseId = cptc.ClinicalCaseId                            
        AND cp.VersionID = cptc.VersionID                            
        AND ISNULL(cp.ClaimId, 0) = ISNULL(cptc.ClaimId, 0)                            
       ORDER BY cptc.ClinicalCaseId                            
       FOR XML PATH('')                            
       ), 2, 1000) [CptCode]                            
    FROM dbo.cptcode cp                            
    WHERE ClinicalCaseId = @ClinicalCaseId                            
    ) cpt ON cc.ClinicalCaseId = cpt.ClinicalCaseId                            
    AND cpt.VersionId = cv.VersionId                            
    AND ISNULL(cpt.ClaimId, 0) = ISNULL(c.ClaimId, 0)                          
   LEFT JOIN QAAcceptedRemarksCTE qar ON qar.ClinicalCaseId = c.ClinicalCaseId AND ISNULL(qar.ClaimId, 0) = ISNULL(c.ClaimId, 0)                                                           
   LEFT JOIN Payor QAP ON QAP.PayorId = qar.QAPayorId                            
   LEFT JOIN [Provider] QAPr ON QAPr.ProviderID = qar.QAProviderID                            
   LEFT JOIN ProviderFeedback QAPf ON QAPf.ProviderFeedbackId = qar.QAProviderFeedbackID  
   LEFT JOIN QARejectedRemarksCTE qrr ON qrr.ClinicalCaseId = c.ClinicalCaseId AND ISNULL(qrr.ClaimId, 0) = ISNULL(c.ClaimId, 0)                                                           
   LEFT JOIN Payor QAP1 ON QAP1.PayorId = qrr.QAPayorId                            
   LEFT JOIN [Provider] QAPr1 ON QAPr1.ProviderID = qrr.QAProviderID                            
   LEFT JOIN ProviderFeedback QAPf1 ON QAPf1.ProviderFeedbackId = qrr.QAProviderFeedbackID  
   LEFT JOIN List l ON l.ListId = CC.ListId                            
   WHERE wi.ClinicalCaseId = @ClinicalCaseId                           
   ORDER BY c.ClaimOrder                            
                            
   UPDATE WorkItem                            
   SET StatusId = 9                            
    ,ShadowQABy = @UserId                            
    ,ShadowQADate = GETUTCDATE()                            
   WHERE ClinicalCaseId = @ClinicalCaseId                           
                            
   --IF NOT EXISTS (                            
   --  SELECT 1                            
   --  FROM Version                            
   --  WHERE UserID = @UserId           
   --   AND StatusId = 9                            
   --  )                            
   -- AND EXISTS (                            
   --  SELECT 1                            
   --  FROM #ShadowQAAvailable                            
   --  )                            
   --BEGIN                            
    INSERT INTO Version (                            
     ClinicalcaseID                            
     ,VersionDate                            
     ,userID                            
     ,Statusid                            
     )                            
    SELECT @ClinicalCaseId                          
     ,GETUTCDATE()                            
     ,@UserId                            
     ,9                            
  --END                    
                            
   SELECT *                            
   FROM #ShadowQAAvailable                            
  END                            
  ELSE IF @Role = 'ShadowQA'                            
   AND @ChartType = 'Block'                            
  BEGIN              
              
    SELECT @CCIDs = COALESCE(@CCIDs + ',', '') + cast(W.Clinicalcaseid as varchar(10))                            
 FROM WorkItem W                              
 JOIN ProjectUser pu                                                                    
 ON pu.ProjectId = W.ProjectId                              
 WHERE W.ProjectId = @ProjectID AND (                  
   (                            
    W.StatusId = 9                            
    AND W.ShadowQABy = @UserId                            
    AND ISNULL(W.IsBlocked, 0) = 1                            
    )                            
   )                            
  AND pu.UserId = @UserId                            
  AND pu.RoleId = 3                     
  AND pu.IsActive = 1                        
 ORDER BY W.ClinicalCaseId               
             
IF @PrevOrNextCCID  = 0                  
 SELECT TOP 1  @CurrCCId = value FROM dbo.fnSplit(@CCIDs, ',')                   
ELSE                  
 SELECT @CurrCCId = @PrevOrNextCCID                  
              
  ;WITH ClaimCTE AS                           
 (                          
 SELECT DISTINCT wp.WorkItemProviderId, wp.ClinicalCaseId, wp.VersionId, wp.ProviderId, wp.PayorId,  wp.ProviderFeedbackId, wp.ClaimId                      
 , DENSE_RANK() OVER(PARTITION BY wp.ClinicalCaseId ORDER BY ISNULL(wp.ClaimId, 0) ) AS ClaimOrder                      
 FROM WorkItemProvider wp                      
 LEFT JOIN Claim c  ON wp.ClinicalCaseId = c.ClinicalCaseId  AND wp.VersionId = c.VersionId                      
 WHERE wp.ClinicalCaseId = @CurrCCId                          
 )                    
 ,  QAVersionCTE                        
   AS (                        
    SELECT v.ClinicalCaseId                        
     ,v.VersionId                        
     ,v.VersionDate                        
     ,v.StatusId                        
     ,ROW_NUMBER() OVER (                        
      PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC                        
      ) AS rn                        
    FROM [Version] v                        
    JOIN WorkItem wi ON v.ClinicalCaseId = wi.ClinicalCaseId                        
    WHERE wi.ProjectId = @ProjectID                        
     AND wi.StatusId IN (                        
      8                        
      ,9                        
      )                        
     AND v.StatusId IN (                        
      7                        
      ,8                        
      )                        
    )                        
    ,QARemarksCTE                        
   AS (                        
    SELECT wa.[ClinicalCaseId], wa.ClaimId                        
     ,MAX(CASE                         
       WHEN FieldName = 'PayorId'                        
        THEN FieldValue                        
       END) AS QAPayorId                        
     ,MAX(CASE                         
       WHEN FieldName = 'ProviderID'                        
        THEN FieldValue                        
       END) AS QAProviderID                        
  ,MAX(CASE                         
       WHEN FieldName = 'CPTCode'                        
        THEN FieldValue               
       END) AS QACPTCode                        
     ,MAX(CASE                         
       WHEN FieldName = 'Mod'                        
        THEN FieldValue                        
       END) AS QAMod                        
     ,MAX(CASE                         
       WHEN FieldName = 'Dx'                        
        THEN FieldValue                        
       END) AS QADx                        
     ,MAX(CASE                         
       WHEN FieldName = 'ProviderFeedbackID'                        
        THEN FieldValue                        
       END) AS QAProviderFeedbackID                        
     ,MAX(CASE                         
       WHEN FieldName = 'PayorId'                        
        THEN Remark                        
       END) AS QAPayorIdRemark                        
     ,MAX(CASE                         
       WHEN FieldName = 'ProviderID'                        
        THEN Remark                        
       END) AS QAProviderIDRemark                        
     ,MAX(CASE                  
       WHEN FieldName = 'CPTCode'                        
        THEN Remark                        
       END) AS QACPTCodeRemark                        
     ,MAX(CASE                         
       WHEN FieldName = 'Mod'                        
        THEN Remark                        
       END) AS QAModRemark                        
     ,MAX(CASE                  WHEN FieldName = 'Dx'                        
        THEN Remark                        
       END) AS QADxRemark                        
     ,MAX(CASE                         
       WHEN FieldName = 'ProviderFeedbackID'                        
THEN Remark                        
       END) AS QAProviderFeedbackIDRemark                    
    ,wa.ErrorTypeId AS QAErrorTypeId                  
    FROM [dbo].[WorkItemAudit] wa                        
    JOIN QAVersionCTE qv ON qv.ClinicalCaseId = wa.ClinicalCaseId                        
     AND wa.VersionId = qv.VersionId                        
    GROUP BY wa.ClinicalCaseId, wa.ClaimId, wa.ErrorTypeId                        
    )                        
   SELECT DISTINCT cc.ClinicalCaseID                        
    ,PatientMRN                        
    ,PatientFirstName + ' ' + PatientLastName AS [Name]                        
    ,CAST(DateOfService AS VARCHAR(10)) AS DateOfService                        
    ,dbo.fn_GetUserName(AssignedTo) AS CodedBy                        
    ,dbo.fn_GetUserName(QABy) AS QABy                        
    ,dbo.fn_GetUserName(ShadowQABy) AS ShadowQABy              
 ,isnull(CC.ProviderId, 0) ProviderId              
 ,c.PayorId                    
    ,c.ProviderFeedbackId              
    ,Pr.[Name] AS ProviderText                        
    ,P.[Name] AS PayorText                        
    ,Pf.Feedback AS ProviderFeedbackText                        
    ,wi.NoteTitle                        
    ,dx.DxCode                        
    ,cp.CPTCode                        
    ,QAP.[Name] AS QAPayorText                        
    ,QAPr.[Name] QAProviderText                        
    ,QACPTCode                        
    ,QAMod                        
    ,QADx                        
    ,QAPf.Feedback AS QAProviderFeedbackText                        
    ,QAPayorIdRemark                        
    ,QAProviderIDRemark                        
    ,QACPTCodeRemark                        
    ,QAModRemark                        
    ,QADxRemark                        
    ,QAProviderFeedbackIDRemark                        
    ,bc.[Name] AS BlockCategory                        
    ,bh.Remarks AS BlockRemarks                        
    ,bh.CreateDate AS BlockedDate                        
    ,l.[Name] AS ListName                    
 ,c.ClaimId                    
 ,c.ClaimOrder                    
 ,QAErrorTypeId                  
  ,wi.ProjectId AS ProjectId              
   --INTO #ShadowQABlocked                            
   FROM ClaimCTE c                      
   JOIN ClinicalCase cc ON c.ClinicalCaseId =CC.ClinicalCaseId                    
   INNER JOIN WorkItem wi ON wi.ClinicalCaseId = cc.ClinicalCaseId                        
   LEFT JOIN BlockHistory bh ON bh.ClinicalCaseId = CC.ClinicalCaseId                        
   LEFT JOIN BlockCategory bc ON bc.BlockCategoryId = bh.BlockCategoryId                        
   --LEFT JOIN WorkItemProvider wp ON wp.ClinicalCaseId = CC.ClinicalCaseId                        
   LEFT JOIN QAVersionCTE qv ON qv.ClinicalCaseId = cc.ClinicalCaseId                        
   --LEFT JOIN CoderVersionCTE cv ON cv.ClinicalCaseId = cc.ClinicalCaseId                        
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
  FROM dbo.DxCode dx WHERE dx.ClinicalCaseId = @CurrCCId                          
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
  FROM dbo.cptcode cp WHERE cp.ClinicalCaseId = @CurrCCId                          
 ) cp                    
 ON wi.ClinicalCaseId = cp.ClinicalCaseId AND cp.VersionId = c.VersionId AND cp.ClaimId = ISNULL(c.ClaimId, 0)                    
   LEFT JOIN QARemarksCTE qr ON qr.ClinicalCaseId = c.ClinicalCaseId AND ISNULL(qr.ClaimId, 0) = ISNULL(c.ClaimId, 0)                    
   LEFT JOIN Payor QAP ON QAP.PayorId = qr.QAPayorId                        
   LEFT JOIN [Provider] QAPr ON QAPr.ProviderID = qr.QAProviderID                        
   LEFT JOIN ProviderFeedback QAPf ON QAPf.ProviderFeedbackId = qr.QAProviderFeedbackID                        
   LEFT JOIN Payor P ON P.PayorId = c.PayorId                        
   LEFT JOIN [Provider] Pr ON Pr.ProviderID = c.ProviderID                        
   LEFT JOIN ProviderFeedback Pf ON Pf.ProviderFeedbackId = c.ProviderFeedbackID                    
   LEFT JOIN List l ON l.ListId = CC.ListId                        
   WHERE c.ClinicalCaseId = @CurrCCId AND bh.BlockedInQueueKind = 'ShadowQA'              
   ORDER BY c.ClaimOrder              
          
 SELECT @CCIDs AS CCIDs            
                               
  END                            
                            
  COMMIT TRANSACTION               
 END TRY                            
                            
 BEGIN CATCH                            
  ROLLBACK TRANSACTION                            
 END CATCH                            
END 