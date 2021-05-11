CREATE PROCEDURE [dbo].[USPGetNext]            
 --1,'Coder','ReadyForPosting',1                                  
 @ProjectID INT            
 ,@Role VARCHAR(50)            
 ,@ChartType VARCHAR(50)            
 ,@UserId INT            
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
   SELECT @message = CONVERT(VARCHAR(30), GETDATE(), 121) + ': Sorry, could not obtain a lock within the timeout period, return code was ' + CONVERT(VARCHAR(30), @RC) + '.'            
            
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
   SELECT @message = CONVERT(VARCHAR(30), GETDATE(), 121) + ': AppLock obtained ..'            
            
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
    ,CC.ProviderId            
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
    ,CodedDate = GETDATE()            
   WHERE ClinicalCaseId = (          
     SELECT ClinicalCaseId            
     FROM #CoderAvailable            
     )            
            
   IF NOT EXISTS (            
     SELECT 1            
     FROM Version            
     WHERE UserID = @UserId            
     AND StatusId = 2            
     )            
    AND EXISTS (            
     SELECT 1            
     FROM #CoderAvailable            
     )            
   BEGIN            
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
     ,GETDATE()            
     ,@UserId            
     ,2            
   END            
            
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
 SELECT DISTINCT wp.WorkItemProviderId, wp.ClinicalCaseId, wp.VersionId, wp.ProviderId, wp.PayorId            
 ,  wp.ProviderFeedbackId, wp.ClaimId              
 , DENSE_RANK() OVER(PARTITION BY wp.ClinicalCaseId ORDER BY ISNULL(wp.ClaimId, 0) ) AS ClaimOrder    
 , DENSE_RANK() OVER(PARTITION BY wp.ClinicalCaseId ORDER BY wp.VersionId DESC) as rn              
 FROM WorkItemProvider wp  LEFT JOIN Claim c  ON wp.ClinicalCaseId = c.ClinicalCaseId  AND wp.VersionId = c.VersionId              
 WHERE wp.ClinicalCaseId = @ClinicalCaseId              
 )            
 ,  QAVersionCTE            
   AS (            
    SELECT TOP 1 v.ClinicalCaseId            
     ,v.VersionId            
     ,v.VersionDate            
     ,v.StatusId            
     --,ROW_NUMBER() OVER (            
     -- PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC            
     -- ) AS rn            
    FROM [Version] v            
    JOIN WorkItem wi ON v.ClinicalCaseId = wi.ClinicalCaseId            
    WHERE wi.ClinicalCaseId = @ClinicalCaseId            
     AND wi.StatusId = 14            
     AND v.StatusId IN (            
      7            
      ,8            
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
     AND wi.StatusId = 14            
     AND v.StatusId = 12            
    )            
    ,RebuttedCTE            
   AS (            
    SELECT wa.[ClinicalCaseId], wa.ClaimId            
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
    GROUP BY wa.ClinicalCaseId, wa.ClaimId            
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
    GROUP BY wa.ClinicalCaseId            
     , wa.ErrorTypeId            
     , wa.ClaimId        
    )            
   SELECT DISTINCT             
    cc.ClinicalCaseID            
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
    ,cp.CPTCode            
    ,QAPayorId            
    ,QAPa.Name AS QAPayorText            
    ,QAProviderID            
    ,QAP.Name AS QAProviderText            
    ,QACPTCode            
    ,QAMod            
    ,QADx            
    ,QAProviderFeedbackID            
    ,QAPf.Feedback AS QAFeedbackText            
    ,QAPayorIdRemark            
    ,QAProviderIDRemark            
    ,QACPTCodeRemark            
    ,QAModRemark            
    ,QADxRemark            
    ,QAProviderFeedbackIDRemark            
    --ShadowQA Fields                                    
    ,ShadowQAPayorId            
    ,SQAPa.Name AS ShadowQAPayorText            
    ,ShadowQAProviderID            
    ,SQAP.Name AS ShadowQAProviderText            
    ,ShadowQACPTCode            
    ,ShadowQAMod            
    ,ShadowQADx            
    ,ShadowQAProviderFeedbackID            
    ,SQAPf.Feedback AS ShadowQAFeedbackText            
    ,ShadowQAPayorIdRemark            
    ,ShadowQAProviderIDRemark            
    ,ShadowQACPTCodeRemark            
    ,ShadowQAModRemark            
    ,ShadowQADxRemark            
    ,ShadowQAProviderFeedbackIDRemark            
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
 ,QAErrorTypeId        
 ,ShadowQAErrorTypeId          
    ,c.ClaimId            
    ,c.ClaimOrder            
    ,l.[Name] AS ListName            
   FROM ClaimCTE c            
   INNER JOIN ClinicalCase cc ON cc.ClinicalCaseId = c.ClinicalCaseId            
   INNER JOIN WorkItem wi ON wi.ClinicalCaseId = cc.ClinicalCaseId            
   --INNER JOIN WorkItemProvider wp ON wp.ClinicalCaseId = CC.ClinicalCaseId            
   LEFT JOIN [Provider] P ON c.ProviderId = P.ProviderID            
   LEFT JOIN ProviderFeedback Pf ON c.ProviderFeedbackId = Pf.ProviderFeedbackId            
   LEFT JOIN Payor Pa ON c.PayorId = pa.PayorId            
   --LEFT JOIN QAVersionCTE qv ON qv.ClinicalCaseId = cc.ClinicalCaseId           --LEFT JOIN CoderVersionCTE cv ON cv.ClinicalCaseId = cc.ClinicalCaseId            
   --LEFT JOIN DxCode dx                                                    
   --ON cc.ClinicalCaseId = dx.ClinicalCaseId AND dx.VersionId = cv.VersionId                            
   INNER JOIN (            
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
    AND dx.VersionID = c.VersionID AND ISNULL(dx.ClaimId, 0) = ISNULL(c.ClaimId, 0)            
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
    AND cp.VersionId = c.VersionId AND ISNULL(cp.ClaimId, 0) = ISNULL(c.ClaimId, 0)            
   LEFT JOIN AuditCTE a ON a.ClinicalCaseId = cc.ClinicalCaseId AND ISNULL(a.ClaimId, 0) = ISNULL(c.ClaimId, 0)           
   LEFT JOIN [Provider] QAP ON a.QAProviderID = QAP.ProviderID            
 LEFT JOIN ProviderFeedback QAPf ON a.QAProviderFeedbackID = QAPf.ProviderFeedbackId            
   LEFT JOIN Payor QAPa ON QAPa.PayorId = a.QAPayorId            
   LEFT JOIN ShadowQAAuditCTE sa ON sa.ClinicalCaseId = cc.ClinicalCaseId AND ISNULL(sa.ClaimId, 0) = ISNULL(c.ClaimId, 0)            
   LEFT JOIN [Provider] SQAP ON sa.ShadowQAProviderID = SQAP.ProviderID            
   LEFT JOIN ProviderFeedback SQAPf ON sa.ShadowQAProviderFeedbackID = SQAPf.ProviderFeedbackId            
   LEFT JOIN Payor SQAPa ON SQAPa.PayorId = sa.ShadowQAPayorId            
   LEFT JOIN RebuttedCTE r ON r.ClinicalCaseId = cc.ClinicalCaseId AND ISNULL(r.ClaimId, 0) = ISNULL(c.ClaimId, 0)        
   --LEFT JOIN ProjectUser pu ON pu.ProjectId = wi.ProjectId            
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
 ,   CoderVersionCTE            
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
     AND v.StatusId = 3        
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
   INNER JOIN (      
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
   INNER JOIN (            
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
   INNER JOIN ProjectUser pu ON pu.ProjectId = w.ProjectId            
   LEFT JOIN List l ON l.ListId = CC.ListId          
   WHERE W.ClinicalCaseId = @ClinicalCaseId            
   ORDER BY c.ClaimOrder            
             
  END            
  ELSE IF @Role = 'Coder'            
   AND @ChartType = 'Block'            
  BEGIN            
   SELECT TOP 1 CC.ClinicalCaseID            
    ,PatientMRN            
    ,PatientFirstName + ' ' + PatientLastName AS [Name]            
    --,CAST(DateOfService AS VARCHAR(20)) AS DateOfService            
    --,isnull(CC.ProviderId, 0)           
  ,DateOfService            
    ,CC.ProviderId          
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
   ORDER BY CC.ClinicalCaseID            
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
                                                      
                                                      
 UPDATE WorkItem set StatusId = 5, QABy = @UserId, QADate = GETDATE()               
 WHERE ClinicalCaseId = @ClinicalCaseId              
 --WHERE ClinicalCaseId = (SELECT ClinicalCaseId FROM #QAAvailable)              
               
 IF NOT EXISTS(SELECT 1 FROM [Version] WHERE UserID = @UserId AND StatusId = 5) AND EXISTS(SELECT 1 FROM #QAAvailable)              
 BEGIN              
  INSERT INTO [Version] (ClinicalcaseID,VersionDate,userID,Statusid)              
  SELECT @ClinicalCaseId,GETDATE(),@UserId,5              
  --SELECT (SELECT ClinicalCaseId FROM #QAAvailable),GETDATE(),@UserId,5              
 END              
                                                      
 SELECT * FROM #QAAvailable                                                    
                                                      
END             
  ELSE IF @Role = 'QA'            
   AND @ChartType = 'Block'            
  BEGIN            
    SELECT TOP 1 @ClinicalCaseId = W.ClinicalCaseId               
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
   WHERE c.ClinicalCaseId = @ClinicalCaseId          
   ORDER BY c.ClaimOrder            
    --UPDATE WorkItem set StatusId = 5, QABy = @UserId, QADate = GETDATE() WHERE ClinicalCaseId = (                
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
     AND v.StatusId = 7            
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
     AND wi.StatusId = 12            
     AND v.StatusId = 3            
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
   --LEFT JOIN WorkItemProvider wp                                                      
   --ON wp.ClinicalCaseId =CC.ClinicalCaseId                                                      
   --LEFT JOIN QAVersionCTE qv                                                      
   --ON qv.ClinicalCaseId = cc.ClinicalCaseId                                                
   LEFT JOIN RebuttedVersionCTE cv                                                      
      ON cv.ClinicalCaseId = cc.ClinicalCaseId                                                      
   --LEFT JOIN DxCode dx                                                      
   --ON cc.ClinicalCaseId = dx.ClinicalCaseId AND dx.VersionId = cv.VersionId                              
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
            
   WITH ClaimCTE            
   AS (            
    SELECT DISTINCT wp.WorkItemProviderId            
     ,wp.ClinicalCaseId            
     ,wp.VersionId            
     ,wp.ProviderId            
     ,wp.PayorId            
     ,wp.ProviderFeedbackId            
     ,wp.ClaimId            
     ,DENSE_RANK() OVER (            
      PARTITION BY wp.ClinicalCaseId ORDER BY ISNULL(wp.ClaimId, 0)            
      ) AS ClaimOrder            
    FROM WorkItemProvider wp            
    LEFT JOIN Claim c ON wp.ClinicalCaseId = c.ClinicalCaseId            
     AND wp.VersionId = c.VersionId            
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
    WHERE wi.ProjectId = @ProjectID            
     AND wi.StatusId = 11            
     AND v.StatusId IN (            
      6            
      ,7            
      )            
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
    WHERE wi.ProjectId = @ProjectID            
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
    WHERE wi.ProjectId = @ProjectID            
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
    WHERE wi.ProjectId = @ProjectID            
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
    GROUP BY wa.ClinicalCaseId            
     ,wa.ClaimId            
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
    GROUP BY wa.ClinicalCaseId            
     ,wa.ErrorTypeId            
     ,wa.ClaimId            
    )            
    ,ShadowQAAuditCTE            
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
    ,QAPayorId            
    ,QAPa.Name AS QAPayorText            
    ,QAProviderID            
    ,QAP.Name AS QAProviderText            
    ,QACPTCode            
    ,QAMod            
    ,QADx            
    ,QAProviderFeedbackID            
    ,QAPf.Feedback AS QAFeedbackText            
    ,QAPayorIdRemark            
    ,QAProviderIDRemark            
    ,QACPTCodeRemark            
    ,QAModRemark            
    ,QADxRemark            
    ,QAProviderFeedbackIDRemark            
    --ShadowQA Fields                                    
    ,ShadowQAPayorId            
    ,SQAPa.Name AS ShadowQAPayorText            
    ,ShadowQAProviderID            
    ,SQAP.Name AS ShadowQAProviderText            
    ,ShadowQACPTCode            
    ,ShadowQAMod            
    ,ShadowQADx            
    ,ShadowQAProviderFeedbackID            
    ,SQAPf.Feedback AS ShadowQAFeedbackText            
    ,ShadowQAPayorIdRemark            
    ,ShadowQAProviderIDRemark            
    ,ShadowQACPTCodeRemark            
    ,ShadowQAModRemark            
    ,ShadowQADxRemark            
    ,ShadowQAProviderFeedbackIDRemark            
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
    ,QAErrorTypeId      
    ,ShadowQAErrorTypeId            
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
   LEFT JOIN AuditCTE a ON a.ClinicalCaseId = c.ClinicalCaseId AND ISNULL(a.ClaimId, 0) = ISNULL(c.ClaimId, 0)          
   LEFT JOIN [Provider] QAP ON a.QAProviderID = QAP.ProviderID            
   LEFT JOIN ProviderFeedback QAPf ON a.QAProviderFeedbackID = QAPf.ProviderFeedbackId            
   LEFT JOIN Payor QAPa ON QAPa.PayorId = a.QAPayorId            
   LEFT JOIN ShadowQAAuditCTE sa ON sa.ClinicalCaseId = c.ClinicalCaseId AND ISNULL(sa.ClaimId, 0) = ISNULL(c.ClaimId, 0)            
   LEFT JOIN [Provider] SQAP ON sa.ShadowQAProviderID = SQAP.ProviderID            
   LEFT JOIN ProviderFeedback SQAPf ON sa.ShadowQAProviderFeedbackID = SQAPf.ProviderFeedbackId            
   LEFT JOIN Payor SQAPa ON SQAPa.PayorId = sa.ShadowQAPayorId            
   LEFT JOIN RebuttedCTE r ON r.ClinicalCaseId = c.ClinicalCaseId AND ISNULL(r.ClaimId, 0) = ISNULL(c.ClaimId, 0)            
   JOIN ProjectUser pu ON pu.ProjectId = wi.ProjectId            
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
            
   WITH ClaimCTE            
   AS (            
    SELECT DISTINCT wp.WorkItemProviderId            
     ,wp.ClinicalCaseId            
     ,wp.VersionId            
     ,wp.ProviderId            
     ,wp.PayorId    
     ,wp.ProviderFeedbackId            
     ,wp.ClaimId            
     ,DENSE_RANK() OVER (            
      PARTITION BY wp.ClinicalCaseId ORDER BY ISNULL(wp.ClaimId, 0)            
      ) AS ClaimOrder            
    FROM WorkItemProvider wp            
    LEFT JOIN Claim c ON wp.ClinicalCaseId = c.ClinicalCaseId            
     AND wp.VersionId = c.VersionId            
    WHERE wp.ClinicalCaseId = @ClinicalCaseId            
    )            
    , ShadowQAVersionCTE            
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
    WHERE wi.ProjectId = @ProjectID            
     AND wi.StatusId = 13            
     AND v.StatusId IN (            
      6            
      ,7            
      )            
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
    WHERE wi.ProjectId = @ProjectID            
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
    WHERE wi.ProjectId = @ProjectID            
     AND wi.StatusId = 13            
     AND v.StatusId = 13            
    )            
    ,QARemarksCTE            
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
    GROUP BY wa.ClinicalCaseId            
     ,wa.ClaimId        
  ,wa.ErrorTypeId        
    )            
    ,ShadowQAAuditCTE            
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
    GROUP BY wa.ClinicalCaseId            
     ,wa.ClaimId        
  ,wa.ErrorTypeId        
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
    ,ShadowQAPayorId            
    ,ShadowQAProviderID            
    ,ShadowQACPTCode            
    ,ShadowQAMod            
    ,ShadowQADx            
    ,ShadowQAProviderFeedbackID            
    ,ShadowQAPayorIdRemark            
    ,ShadowQAProviderIDRemark            
    ,ShadowQACPTCodeRemark            
    ,ShadowQAModRemark            
    ,ShadowQADxRemark            
    ,ShadowQAProviderFeedbackIDRemark            
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
    ,QAP.[Name] AS QAPayorText            
    ,QAPr.[Name] QAProviderText            
    ,l.[Name] AS ListName            
    ,c.ClaimId            
    ,c.ClaimOrder        
   ,QAErrorTypeId      
   ,ShadowQAErrorTypeId      
   FROM ClaimCTE c            
   JOIN ClinicalCase cc ON cc.ClinicalCaseId = c.ClinicalCaseId          
   INNER JOIN WorkItem wi ON wi.ClinicalCaseId = cc.ClinicalCaseId            
   --INNER JOIN WorkItemProvider wp ON wp.ClinicalCaseId = CC.ClinicalCaseId            
   LEFT JOIN ShadowQAVersionCTE qv ON qv.ClinicalCaseId = cc.ClinicalCaseId            
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
   LEFT JOIN ShadowQAAuditCTE a ON a.ClinicalCaseId = cc.ClinicalCaseId AND ISNULL(a.ClaimId, 0) = ISNULL(c.ClaimId, 0)            
   LEFT JOIN QARebuttedCTE r ON r.ClinicalCaseId = cc.ClinicalCaseId AND ISNULL(r.ClaimId, 0) = ISNULL(c.ClaimId, 0)          
   JOIN ProjectUser pu ON pu.ProjectId = wi.ProjectId            
   LEFT JOIN QARemarksCTE qr ON qr.ClinicalCaseId = wi.ClinicalCaseId AND ISNULL(qr.ClaimId, 0) = ISNULL(c.ClaimId, 0)            
   LEFT JOIN Payor QAP ON QAP.PayorId = qr.QAPayorId            
   LEFT JOIN [Provider] QAPr ON QAPr.ProviderID = qr.QAProviderID            
   LEFT JOIN ProviderFeedback QAPf ON QAPf.ProviderFeedbackId = qr.QAProviderFeedbackID            
   LEFT JOIN Payor P ON P.PayorId = c.PayorId            
   LEFT JOIN [Provider] Pr ON Pr.ProviderID = c.ProviderID            
   LEFT JOIN ProviderFeedback Pf ON Pf.ProviderFeedbackId = c.ProviderFeedbackID            
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
    WHERE wi.ProjectId = @ProjectID            
     AND wi.StatusId IN (            
      8            
      ,9            
      )            
     AND v.StatusId IN (            
      3            
      ,12            
      )            
    )            
    ,QARemarksCTE            
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
    ,l.[Name] AS ListName            
    ,c.ClaimId            
    ,c.ClaimOrder        
   ,QAErrorTypeId      
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
   LEFT JOIN QARemarksCTE qr ON qr.ClinicalCaseId = c.ClinicalCaseId AND ISNULL(qr.ClaimId, 0) = ISNULL(c.ClaimId, 0)                                           
   LEFT JOIN Payor QAP ON QAP.PayorId = qr.QAPayorId            
   LEFT JOIN [Provider] QAPr ON QAPr.ProviderID = qr.QAProviderID            
   LEFT JOIN ProviderFeedback QAPf ON QAPf.ProviderFeedbackId = qr.QAProviderFeedbackID          
   LEFT JOIN List l ON l.ListId = CC.ListId            
   WHERE wi.ClinicalCaseId = @ClinicalCaseId           
   ORDER BY c.ClaimOrder            
            
   UPDATE WorkItem            
   SET StatusId = 9            
    ,ShadowQABy = @UserId            
    ,ShadowQADate = GETDATE()            
   WHERE ClinicalCaseId = @ClinicalCaseId           
            
   IF NOT EXISTS (                 SELECT 1            
     FROM Version            
     WHERE UserID = @UserId            
      AND StatusId = 9            
     )            
    AND EXISTS (            
     SELECT 1            
     FROM #ShadowQAAvailable            
     )            
   BEGIN            
    INSERT INTO Version (            
     ClinicalcaseID            
     ,VersionDate            
     ,userID            
     ,Statusid            
     )            
    SELECT @ClinicalCaseId          
     ,GETDATE()            
     ,@UserId            
     ,9            
  END            
            
   SELECT *            
   FROM #ShadowQAAvailable            
  END            
  ELSE IF @Role = 'ShadowQA'            
   AND @ChartType = 'Block'            
  BEGIN            
    SELECT TOP 1 @ClinicalCaseId = W.ClinicalCaseId               
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
              
 ;WITH ClaimCTE AS               
 (              
 SELECT DISTINCT wp.WorkItemProviderId, wp.ClinicalCaseId, wp.VersionId, wp.ProviderId, wp.PayorId,  wp.ProviderFeedbackId, wp.ClaimId          
 , DENSE_RANK() OVER(PARTITION BY wp.ClinicalCaseId ORDER BY ISNULL(wp.ClaimId, 0) ) AS ClaimOrder          
 FROM WorkItemProvider wp          
 LEFT JOIN Claim c  ON wp.ClinicalCaseId = c.ClinicalCaseId  AND wp.VersionId = c.VersionId          
 WHERE wp.ClinicalCaseId = @ClinicalCaseId              
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
   -- ,CoderVersionCTE            
   --AS (            
   -- SELECT v.ClinicalCaseId            
   --  ,v.VersionId            
   --  ,v.VersionDate            
   --  ,v.StatusId            
   --  ,ROW_NUMBER() OVER (            
   --   PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC            
   --   ) AS rn            
   -- FROM [Version] v            
   -- JOIN WorkItem wi ON v.ClinicalCaseId = wi.ClinicalCaseId            
   -- WHERE wi.ProjectId = @ProjectID            
   --  AND wi.StatusId IN (            
   --   8            
   --   ,9            
   --   )            
   --  AND v.StatusId IN (            
   --   3            
   --   ,12            
   --   )            
   -- )            
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
   SELECT TOP 1 cc.ClinicalCaseID            
    ,PatientMRN            
    ,PatientFirstName + ' ' + PatientLastName AS [Name]            
    ,CAST(DateOfService AS VARCHAR(10)) AS DateOfService            
    ,dbo.fn_GetUserName(AssignedTo) AS CodedBy            
    ,dbo.fn_GetUserName(QABy) AS QABy            
    ,dbo.fn_GetUserName(ShadowQABy) AS ShadowQABy            
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
 ON wi.ClinicalCaseId = cp.ClinicalCaseId AND cp.VersionId = c.VersionId AND cp.ClaimId = ISNULL(c.ClaimId, 0)        
   LEFT JOIN QARemarksCTE qr ON qr.ClinicalCaseId = c.ClinicalCaseId AND ISNULL(qr.ClaimId, 0) = ISNULL(c.ClaimId, 0)        
   LEFT JOIN Payor QAP ON QAP.PayorId = qr.QAPayorId            
   LEFT JOIN [Provider] QAPr ON QAPr.ProviderID = qr.QAProviderID            
   LEFT JOIN ProviderFeedback QAPf ON QAPf.ProviderFeedbackId = qr.QAProviderFeedbackID            
   LEFT JOIN Payor P ON P.PayorId = c.PayorId            
   LEFT JOIN [Provider] Pr ON Pr.ProviderID = c.ProviderID            
   LEFT JOIN ProviderFeedback Pf ON Pf.ProviderFeedbackId = c.ProviderFeedbackID        
   LEFT JOIN List l ON l.ListId = CC.ListId            
   WHERE c.ClinicalCaseId = @ClinicalCaseId          
   ORDER BY c.ClaimOrder        
        
    --UPDATE WorkItem set StatusId = 9, QABy = @UserId, QADate = GETDATE() WHERE ClinicalCaseId = (                
    --SELECT ClinicalCaseId FROM #ShadowQABlocked                
    --)                
    --SELECT * FROM #ShadowQABlocked                
  END            
            
  COMMIT TRANSACTION            
 END TRY            
            
 BEGIN CATCH            
  ROLLBACK TRANSACTION            
 END CATCH            
END     