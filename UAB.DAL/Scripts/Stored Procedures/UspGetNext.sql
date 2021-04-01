  
  
  
Create PROCEDURE [dbo].[USPGetNext] --1,'QA','Available',1                    
@ProjectID INT,                                        
@Role VARCHAR(50),                                      
@ChartType VARCHAR(50),                                      
@UserId INT                                      
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
  RAISERROR (@message, 0, 1) WITH NOWAIT;                  
  ROLLBACK TRANSACTION                  
  RETURN @RC                  
 END                      
 ELSE                      
 BEGIN                      
  SELECT @message = CONVERT(VARCHAR(30), GETDATE(), 121) + ': AppLock obtained ..'                      
  RAISERROR (@message, 0, 1) WITH NOWAIT;                  
 END                  
                  
DECLARE @WorkItemID int                                      
DECLARE @ClinicalCaseId int                                      
                                      
IF @Role = 'Coder' AND @ChartType = 'Available'                                      
BEGIN                                      
                          
 SELECT TOP 1 CC.ClinicalCaseID, PatientMRN                                        
    ,PatientFirstName + ' ' + PatientLastName AS [Name]                                        
 --   ,CAST(DateOfService AS VARCHAR(20)) AS DateOfService                
 --,isnull(CC.ProviderId, 0)  ProviderId                                     
 ,DateOfService  
 ,CC.ProviderId  
 ,l.[Name] AS ListName  
    INTO #CoderAvailable                                    
 FROM ClinicalCase CC INNER JOIN WorkItem W                                         
   ON W.ClinicalCaseId = CC.ClinicalCaseId                                    
   INNER JOIN ProjectUser pu ON pu.ProjectId = w.ProjectId  
   LEFT JOIN List l ON l.ListId = CC.ListId                  
 WHERE CC.ProjectId = @ProjectID                    
 AND (                    
  (W.IsPriority = 1 AND W.StatusID IN (1, 2) AND (W.AssignedTo = @UserId OR W.AssignedTo IS NULL))                  
  OR                    
  (W.StatusId = 2 AND w.AssignedTo = @UserId AND ISNULL(W.IsBlocked, 0) = 0)                    
  OR                    
  W.StatusId = 1                    
 )                     
 AND pu.UserId = @UserId AND pu.RoleId = 1 AND pu.IsActive = 1                            
 ORDER BY CASE                     
   WHEN W.IsPriority = 1 THEN 1                     
   WHEN (W.StatusId = 2 AND w.AssignedTo = @UserId AND ISNULL(W.IsBlocked, 0) = 0) THEN 2                    
   WHEN W.StatusId = 1 THEN 3                    
   END ASC                    
   , CC.DateOfService ASC                    
                       
 UPDATE WorkItem set StatusId = 2, AssignedTo = @UserId, CodedDate = GETDATE() where ClinicalCaseId = (                                    
 SELECT ClinicalCaseId FROM #CoderAvailable                                    
 )               
                                    
 SELECT * FROM #CoderAvailable                                    
                                      
END                                      
ELSE IF @Role = 'Coder' AND @ChartType = 'Incorrect'                                     
BEGIN                                      
                                      
 ;WITH QAVersionCTE AS (                
        SELECT v.ClinicalCaseId, v.VersionId, v.VersionDate, v.StatusId                                      
        , ROW_NUMBER() OVER(PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC) AS rn                                      
        FROM [Version] v                                      
        JOIN WorkItem wi                         
        ON v.ClinicalCaseId = wi.ClinicalCaseId                                      
        WHERE wi.ProjectId = @ProjectID                                      
        AND wi.StatusId = 14 and v.StatusId IN (7, 8)                            
    ),ShadowQAVersionCTE AS (                                      
        SELECT v.ClinicalCaseId, v.VersionId, v.VersionDate, v.StatusId                                      
        , ROW_NUMBER() OVER(PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC) AS rn                                      
        FROM [Version] v                   
        JOIN WorkItem wi                                    
        ON v.ClinicalCaseId = wi.ClinicalCaseId                                      
        WHERE wi.ProjectId = @ProjectID                                      
  AND wi.StatusId = 14                      
        AND v.StatusId = 11                                      
    )                        
 ,CoderVersionCTE AS (                                      
        SELECT v.ClinicalCaseId, v.VersionId, v.VersionDate, v.StatusId                                       
        , ROW_NUMBER() OVER(PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC) AS rn                                      
        FROM [Version] v                                    
        JOIN WorkItem wi                                    
        ON v.ClinicalCaseId = wi.ClinicalCaseId                                    
        WHERE wi.ProjectId = @ProjectID                                    
        AND wi.StatusId = 14 and v.StatusId = 3                            
    ),RebuttedVersionCTE AS (                                        
        SELECT v.ClinicalCaseId, v.VersionId, v.VersionDate, v.StatusId                                        
        , ROW_NUMBER() OVER(PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC) AS rn                                        
        FROM [Version] v                                    
        JOIN WorkItem wi                                    
        ON v.ClinicalCaseId = wi.ClinicalCaseId                                    
        WHERE wi.ProjectId = @ProjectID                                    
        AND wi.StatusId = 14 AND v.StatusId = 12                                  
    )                                        
 ,RebuttedCTE AS (                                        
        SELECT wa.[ClinicalCaseId]                                        
            ,MAX(CASE WHEN FieldName = 'PayorId' THEN FieldValue END) AS RebuttedPayorId                                        
            ,MAX(CASE WHEN FieldName = 'ProviderID' THEN FieldValue END) AS RebuttedProviderID                                        
            ,MAX(CASE WHEN FieldName = 'CPTCode' THEN FieldValue END) AS RebuttedCPTCode                                        
            ,MAX(CASE WHEN FieldName = 'Mod' THEN FieldValue END) AS RebuttedMod                                        
            ,MAX(CASE WHEN FieldName = 'Dx' THEN FieldValue END) AS RebuttedDx                                        
            ,MAX(CASE WHEN FieldName = 'ProviderFeedbackID' THEN FieldValue END) AS RebuttedProviderFeedbackID                                        
            ,MAX(CASE WHEN FieldName = 'PayorId' THEN Remark END) AS RebuttedPayorIdRemark                                        
            ,MAX(CASE WHEN FieldName = 'ProviderID' THEN Remark END) AS RebuttedProviderIDRemark                                        
            ,MAX(CASE WHEN FieldName = 'CPTCode' THEN Remark END) AS RebuttedCPTCodeRemark                        
            ,MAX(CASE WHEN FieldName = 'Mod' THEN Remark END) AS RebuttedModRemark                                        
            ,MAX(CASE WHEN FieldName = 'Dx' THEN Remark END) AS RebuttedDxRemark                                
            ,MAX(CASE WHEN FieldName = 'ProviderFeedbackID' THEN Remark END) AS RebuttedProviderFeedbackIDRemark                                        
        FROM [dbo].[WorkItemAudit] wa                                        
        JOIN RebuttedVersionCTE rv                                        
        ON rv.ClinicalCaseId = wa.ClinicalCaseId AND wa.VersionId = rv.VersionId                                        
        GROUP BY wa.ClinicalCaseId )                                 
    ,AuditCTE AS (                                      
        SELECT wa.[ClinicalCaseId]                                      
            ,MAX(CASE WHEN FieldName = 'PayorId' THEN FieldValue END) AS QAPayorId                                      
    ,MAX(CASE WHEN FieldName = 'ProviderID' THEN FieldValue END) AS QAProviderID                                      
            ,MAX(CASE WHEN FieldName = 'CPTCode' THEN FieldValue END) AS QACPTCode                                      
            ,MAX(CASE WHEN FieldName = 'Mod' THEN FieldValue END) AS QAMod                                      
            ,MAX(CASE WHEN FieldName = 'Dx' THEN FieldValue END) AS QADx                                      
            ,MAX(CASE WHEN FieldName = 'ProviderFeedbackID' THEN FieldValue END) AS QAProviderFeedbackID                                      
            ,MAX(CASE WHEN FieldName = 'PayorId' THEN Remark END) AS QAPayorIdRemark                                      
            ,MAX(CASE WHEN FieldName = 'ProviderID' THEN Remark END) AS QAProviderIDRemark                                      
            ,MAX(CASE WHEN FieldName = 'CPTCode' THEN Remark END) AS QACPTCodeRemark                                      
   ,MAX(CASE WHEN FieldName = 'Mod' THEN Remark END) AS QAModRemark                                      
            ,MAX(CASE WHEN FieldName = 'Dx' THEN Remark END) AS QADxRemark                                      
            ,MAX(CASE WHEN FieldName = 'ProviderFeedbackID' THEN Remark END) AS QAProviderFeedbackIDRemark                      
   ,et.[Name] AS QAErrorType                      
        FROM [dbo].[WorkItemAudit] wa                                      
        JOIN QAVersionCTE qv                                      
        ON qv.ClinicalCaseId = wa.ClinicalCaseId AND wa.VersionId = qv.VersionId                      
  LEFT JOIN ErrorType et                      
  ON et.ErrorTypeId = wa.ErrorTypeId                      
        GROUP BY wa.ClinicalCaseId, et.[Name]                      
    )                      
 ,ShadowQAAuditCTE AS (                                      
        SELECT wa.[ClinicalCaseId]                                      
            ,MAX(CASE WHEN FieldName = 'PayorId' THEN FieldValue END) AS ShadowQAPayorId                                      
            ,MAX(CASE WHEN FieldName = 'ProviderID' THEN FieldValue END) AS ShadowQAProviderID                                      
            ,MAX(CASE WHEN FieldName = 'CPTCode' THEN FieldValue END) AS ShadowQACPTCode                                      
        ,MAX(CASE WHEN FieldName = 'Mod' THEN FieldValue END) AS ShadowQAMod                                      
            ,MAX(CASE WHEN FieldName = 'Dx' THEN FieldValue END) AS ShadowQADx                                      
            ,MAX(CASE WHEN FieldName = 'ProviderFeedbackID' THEN FieldValue END) AS ShadowQAProviderFeedbackID                               
            ,MAX(CASE WHEN FieldName = 'PayorId' THEN Remark END) AS ShadowQAPayorIdRemark                                      
     ,MAX(CASE WHEN FieldName = 'ProviderID' THEN Remark END) AS ShadowQAProviderIDRemark                                      
            ,MAX(CASE WHEN FieldName = 'CPTCode' THEN Remark END) AS ShadowQACPTCodeRemark      
   ,MAX(CASE WHEN FieldName = 'Mod' THEN Remark END) AS ShadowQAModRemark                                      
            ,MAX(CASE WHEN FieldName = 'Dx' THEN Remark END) AS ShadowQADxRemark                                      
            ,MAX(CASE WHEN FieldName = 'ProviderFeedbackID' THEN Remark END) AS ShadowQAProviderFeedbackIDRemark                      
   ,et.[Name] AS ShadowQAErrorType                      
        FROM [dbo].[WorkItemAudit] wa                                      
        JOIN ShadowQAVersionCTE qv                                      
        ON qv.ClinicalCaseId = wa.ClinicalCaseId AND wa.VersionId = qv.VersionId                      
 LEFT JOIN ErrorType et                      
  ON et.ErrorTypeId = wa.ErrorTypeId                      
        GROUP BY wa.ClinicalCaseId, et.[Name]                      
    )                      
    SELECT TOP 1                                      
    cc.ClinicalCaseID                                      
    ,PatientMRN                                      
    ,PatientFirstName + ' ' + PatientLastName AS [Name]                                      
    ,CAST(DateOfService AS VARCHAR(10)) AS DateOfService                
 ,isnull(CC.ProviderId,0)                        
 ,dbo.fn_GetUserName(AssignedTo) AS CodedBy                      
 ,dbo.fn_GetUserName(QABy) AS QABy                      
 ,dbo.fn_GetUserName(ShadowQABy) AS ShadowQABy                                   
    ,wp.ProviderId               
 ,p.Name AS ProviderText                                 
    ,wp.PayorId                                      
 ,pa.Name AS PayorText                      
    ,wp.ProviderFeedbackId                             
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
                      
 ,QAErrorType                      
 ,ShadowQAErrorType                      
    FROM ClinicalCase cc                                      
    INNER JOIN WorkItem wi                    
    ON wi.ClinicalCaseId = cc.ClinicalCaseId                                      
    INNER JOIN WorkItemProvider wp                                      
    ON wp.ClinicalCaseId =CC.ClinicalCaseId                               
 LEFT JOIN [Provider] P                       
 ON wp.ProviderId = P.ProviderID                      
 LEFT JOIN ProviderFeedback Pf                      
 ON wp.ProviderFeedbackId = Pf.ProviderFeedbackId                      
 LEFT JOIN Payor Pa                       
 ON wp.PayorId = pa.PayorId                             
    LEFT JOIN QAVersionCTE qv                                      
    ON qv.ClinicalCaseId = cc.ClinicalCaseId                                      
    LEFT JOIN CoderVersionCTE cv                                      
    ON cv.ClinicalCaseId = cc.ClinicalCaseId                                      
    --LEFT JOIN DxCode dx                                      
    --ON cc.ClinicalCaseId = dx.ClinicalCaseId AND dx.VersionId = cv.VersionId              
 INNER JOIN (                    
  SELECT DISTINCT dx.ClinicalCaseId, dx.VersionId,                                      
  SUBSTRING(                                      
   (                                      
    SELECT ',' + dxc.DxCode  AS [text()]                                      
    FROM dbo.DxCode dxc                                      
    WHERE dx.ClinicalCaseId = dxc.ClinicalCaseId AND dxc.VersionId = dx.VersionId                          
    ORDER BY dxc.ClinicalCaseId                                      
    FOR XML PATH ('')                                      
   ), 2, 1000) [DxCode]                                      
  FROM dbo.DxCode dx                                      
 ) dx on dx.ClinicalCaseId = wp.ClinicalCaseId AND dx.VersionID = cv.VersionID              
    LEFT JOIN (                                      
  SELECT DISTINCT cp.ClinicalCaseId, cp.VersionId,                                      
  SUBSTRING(                                      
   (                                      
    SELECT '|' + cptc.cptCode + '^'+isnull(cptc.Modifier,'') + '^'+ISNULL(cptc.qty,'')+'^'+isnull(cptc.Links,'')   AS [text()]               
    FROM dbo.cptcode cptc                                      
    WHERE cp.ClinicalCaseId = cptc.ClinicalCaseId AND cp.VersionID = cptc.VersionID                                      
    ORDER BY cptc.ClinicalCaseId                                      
    FOR XML PATH ('')                              
   ), 2, 1000) [CptCode]                                      
  FROM dbo.cptcode cp                                      
 ) cp              
    ON cc.ClinicalCaseId = cp.ClinicalCaseId AND cp.VersionId = cv.VersionId                                      
    LEFT JOIN AuditCTE a                                      
    ON a.ClinicalCaseId = cc.ClinicalCaseId                          
 LEFT JOIN [Provider] QAP                       
 ON a.QAProviderID = QAP.ProviderID                      
 LEFT JOIN ProviderFeedback QAPf                      
 ON a.QAProviderFeedbackID = QAPf.ProviderFeedbackId                      
 LEFT JOIN Payor QAPa                       
 ON QAPa.PayorId = a.QAPayorId                      
                      
 LEFT JOIN ShadowQAAuditCTE sa                                      
    ON sa.ClinicalCaseId = cc.ClinicalCaseId                         
 LEFT JOIN [Provider] SQAP                       
 ON sa.ShadowQAProviderID = SQAP.ProviderID                      
 LEFT JOIN ProviderFeedback SQAPf                      
 ON sa.ShadowQAProviderFeedbackID = SQAPf.ProviderFeedbackId                      
 LEFT JOIN Payor SQAPa                       
 ON SQAPa.PayorId = sa.ShadowQAPayorId                      
                      
    LEFT JOIN RebuttedCTE r                                        
    ON r.ClinicalCaseId = cc.ClinicalCaseId                                    
    LEFT JOIN ProjectUser pu                                    
   ON pu.ProjectId = wi.ProjectId                                    
    WHERE cc.ProjectId = @ProjectID AND wi.StatusId = 14 AND wi.AssignedTo = @UserId AND pu.RoleId = 1 AND pu.IsActive = 1                                    
    AND qv.rn = 1                                      
    ORDER BY qv.VersionDate                                      
                                        
END                                      
ELSE IF @Role = 'Coder' AND @ChartType = 'ReadyForPosting'                                      
BEGIN          
 ;WITH CoderVersionCTE AS (                                      
        SELECT v.ClinicalCaseId, v.VersionId, v.VersionDate, v.StatusId                                      
        , ROW_NUMBER() OVER(PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC) AS rn                                      
        FROM [Version] v                                      
        JOIN WorkItem wi                            
        ON v.ClinicalCaseId = wi.ClinicalCaseId                                      
        WHERE wi.ProjectId = @ProjectID          
  AND wi.AssignedTo = @UserId          
        AND wi.StatusId = 15                            
  AND v.StatusId = 3                            
 )                            
 SELECT TOP 1 CC.ClinicalCaseID,PatientMRN                                        
    ,PatientFirstName + ' ' + PatientLastName AS [Name]                                        
    ,CAST(DateOfService AS VARCHAR(20)) AS DateOfService                
 ,isnull(CC.ProviderId,0)                                      
      ,cp.CPTCode,dx.DxCode,wp.ProviderId,                                      
   wp.PayorId,wp.ProviderFeedbackId,w.NoteTitle                   
    ,dbo.fn_GetUserName(AssignedTo) AS CodedBy                      
 ,dbo.fn_GetUserName(QABy) AS QABy                      
 ,dbo.fn_GetUserName(ShadowQABy) AS ShadowQABy                                         
    FROM ClinicalCase CC INNER JOIN WorkItem W                                         
   ON W.ClinicalCaseId = CC.ClinicalCaseId                                       
   INNER JOIN CoderVersionCTE cv ON cv.ClinicalCaseID = cc.ClinicalCaseID                                      
   INNER JOIN WorkItemProvider wp on wp.ClinicalCaseId = w.ClinicalCaseId AND wp.VersionID = cv.VersionID                                      
  INNER JOIN       
  (                                      
  SELECT DISTINCT cp.ClinicalCaseId, cp.VersionId,                                      
  SUBSTRING(                                      
   (                                      
    SELECT '|' + cptc.cptCode + '^'+isnull(cptc.Modifier,'') + '^'+ISNULL(cptc.qty,'')+'^'+isnull(cptc.Links,'')   AS [text()]               
    FROM dbo.cptcode cptc                                      
    WHERE cp.ClinicalCaseId = cptc.ClinicalCaseId AND cp.VersionID = cptc.VersionID                                      
    ORDER BY cptc.ClinicalCaseId                                      
    FOR XML PATH ('')                              
   ), 2, 1000) [CptCode]    
  FROM dbo.cptcode cp                                      
 ) cp on cp.ClinicalCaseId = wp.ClinicalCaseId AND cp.VersionID = cv.VersionID                                      
   INNER JOIN (                                      
  SELECT DISTINCT dx.ClinicalCaseId, dx.VersionId,                                      
  SUBSTRING(                                      
   (                                      
    SELECT ',' + dxc.DxCode  AS [text()]                                      
    FROM dbo.DxCode dxc                                      
    WHERE dx.ClinicalCaseId = dxc.ClinicalCaseId AND dxc.VersionId = dx.VersionId                          
    ORDER BY dxc.ClinicalCaseId                                      
    FOR XML PATH ('')                                      
   ), 2, 1000) [DxCode]                                      
  FROM dbo.DxCode dx                                      
 ) dx on dx.ClinicalCaseId = wp.ClinicalCaseId AND dx.VersionID = cv.VersionID                                      
 INNER JOIN ProjectUser pu ON pu.ProjectId = w.ProjectId                       
 WHERE CC.ProjectId = @ProjectID AND W.StatusId = 15                                      
 AND cv.rn = 1                                      
 AND w.AssignedTo = @UserId          
 AND pu.UserId = @UserId AND pu.RoleId = 1 AND pu.IsActive = 1                          
 ORDER BY CC.CreatedDate                                       
                         
END                                      
ELSE IF @Role = 'Coder' AND @ChartType = 'Block'                           
BEGIN                    
                       
 SELECT TOP 1 CC.ClinicalCaseID,PatientMRN                                        
    ,PatientFirstName + ' ' + PatientLastName AS [Name]                                        
    ,CAST(DateOfService AS VARCHAR(20)) AS DateOfService                
 ,isnull(CC.ProviderId,0)                     
 ,Bc.Name AS BlockCategory                    
 ,Bh.Remarks AS BlockRemarks                    
 ,Bh.CreateDate AS BlockedDate    
  ,l.[Name] AS ListName                                  
 FROM ClinicalCase CC INNER JOIN WorkItem W                                         
   ON W.ClinicalCaseId = CC.ClinicalCaseId  
   LEFT JOIN List l ON l.ListId = CC.ListId                                   
   INNER JOIN ProjectUser pu ON pu.ProjectId = w.ProjectId                       
   INNER JOIN BlockHistory Bh ON Bh.ClinicalCaseId = CC.ClinicalcaseId                         
   INNER JOIN BlockCategory Bc ON Bh.BlockCategoryId = Bc.BlockCategoryId                         
 WHERE CC.ProjectId = @ProjectID                     
 AND ((w.AssignedTo = @UserId AND ISNULL(W.IsBlocked, 0) = 1))  
 AND pu.UserId = @UserId AND pu.RoleId = 1 AND pu.IsActive = 1  
 ORDER BY CC.ClinicalCaseID                            
                    
END                    
ELSE IF @Role = 'QA' AND @ChartType = 'Available'                                      
BEGIN                                      
                                      
 ;WITH VersionCTE AS (                                      
 SELECT v.ClinicalCaseId, v.VersionId, v.VersionDate                                      
 , ROW_NUMBER() OVER(PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC) AS rn                                      
 FROM [Version] v                                      
 JOIN WorkItem wi                                      
 ON v.ClinicalCaseId = wi.ClinicalCaseId                 
 JOIN ProjectUser pu                                    
 ON pu.ProjectId = wi.ProjectId                              
 WHERE wi.ProjectId = @ProjectID AND pu.RoleId = 2                                    
 AND wi.StatusId IN (4, 5) AND v.StatusId = 3                                    
 )                                    
 SELECT TOP 1                       
  dbo.fn_GetUserName(AssignedTo) AS CodedBy                      
 ,dbo.fn_GetUserName(QABy) AS QABy                      
 ,dbo.fn_GetUserName(ShadowQABy) AS ShadowQABy,                                 
 cc.ClinicalCaseID       
 ,PatientMRN                                      
 ,PatientFirstName + ' ' + PatientLastName AS [Name]                                      
 ,CAST(DateOfService AS VARCHAR(10)) AS DateOfService                
 ,isnull(CC.ProviderId,0) ProviderId                                                         
 ,wp.PayorId                                      
 ,wp.ProviderFeedbackId                                      
 ,W.NoteTitle                             
 ,dx.DxCode                                      
 ,cp.CPTCode                       
 ,'' AS Modifier --Need to remove later              
 INTO #QAAvailable                                    
 FROM ClinicalCase cc                                      
 INNER JOIN WorkItem W                                      
 ON W.ClinicalCaseId = cc.ClinicalCaseId                        
 INNER JOIN WorkItemProvider wp                              
 ON wp.ClinicalCaseId =CC.ClinicalCaseId                                      
 INNER JOIN VersionCTE v                                      
 ON v.ClinicalCaseId = cc.ClinicalCaseId                                      
 INNER JOIN (                                      
  SELECT DISTINCT dx.ClinicalCaseId, dx.VersionId,                                      
  SUBSTRING(                                      
   (                                      
    SELECT ',' + dxc.DxCode  AS [text()]                                      
    FROM dbo.DxCode dxc                                      
    WHERE dx.ClinicalCaseId = dxc.ClinicalCaseId AND dx.VersionID = dxc.VersionID                                      
  ORDER BY dxc.ClinicalCaseId                              
    FOR XML PATH ('')                              
   ), 2, 1000) [DxCode]                                      
  FROM dbo.DxCode dx                                      
 ) dx                                      
 ON cc.ClinicalCaseId = dx.ClinicalCaseId AND dx.VersionId = v.VersionId                  
 INNER JOIN (                                      
  SELECT DISTINCT cp.ClinicalCaseId, cp.VersionId,                                      
  SUBSTRING(                                      
   (                                      
    SELECT '|' + cptc.cptCode + '^'+isnull(cptc.Modifier,'') + '^'+ISNULL(cptc.qty,'')+'^'+isnull(cptc.Links,'')   AS [text()]               
    FROM dbo.cptcode cptc                                      
    WHERE cp.ClinicalCaseId = cptc.ClinicalCaseId AND cp.VersionID = cptc.VersionID                                      
    ORDER BY cptc.ClinicalCaseId                                      
    FOR XML PATH ('')                              
   ), 2, 1000) [CptCode]                                      
  FROM dbo.cptcode cp                                      
 ) cp                                     
 ON cc.ClinicalCaseId = cp.ClinicalCaseId AND cp.VersionId = v.VersionId                                      
 --INNER JOIN CptCode cpt                                      
 --ON cc.ClinicalCaseId = cpt.ClinicalCaseId AND cpt.VersionId = v.VersionId                                    
 LEFT JOIN ProjectUser pu                                    
 ON pu.ProjectId = W.ProjectId                                    
 WHERE cc.ProjectId = @ProjectID AND ((W.StatusId = 5 AND w.QABy = @UserId AND ISNULL(W.IsBlocked, 0) = 0) OR W.StatusId = 4) AND pu.UserId = @UserId AND pu.RoleId = 2 AND pu.IsActive = 1                                    
 AND v.rn = 1                                      
 ORDER BY v.VersionDate                                      
                                      
 UPDATE WorkItem set StatusId = 5, QABy = @UserId, QADate = GETDATE() WHERE ClinicalCaseId = (                                    
 SELECT ClinicalCaseId FROM #QAAvailable                                    
 )                                    
                                     
 SELECT * FROM #QAAvailable                                    
                                      
END  
ELSE IF @Role = 'QA' AND @ChartType = 'Block'                                      
BEGIN                                      
                                      
 ;WITH VersionCTE AS (                                      
 SELECT v.ClinicalCaseId, v.VersionId, v.VersionDate                                      
 , ROW_NUMBER() OVER(PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC) AS rn                                      
 FROM [Version] v                                      
 JOIN WorkItem wi                                      
 ON v.ClinicalCaseId = wi.ClinicalCaseId                 
 JOIN ProjectUser pu                                    
 ON pu.ProjectId = wi.ProjectId                              
 WHERE wi.ProjectId = @ProjectID AND pu.RoleId = 2                                    
 AND wi.StatusId IN (4, 5) AND v.StatusId = 3                                    
 )                                    
 SELECT TOP 1                       
  dbo.fn_GetUserName(AssignedTo) AS CodedBy                      
 ,dbo.fn_GetUserName(QABy) AS QABy                      
 ,dbo.fn_GetUserName(ShadowQABy) AS ShadowQABy,                                 
 cc.ClinicalCaseID                                      
 ,PatientMRN                                      
 ,PatientFirstName + ' ' + PatientLastName AS [Name]                                      
 ,CAST(DateOfService AS VARCHAR(10)) AS DateOfService                
 ,isnull(CC.ProviderId,0) ProviderId                                                         
 ,wp.PayorId                                      
 ,wp.ProviderFeedbackId                                      
 ,W.NoteTitle                             
 ,dx.DxCode                                      
 ,cp.CPTCode                       
 ,'' AS Modifier --Need to remove later 
 ,bc.Name AS BlockCategory  
 ,bh.Remarks AS BlockRemarks  
 ,bh.CreateDate AS BlockedDate
 ,l.[Name] AS ListName              
 --INTO #QABlocked  
 FROM ClinicalCase cc                                      
 INNER JOIN WorkItem W                                      
 ON W.ClinicalCaseId = cc.ClinicalCaseId   
 LEFT JOIN List l ON l.ListId = CC.ListId 
 LEFT JOIN BlockHistory bh ON bh.ClinicalCaseId =CC.ClinicalCaseId  
 LEFT JOIN BlockCategory bc ON bc.BlockCategoryId =bh.BlockCategoryId                      
 INNER JOIN WorkItemProvider wp                              
 ON wp.ClinicalCaseId =CC.ClinicalCaseId                                      
 INNER JOIN VersionCTE v                                      
 ON v.ClinicalCaseId = cc.ClinicalCaseId                                      
 INNER JOIN (                                      
  SELECT DISTINCT dx.ClinicalCaseId, dx.VersionId,                                      
  SUBSTRING(                                      
   (                                      
    SELECT ',' + dxc.DxCode  AS [text()]                                      
    FROM dbo.DxCode dxc                                      
    WHERE dx.ClinicalCaseId = dxc.ClinicalCaseId AND dx.VersionID = dxc.VersionID                                      
  ORDER BY dxc.ClinicalCaseId                              
    FOR XML PATH ('')                              
   ), 2, 1000) [DxCode]                                      
  FROM dbo.DxCode dx                                      
 ) dx                                      
 ON cc.ClinicalCaseId = dx.ClinicalCaseId AND dx.VersionId = v.VersionId                  
 INNER JOIN (                                      
  SELECT DISTINCT cp.ClinicalCaseId, cp.VersionId,                                      
  SUBSTRING(                                      
   (                                      
    SELECT '|' + cptc.cptCode + '^'+isnull(cptc.Modifier,'') + '^'+ISNULL(cptc.qty,'')+'^'+isnull(cptc.Links,'')   AS [text()]               
    FROM dbo.cptcode cptc                                      
    WHERE cp.ClinicalCaseId = cptc.ClinicalCaseId AND cp.VersionID = cptc.VersionID                                      
    ORDER BY cptc.ClinicalCaseId                                      
    FOR XML PATH ('')                              
   ), 2, 1000) [CptCode]                                      
  FROM dbo.cptcode cp                                      
 ) cp                                     
 ON cc.ClinicalCaseId = cp.ClinicalCaseId AND cp.VersionId = v.VersionId                                      
 --INNER JOIN CptCode cpt                                      
 --ON cc.ClinicalCaseId = cpt.ClinicalCaseId AND cpt.VersionId = v.VersionId                                    
 LEFT JOIN ProjectUser pu                                    
 ON pu.ProjectId = W.ProjectId                                    
 WHERE cc.ProjectId = @ProjectID AND ((W.StatusId = 5 AND w.QABy = @UserId AND ISNULL(W.IsBlocked, 0) = 1)) AND pu.UserId = @UserId AND pu.RoleId = 2 AND pu.IsActive = 1                                    
 AND v.rn = 1                                      
 ORDER BY v.VersionDate                                      
                                      
 --UPDATE WorkItem set StatusId = 5, QABy = @UserId, QADate = GETDATE() WHERE ClinicalCaseId = (  
 --SELECT ClinicalCaseId FROM #QABlocked  
 --)  
                                     
 --SELECT * FROM #QABlocked  
                                      
END  
ELSE IF @Role = 'QA' AND @ChartType = 'RebuttalOfCoder'                                      
BEGIN                                      
 ;WITH QAVersionCTE AS (                                      
        SELECT v.ClinicalCaseId, v.VersionId, v.VersionDate, v.StatusId                                      
        , ROW_NUMBER() OVER(PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC) AS rn                                      
        FROM [Version] v                                      
        JOIN WorkItem wi                                    
        ON v.ClinicalCaseId = wi.ClinicalCaseId                                      
        WHERE wi.ProjectId = @ProjectID                                      
        AND wi.StatusId = 12                                     
  AND v.StatusId = 7                                   
    )                                      
 ,CoderVersionCTE AS (                          
        SELECT v.ClinicalCaseId, v.VersionId, v.VersionDate, v.StatusId                                      
        , ROW_NUMBER() OVER(PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC) AS rn                                      
        FROM [Version] v                                      
        JOIN WorkItem wi                                    
        ON v.ClinicalCaseId = wi.ClinicalCaseId                                      
        WHERE wi.ProjectId = @ProjectID                                      
        AND wi.StatusId = 12                                       
        AND v.StatusId = 3                                      
    )                                      
 ,RebuttedVersionCTE AS (                                    
        SELECT v.ClinicalCaseId, v.VersionId, v.VersionDate, v.StatusId                                      
        , ROW_NUMBER() OVER(PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC) AS rn                                      
        FROM [Version] v                                      
        JOIN WorkItem wi                                    
        ON v.ClinicalCaseId = wi.ClinicalCaseId                                      
        WHERE wi.ProjectId = @ProjectID AND wi.StatusId = 12                            
        AND v.StatusId = 12      
    )                                      
    ,AuditCTE AS (                                      
        SELECT wa.[ClinicalCaseId]                                      
        ,MAX(CASE WHEN FieldName = 'PayorId' THEN FieldValue END) AS QAPayorId                                      
            ,MAX(CASE WHEN FieldName = 'ProviderID' THEN FieldValue END) AS QAProviderID                                      
            ,MAX(CASE WHEN FieldName = 'CPTCode' THEN FieldValue END) AS QACPTCode                                      
            ,MAX(CASE WHEN FieldName = 'Mod' THEN FieldValue END) AS QAMod                                      
            ,MAX(CASE WHEN FieldName = 'Dx' THEN FieldValue END) AS QADx                                      
            ,MAX(CASE WHEN FieldName = 'ProviderFeedbackID' THEN FieldValue END) AS QAProviderFeedbackID                                      
            ,MAX(CASE WHEN FieldName = 'PayorId' THEN Remark END) AS QAPayorIdRemark                                      
            ,MAX(CASE WHEN FieldName = 'ProviderID' THEN Remark END) AS QAProviderIDRemark                                      
            ,MAX(CASE WHEN FieldName = 'CPTCode' THEN Remark END) AS QACPTCodeRemark                                      
   ,MAX(CASE WHEN FieldName = 'Mod' THEN Remark END) AS QAModRemark                                      
            ,MAX(CASE WHEN FieldName = 'Dx' THEN Remark END) AS QADxRemark                                      
            ,MAX(CASE WHEN FieldName = 'ProviderFeedbackID' THEN Remark END) AS QAProviderFeedbackIDRemark                                      
        FROM [dbo].[WorkItemAudit] wa                                      
        JOIN QAVersionCTE qv                                      
        ON qv.ClinicalCaseId = wa.ClinicalCaseId AND wa.VersionId = qv.VersionId                                      
        GROUP BY wa.ClinicalCaseId                                      
    )                                      
    ,RebuttedCTE AS (                                      
        SELECT wa.[ClinicalCaseId]                                      
            ,MAX(CASE WHEN FieldName = 'PayorId' THEN FieldValue END) AS RebuttedPayorId                                      
            ,MAX(CASE WHEN FieldName = 'ProviderID' THEN FieldValue END) AS RebuttedProviderID                                      
            ,MAX(CASE WHEN FieldName = 'CPTCode' THEN FieldValue END) AS RebuttedCPTCode                                      
            ,MAX(CASE WHEN FieldName = 'Mod' THEN FieldValue END) AS RebuttedMod                                      
            ,MAX(CASE WHEN FieldName = 'Dx' THEN FieldValue END) AS RebuttedDx        
            ,MAX(CASE WHEN FieldName = 'ProviderFeedbackID' THEN FieldValue END) AS RebuttedProviderFeedbackID                                      
            ,MAX(CASE WHEN FieldName = 'PayorId' THEN Remark END) AS RebuttedPayorIdRemark                                      
            ,MAX(CASE WHEN FieldName = 'ProviderID' THEN Remark END) AS RebuttedProviderIDRemark                                      
            ,MAX(CASE WHEN FieldName = 'CPTCode' THEN Remark END) AS RebuttedCPTCodeRemark                                      
   ,MAX(CASE WHEN FieldName = 'Mod' THEN Remark END) AS RebuttedModRemark                                      
            ,MAX(CASE WHEN FieldName = 'Dx' THEN Remark END) AS RebuttedDxRemark                                      
            ,MAX(CASE WHEN FieldName = 'ProviderFeedbackID' THEN Remark END) AS RebuttedProviderFeedbackIDRemark                       
        FROM [dbo].[WorkItemAudit] wa                                      
        JOIN RebuttedVersionCTE rv                                      
        ON rv.ClinicalCaseId = wa.ClinicalCaseId AND wa.VersionId = rv.VersionId                                      
        GROUP BY wa.ClinicalCaseId                                      
    )                                      
    SELECT TOP 1                                      
    cc.ClinicalCaseID                                      
    ,PatientMRN                                      
    ,PatientFirstName + ' ' + PatientLastName AS [Name]                                      
    ,CAST(DateOfService AS VARCHAR(10)) AS DateOfService                
 ,isnull(isnull(CC.ProviderId,0),0)              
  ,dbo.fn_GetUserName(AssignedTo) AS CodedBy                      
 ,dbo.fn_GetUserName(QABy) AS QABy                      
 ,dbo.fn_GetUserName(ShadowQABy) AS ShadowQABy                               
    ,wp.ProviderId                                      
    ,wp.PayorId                                      
    ,wp.ProviderFeedbackId                                      
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
    FROM ClinicalCase cc                                      
    INNER JOIN WorkItem wi                                      
    ON wi.ClinicalCaseId = cc.ClinicalCaseId                                      
    INNER JOIN WorkItemProvider wp                                      
    ON wp.ClinicalCaseId =CC.ClinicalCaseId                                      
    LEFT JOIN QAVersionCTE qv                                      
    ON qv.ClinicalCaseId = cc.ClinicalCaseId                                
 LEFT JOIN RebuttedVersionCTE cv                                      
    ON cv.ClinicalCaseId = cc.ClinicalCaseId                                      
    --LEFT JOIN DxCode dx                                      
    --ON cc.ClinicalCaseId = dx.ClinicalCaseId AND dx.VersionId = cv.VersionId              
 INNER JOIN (                                      
  SELECT DISTINCT dx.ClinicalCaseId, dx.VersionId,                                      
  SUBSTRING(                                      
   (                                      
    SELECT ',' + dxc.DxCode  AS [text()]                                      
    FROM dbo.DxCode dxc                                      
    WHERE dx.ClinicalCaseId = dxc.ClinicalCaseId AND dxc.VersionId = dx.VersionId                          
    ORDER BY dxc.ClinicalCaseId                                      
    FOR XML PATH ('')                                      
   ), 2, 1000) [DxCode]                                      
  FROM dbo.DxCode dx                                      
 ) dx on dx.ClinicalCaseId = wp.ClinicalCaseId AND dx.VersionID = cv.VersionID        
 LEFT JOIN   
 (                                      
  SELECT DISTINCT cp.ClinicalCaseId, cp.VersionId,                                      
  SUBSTRING(                                      
   (                                      
    SELECT '|' + cptc.cptCode + '^'+isnull(cptc.Modifier,'') + '^'+ISNULL(cptc.qty,'')+'^'+isnull(cptc.Links,'')   AS [text()]               
    FROM dbo.cptcode cptc                                      
    WHERE cp.ClinicalCaseId = cptc.ClinicalCaseId AND cp.VersionID = cptc.VersionID                                      
    ORDER BY cptc.ClinicalCaseId                                      
    FOR XML PATH ('')                              
   ), 2, 1000) [CptCode]                                      
  FROM dbo.cptcode cp                                      
 ) cpt                                      
    ON cc.ClinicalCaseId = cpt.ClinicalCaseId AND cpt.VersionId = cv.VersionId                                      
    LEFT JOIN AuditCTE a                                      
    ON a.ClinicalCaseId = cc.ClinicalCaseId                                      
 LEFT JOIN RebuttedCTE r                           
    ON r.ClinicalCaseId = cc.ClinicalCaseId                                    
 LEFT JOIN ProjectUser pu                                    
 ON pu.ProjectId = wi.ProjectId                                    
    WHERE cc.ProjectId = @ProjectID AND wi.StatusId = 12 AND pu.UserId = @UserId AND pu.RoleId = 2 AND pu.IsActive = 1                                    
    AND qv.rn = 1      
    ORDER BY qv.VersionDate                                      
END                                      
ELSE IF @Role = 'QA' AND @ChartType = 'ShadowQARejected' --RebuttalOfShadowQA                                      
BEGIN                                      
 ;WITH QAVersionCTE AS (                                      
        SELECT v.ClinicalCaseId, v.VersionId, v.VersionDate, v.StatusId                                      
        , ROW_NUMBER() OVER(PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC) AS rn                                      
        FROM [Version] v                                      
        JOIN WorkItem wi                                    
 ON v.ClinicalCaseId = wi.ClinicalCaseId                                      
        WHERE wi.ProjectId = @ProjectID                                      
  AND wi.StatusId = 11                                    
        AND v.StatusId IN (6, 7)                      
    )                      
 ,CoderVersionCTE AS (                                      
   SELECT v.ClinicalCaseId, v.VersionId, v.VersionDate, v.StatusId                                      
        , ROW_NUMBER() OVER(PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC) AS rn                                      
        FROM [Version] v                                      
        JOIN WorkItem wi                                    
        ON v.ClinicalCaseId = wi.ClinicalCaseId                                      
        WHERE wi.ProjectId = @ProjectID                                     
  AND wi.StatusId = 11                                    
        AND v.StatusId = 3                                      
    ),ShadowQAVersionCTE AS (                                      
        SELECT v.ClinicalCaseId, v.VersionId, v.VersionDate, v.StatusId                                      
        , ROW_NUMBER() OVER(PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC) AS rn                                      
        FROM [Version] v                               
        JOIN WorkItem wi                                    
        ON v.ClinicalCaseId = wi.ClinicalCaseId                                      
        WHERE wi.ProjectId = @ProjectID                                      
  AND wi.StatusId = 11                                    
        AND v.StatusId = 11                                      
    )                                    
 ,RebuttedVersionCTE AS (                                        
        SELECT v.ClinicalCaseId, v.VersionId, v.VersionDate, v.StatusId                                        
        , ROW_NUMBER() OVER(PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC) AS rn                                        
        FROM [Version] v                                        
        JOIN WorkItem wi                                    
        ON v.ClinicalCaseId = wi.ClinicalCaseId                                        
        WHERE wi.ProjectId = @ProjectID                                    
  AND wi.StatusId = 11                                    
        AND v.StatusId = 13                                      
    )                                        
 ,RebuttedCTE AS (                                        
        SELECT wa.[ClinicalCaseId]                                        
,MAX(CASE WHEN FieldName = 'PayorId' THEN FieldValue END) AS RebuttedPayorId                                        
            ,MAX(CASE WHEN FieldName = 'ProviderID' THEN FieldValue END) AS RebuttedProviderID                                        
            ,MAX(CASE WHEN FieldName = 'CPTCode' THEN FieldValue END) AS RebuttedCPTCode                                        
            ,MAX(CASE WHEN FieldName = 'Mod' THEN FieldValue END) AS RebuttedMod                                        
            ,MAX(CASE WHEN FieldName = 'Dx' THEN FieldValue END) AS RebuttedDx                                        
            ,MAX(CASE WHEN FieldName = 'ProviderFeedbackID' THEN FieldValue END) AS RebuttedProviderFeedbackID                                        
            ,MAX(CASE WHEN FieldName = 'PayorId' THEN Remark END) AS RebuttedPayorIdRemark                                        
            ,MAX(CASE WHEN FieldName = 'ProviderID' THEN Remark END) AS RebuttedProviderIDRemark                                        
            ,MAX(CASE WHEN FieldName = 'CPTCode' THEN Remark END) AS RebuttedCPTCodeRemark                                        
            ,MAX(CASE WHEN FieldName = 'Mod' THEN Remark END) AS RebuttedModRemark                                        
            ,MAX(CASE WHEN FieldName = 'Dx' THEN Remark END) AS RebuttedDxRemark                                        
            ,MAX(CASE WHEN FieldName = 'ProviderFeedbackID' THEN Remark END) AS RebuttedProviderFeedbackIDRemark                                        
        FROM [dbo].[WorkItemAudit] wa                                        
 JOIN RebuttedVersionCTE rv                                        
        ON rv.ClinicalCaseId = wa.ClinicalCaseId AND wa.VersionId = rv.VersionId                                        
        GROUP BY wa.ClinicalCaseId )                      
    ,AuditCTE AS (                                      
        SELECT wa.[ClinicalCaseId]                                      
            ,MAX(CASE WHEN FieldName = 'PayorId' THEN FieldValue END) AS QAPayorId                   
            ,MAX(CASE WHEN FieldName = 'ProviderID' THEN FieldValue END) AS QAProviderID                               
            ,MAX(CASE WHEN FieldName = 'CPTCode' THEN FieldValue END) AS QACPTCode                                      
            ,MAX(CASE WHEN FieldName = 'Mod' THEN FieldValue END) AS QAMod                                      
            ,MAX(CASE WHEN FieldName = 'Dx' THEN FieldValue END) AS QADx                                      
            ,MAX(CASE WHEN FieldName = 'ProviderFeedbackID' THEN FieldValue END) AS QAProviderFeedbackID                                      
            ,MAX(CASE WHEN FieldName = 'PayorId' THEN Remark END) AS QAPayorIdRemark                                      
            ,MAX(CASE WHEN FieldName = 'ProviderID' THEN Remark END) AS QAProviderIDRemark               
            ,MAX(CASE WHEN FieldName = 'CPTCode' THEN Remark END) AS QACPTCodeRemark                                      
   ,MAX(CASE WHEN FieldName = 'Mod' THEN Remark END) AS QAModRemark                                      
            ,MAX(CASE WHEN FieldName = 'Dx' THEN Remark END) AS QADxRemark                                      
            ,MAX(CASE WHEN FieldName = 'ProviderFeedbackID' THEN Remark END) AS QAProviderFeedbackIDRemark                      
   ,et.[Name] AS QAErrorType                      
        FROM [dbo].[WorkItemAudit] wa                                      
        JOIN QAVersionCTE qv                       
        ON qv.ClinicalCaseId = wa.ClinicalCaseId AND wa.VersionId = qv.VersionId                      
  LEFT JOIN ErrorType et                      
  ON et.ErrorTypeId = wa.ErrorTypeId                      
        GROUP BY wa.ClinicalCaseId, et.[Name]                                      
    )                      
 ,ShadowQAAuditCTE AS (                                      
        SELECT wa.[ClinicalCaseId]                              
            ,MAX(CASE WHEN FieldName = 'PayorId' THEN FieldValue END) AS ShadowQAPayorId                                      
            ,MAX(CASE WHEN FieldName = 'ProviderID' THEN FieldValue END) AS ShadowQAProviderID                                      
            ,MAX(CASE WHEN FieldName = 'CPTCode' THEN FieldValue END) AS ShadowQACPTCode                                      
            ,MAX(CASE WHEN FieldName = 'Mod' THEN FieldValue END) AS ShadowQAMod                                      
            ,MAX(CASE WHEN FieldName = 'Dx' THEN FieldValue END) AS ShadowQADx                                      
            ,MAX(CASE WHEN FieldName = 'ProviderFeedbackID' THEN FieldValue END) AS ShadowQAProviderFeedbackID                                      
            ,MAX(CASE WHEN FieldName = 'PayorId' THEN Remark END) AS ShadowQAPayorIdRemark                                      
            ,MAX(CASE WHEN FieldName = 'ProviderID' THEN Remark END) AS ShadowQAProviderIDRemark                                      
            ,MAX(CASE WHEN FieldName = 'CPTCode' THEN Remark END) AS ShadowQACPTCodeRemark                                      
   ,MAX(CASE WHEN FieldName = 'Mod' THEN Remark END) AS ShadowQAModRemark                                      
            ,MAX(CASE WHEN FieldName = 'Dx' THEN Remark END) AS ShadowQADxRemark                                      
            ,MAX(CASE WHEN FieldName = 'ProviderFeedbackID' THEN Remark END) AS ShadowQAProviderFeedbackIDRemark                      
   ,et.[Name] AS ShadowQAErrorType                      
        FROM [dbo].[WorkItemAudit] wa                                      
        JOIN ShadowQAVersionCTE qv                                      
        ON qv.ClinicalCaseId = wa.ClinicalCaseId AND wa.VersionId = qv.VersionId                      
  LEFT JOIN ErrorType et                      
  ON et.ErrorTypeId = wa.ErrorTypeId                      
        GROUP BY wa.ClinicalCaseId, et.[Name]                      
    )                        
    SELECT TOP 1                                      
    cc.ClinicalCaseID                                      
    ,PatientMRN                                      
    ,PatientFirstName + ' ' + PatientLastName AS [Name]                                      
    ,CAST(DateOfService AS VARCHAR(10)) AS DateOfService                
 ,isnull(CC.ProviderId,0)                       
 ,dbo.fn_GetUserName(AssignedTo) AS CodedBy                      
    ,dbo.fn_GetUserName(QABy) AS QABy                      
    ,dbo.fn_GetUserName(ShadowQABy) AS ShadowQABy                                    
    ,wp.ProviderId                        
 ,p.Name AS ProviderText                                            
    ,wp.PayorId                       
 ,pa.Name AS PayorText                                     
    ,wp.ProviderFeedbackId                        
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
                      
 ,QAErrorType                      
 ,ShadowQAErrorType                      
                      
    FROM ClinicalCase cc                                      
    INNER JOIN WorkItem wi                                      
    ON wi.ClinicalCaseId = cc.ClinicalCaseId                          
    INNER JOIN WorkItemProvider wp                                      
    ON wp.ClinicalCaseId =CC.ClinicalCaseId                       
 LEFT JOIN [Provider] P                       
 ON wp.ProviderId = P.ProviderID                      
 LEFT JOIN ProviderFeedback Pf                      
 ON wp.ProviderFeedbackId = Pf.ProviderFeedbackId                      
 LEFT JOIN Payor Pa                       
 ON wp.PayorId = pa.PayorId                                       
    LEFT JOIN QAVersionCTE qv                                      
    ON qv.ClinicalCaseId = cc.ClinicalCaseId                         
 LEFT JOIN CoderVersionCTE cv                                      
    ON cv.ClinicalCaseId = cc.ClinicalCaseId                                      
    --LEFT JOIN DxCode dx                                      
    --ON cc.ClinicalCaseId = dx.ClinicalCaseId AND dx.VersionId = cv.VersionId              
 INNER JOIN (                                      
  SELECT DISTINCT dx.ClinicalCaseId, dx.VersionId,                                      
  SUBSTRING(                                      
   (                                      
    SELECT ',' + dxc.DxCode  AS [text()]                                      
    FROM dbo.DxCode dxc                                      
    WHERE dx.ClinicalCaseId = dxc.ClinicalCaseId AND dxc.VersionId = dx.VersionId                          
    ORDER BY dxc.ClinicalCaseId                                      
    FOR XML PATH ('')                                      
   ), 2, 1000) [DxCode]                                      
  FROM dbo.DxCode dx                                      
 ) dx on dx.ClinicalCaseId = wp.ClinicalCaseId AND dx.VersionID = cv.VersionID              
    LEFT JOIN        
 (        
  SELECT DISTINCT cp.ClinicalCaseId, cp.VersionId,                                      
  SUBSTRING(                                      
   (                                      
    SELECT '|' + cptc.cptCode + '^'+isnull(cptc.Modifier,'') + '^'+ISNULL(cptc.qty,'')+'^'+isnull(cptc.Links,'')   AS [text()]               
    FROM dbo.cptcode cptc                                      
    WHERE cp.ClinicalCaseId = cptc.ClinicalCaseId AND cp.VersionID = cptc.VersionID                                      
    ORDER BY cptc.ClinicalCaseId                                      
    FOR XML PATH ('')                              
   ), 2, 1000) [CptCode]                                      
  FROM dbo.cptcode cp                                      
 ) cp            
    ON cc.ClinicalCaseId = cp.ClinicalCaseId AND cp.VersionId = cv.VersionId                                      
    LEFT JOIN AuditCTE a                                      
    ON a.ClinicalCaseId = cc.ClinicalCaseId                         
 LEFT JOIN [Provider] QAP                       
 ON a.QAProviderID = QAP.ProviderID                      
 LEFT JOIN ProviderFeedback QAPf                      
 ON a.QAProviderFeedbackID = QAPf.ProviderFeedbackId                      
 LEFT JOIN Payor QAPa                       
 ON QAPa.PayorId = a.QAPayorId                      
                                       
    LEFT JOIN ShadowQAAuditCTE sa                                      
    ON sa.ClinicalCaseId = cc.ClinicalCaseId                         
 LEFT JOIN [Provider] SQAP                       
 ON sa.ShadowQAProviderID = SQAP.ProviderID                      
 LEFT JOIN ProviderFeedback SQAPf                      
 ON sa.ShadowQAProviderFeedbackID = SQAPf.ProviderFeedbackId                      
 LEFT JOIN Payor SQAPa                       
 ON SQAPa.PayorId = sa.ShadowQAPayorId                      
 LEFT JOIN RebuttedCTE r                                        
 ON r.ClinicalCaseId = cc.ClinicalCaseId                                    
 JOIN ProjectUser pu                                    
 ON pu.ProjectId = wi.ProjectId                                    
    WHERE cc.ProjectId = @ProjectID AND wi.StatusId = 11 AND pu.UserId = @UserId AND pu.RoleId = 2 AND pu.IsActive = 1                                     
    AND qv.rn = 1                                      
    ORDER BY qv.VersionDate                                      
                                      
END                                      
ELSE IF @Role = 'QA' AND @ChartType  = 'IsBlocked'                                      
BEGIN                                      
                                    
;WITH VersionCTE AS (                                      
 SELECT v.ClinicalCaseId, v.VersionId, v.VersionDate                                      
 , ROW_NUMBER() OVER(PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC) AS rn                                      
 FROM [Version] v                                      
 JOIN WorkItem wi                                      
 ON v.ClinicalCaseId = wi.ClinicalCaseId                                      
 WHERE wi.ProjectId = @ProjectID                                  
 AND wi.StatusId = 2 AND wi.IsBlocked = 1                          
 --AND v.StatusId = 13                                      
 )                                      
 SELECT TOP 1                         
  dbo.fn_GetUserName(AssignedTo) AS CodedBy                      
 ,dbo.fn_GetUserName(QABy) AS QABy                      
 ,dbo.fn_GetUserName(ShadowQABy) AS ShadowQABy,                               
 cc.ClinicalCaseID                                      
 ,PatientMRN                                     ,PatientFirstName + ' ' + PatientLastName AS [Name]                                      
 ,CAST(DateOfService AS VARCHAR(10)) AS DateOfService                
 ,isnull(CC.ProviderId,0)                                      
 ,wp.PayorId                                      
 ,wp.ProviderFeedbackId                                      
 ,wi.NoteTitle                                      
 ,dx.DxCode                                      
 ,cpt.CPTCode                                      
 ,cpt.Modifier                                      
 ,Cq.Question                                      
 FROM ClinicalCase cc                                      
INNER JOIN WorkItem wi                                      
 ON wi.ClinicalCaseId = cc.ClinicalCaseId                          
 INNER JOIN CoderQuestion Cq                                      
 ON Cq.ClinicalCaseId = CC.ClinicalCaseId                                     
 LEFT JOIN WorkItemProvider wp                                      
 ON wp.ClinicalCaseId =CC.ClinicalCaseId                                      
 LEFT JOIN VersionCTE v                                      
 ON v.ClinicalCaseId = cc.ClinicalCaseId                                        
 LEFT JOIN DxCode dx                                      
 ON cc.ClinicalCaseId = dx.ClinicalCaseId AND dx.VersionId = v.VersionId                                      
 LEFT JOIN CptCode cpt                                      
 ON cc.ClinicalCaseId = cpt.ClinicalCaseId AND cpt.VersionId = v.VersionId                              
 JOIN ProjectUser pu                                    
 ON pu.ProjectId = wi.ProjectId                                    
 WHERE cc.ProjectId = @ProjectID AND wi.StatusId = 2 AND wi.IsBlocked = 1 AND pu.UserId = @UserId AND pu.RoleId = 2 AND pu.IsActive = 1                                    
 AND v.rn = 1                                      
 ORDER BY v.VersionDate                                      
                                    
END                                      
ELSE IF @Role = 'ShadowQA' AND @ChartType IN ('RebuttalOfQA', 'RebuttalOfCoder')                                      
BEGIN                                      
 ;WITH QAVersionCTE AS (                                      
        SELECT v.ClinicalCaseId, v.VersionId, v.VersionDate, v.StatusId                                      
        , ROW_NUMBER() OVER(PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC) AS rn                                      
        FROM [Version] v                                      
        JOIN WorkItem wi                                    
        ON v.ClinicalCaseId = wi.ClinicalCaseId                                      
        WHERE wi.ProjectId = @ProjectID                                    
  AND wi.StatusId = 13                                    
        AND v.StatusId = 11                                      
    )                                      
 ,CoderVersionCTE AS (                                      
        SELECT v.ClinicalCaseId, v.VersionId, v.VersionDate, v.StatusId                                      
        , ROW_NUMBER() OVER(PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC) AS rn                                      
        FROM [Version] v                                      
        JOIN WorkItem wi                                    
        ON v.ClinicalCaseId = wi.ClinicalCaseId                                      
        WHERE wi.ProjectId = @ProjectID                           
  AND wi.StatusId = 13                                    
        AND v.StatusId = 3                                      
    )                                      
 ,RebuttedVersionCTE AS (                                      
        SELECT v.ClinicalCaseId, v.VersionId, v.VersionDate, v.StatusId                                      
        , ROW_NUMBER() OVER(PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC) AS rn                                   
        FROM [Version] v                                      
        JOIN WorkItem wi                                    
        ON v.ClinicalCaseId = wi.ClinicalCaseId                                      
        WHERE wi.ProjectId = @ProjectID                                    
  AND wi.StatusId = 13                                    
        AND v.StatusId = 13                                      
    )                                      
    ,AuditCTE AS (                                      
        SELECT wa.[ClinicalCaseId]                                      
            ,MAX(CASE WHEN FieldName = 'PayorId' THEN FieldValue END) AS QAPayorId    
            ,MAX(CASE WHEN FieldName = 'ProviderID' THEN FieldValue END) AS QAProviderID                                      
            ,MAX(CASE WHEN FieldName = 'CPTCode' THEN FieldValue END) AS QACPTCode                                      
            ,MAX(CASE WHEN FieldName = 'Mod' THEN FieldValue END) AS QAMod                                      
,MAX(CASE WHEN FieldName = 'Dx' THEN FieldValue END) AS QADx                      
            ,MAX(CASE WHEN FieldName = 'ProviderFeedbackID' THEN FieldValue END) AS QAProviderFeedbackID                     
            ,MAX(CASE WHEN FieldName = 'PayorId' THEN Remark END) AS QAPayorIdRemark                                      
            ,MAX(CASE WHEN FieldName = 'ProviderID' THEN Remark END) AS QAProviderIDRemark                                      
            ,MAX(CASE WHEN FieldName = 'CPTCode' THEN Remark END) AS QACPTCodeRemark                                      
   ,MAX(CASE WHEN FieldName = 'Mod' THEN Remark END) AS QAModRemark                                      
            ,MAX(CASE WHEN FieldName = 'Dx' THEN Remark END) AS QADxRemark                                      
            ,MAX(CASE WHEN FieldName = 'ProviderFeedbackID' THEN Remark END) AS QAProviderFeedbackIDRemark                                      
        FROM [dbo].[WorkItemAudit] wa                                      
        JOIN QAVersionCTE qv                                      
        ON qv.ClinicalCaseId = wa.ClinicalCaseId AND wa.VersionId = qv.VersionId                                      
        GROUP BY wa.ClinicalCaseId                                      
    )                                      
    ,RebuttedCTE AS (                                      
        SELECT wa.[ClinicalCaseId]                                      
            ,MAX(CASE WHEN FieldName = 'PayorId' THEN FieldValue END) AS RebuttedPayorId                                      
            ,MAX(CASE WHEN FieldName = 'ProviderID' THEN FieldValue END) AS RebuttedProviderID                                      
            ,MAX(CASE WHEN FieldName = 'CPTCode' THEN FieldValue END) AS RebuttedCPTCode                                      
            ,MAX(CASE WHEN FieldName = 'Mod' THEN FieldValue END) AS RebuttedMod                                      
            ,MAX(CASE WHEN FieldName = 'Dx' THEN FieldValue END) AS RebuttedDx                                      
            ,MAX(CASE WHEN FieldName = 'ProviderFeedbackID' THEN FieldValue END) AS RebuttedProviderFeedbackID                                      
         ,MAX(CASE WHEN FieldName = 'PayorId' THEN Remark END) AS RebuttedPayorIdRemark                                      
            ,MAX(CASE WHEN FieldName = 'ProviderID' THEN Remark END) AS RebuttedProviderIDRemark                                      
            ,MAX(CASE WHEN FieldName = 'CPTCode' THEN Remark END) AS RebuttedCPTCodeRemark                                      
   ,MAX(CASE WHEN FieldName = 'Mod' THEN Remark END) AS RebuttedModRemark                                      
            ,MAX(CASE WHEN FieldName = 'Dx' THEN Remark END) AS RebuttedDxRemark                                      
            ,MAX(CASE WHEN FieldName = 'ProviderFeedbackID' THEN Remark END) AS RebuttedProviderFeedbackIDRemark                                      
      FROM [dbo].[WorkItemAudit] wa                                      
        JOIN RebuttedVersionCTE rv                                      
        ON rv.ClinicalCaseId = wa.ClinicalCaseId AND wa.VersionId = rv.VersionId                                      
        GROUP BY wa.ClinicalCaseId                             
    )                                      
    SELECT TOP 1                                      
    cc.ClinicalCaseID                                      
    ,PatientMRN                                      
    ,PatientFirstName + ' ' + PatientLastName AS [Name]                                      
    ,CAST(DateOfService AS VARCHAR(10)) AS DateOfService                 
 ,isnull(CC.ProviderId,0)                    
  ,dbo.fn_GetUserName(AssignedTo) AS CodedBy                      
 ,dbo.fn_GetUserName(QABy) AS QABy                      
 ,dbo.fn_GetUserName(ShadowQABy) AS ShadowQABy                                                               
    ,wp.PayorId,    
  wp.ProviderId                                      
    ,wp.ProviderFeedbackId                                      
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
    FROM ClinicalCase cc                                      
    INNER JOIN WorkItem wi                                      
    ON wi.ClinicalCaseId = cc.ClinicalCaseId                                      
    INNER JOIN WorkItemProvider wp                                      
    ON wp.ClinicalCaseId =CC.ClinicalCaseId                                      
    LEFT JOIN QAVersionCTE qv                                      
    ON qv.ClinicalCaseId = cc.ClinicalCaseId                                      
 LEFT JOIN CoderVersionCTE cv                                      
    ON cv.ClinicalCaseId = cc.ClinicalCaseId                                      
    LEFT JOIN     
  (                    
  SELECT DISTINCT dx.ClinicalCaseId, dx.VersionId,                                      
  SUBSTRING(                                      
   (                                      
    SELECT ',' + dxc.DxCode  AS [text()]                                      
    FROM dbo.DxCode dxc                                      
    WHERE dx.ClinicalCaseId = dxc.ClinicalCaseId AND dxc.VersionId = dx.VersionId                          
    ORDER BY dxc.ClinicalCaseId                                      
    FOR XML PATH ('')                                      
   ), 2, 1000) [DxCode]                                      
  FROM dbo.DxCode dx                                      
 ) dx  ON cc.ClinicalCaseId = dx.ClinicalCaseId AND dx.VersionId = cv.VersionId                                      
   LEFT JOIN     
   (                                      
  SELECT DISTINCT cp.ClinicalCaseId, cp.VersionId,                                      
  SUBSTRING(                                      
   (                                    
    SELECT '|' + cptc.cptCode + '^'+isnull(cptc.Modifier,'') + '^'+ISNULL(cptc.qty,'')+'^'+isnull(cptc.Links,'')   AS [text()]               
    FROM dbo.cptcode cptc                                      
    WHERE cp.ClinicalCaseId = cptc.ClinicalCaseId AND cp.VersionID = cptc.VersionID                                      
    ORDER BY cptc.ClinicalCaseId                                      
    FOR XML PATH ('')                              
   ), 2, 1000) [CptCode]                                      
  FROM dbo.cptcode cp                                      
 ) cpt                                      
    ON cc.ClinicalCaseId = cpt.ClinicalCaseId AND cpt.VersionId = cv.VersionId                                      
    LEFT JOIN AuditCTE a                                      
    ON a.ClinicalCaseId = cc.ClinicalCaseId                                      
 LEFT JOIN RebuttedCTE r                                      
    ON r.ClinicalCaseId = cc.ClinicalCaseId                                      
 JOIN ProjectUser pu                                    
 ON pu.ProjectId = wi.ProjectId                                      
    WHERE cc.ProjectId = @ProjectID AND wi.StatusId = 13 AND pu.UserId = @UserId AND pu.RoleId = 3 AND pu.IsActive = 1                                    
    AND qv.rn = 1                                      
    ORDER BY qv.VersionDate                                      
END    
ELSE IF @Role = 'ShadowQA' AND @ChartType = 'Available'                                      
BEGIN                                      
                   
 ;WITH QAVersionCTE AS (                                      
        SELECT v.ClinicalCaseId, v.VersionId, v.VersionDate, v.StatusId                                      
        , ROW_NUMBER() OVER(PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC) AS rn                 
    FROM [Version] v                                      
        JOIN WorkItem wi                                    
        ON v.ClinicalCaseId = wi.ClinicalCaseId                                      
        WHERE wi.ProjectId = @ProjectID                            
  AND wi.StatusId IN (8, 9)                          
        AND v.StatusId IN (7, 8)                            
    )                                      
 ,CoderVersionCTE AS (                                      
        SELECT v.ClinicalCaseId, v.VersionId, v.VersionDate, v.StatusId                                      
        , ROW_NUMBER() OVER(PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC) AS rn                                      
        FROM [Version] v                                      
        JOIN WorkItem wi                                    
        ON v.ClinicalCaseId = wi.ClinicalCaseId                             
        WHERE wi.ProjectId = @ProjectID                            
  AND wi.StatusId IN (8, 9)                          
        AND v.StatusId IN (3, 12)                            
    ) ,QARemarksCTE AS (                                      
        SELECT wa.[ClinicalCaseId]                                      
            ,MAX(CASE WHEN FieldName = 'PayorId' THEN FieldValue END) AS QAPayorId                                      
            ,MAX(CASE WHEN FieldName = 'ProviderID' THEN FieldValue END) AS QAProviderID                                      
            ,MAX(CASE WHEN FieldName = 'CPTCode' THEN FieldValue END) AS QACPTCode                           
            ,MAX(CASE WHEN FieldName = 'Mod' THEN FieldValue END) AS QAMod                                      
            ,MAX(CASE WHEN FieldName = 'Dx' THEN FieldValue END) AS QADx                                      
            ,MAX(CASE WHEN FieldName = 'ProviderFeedbackID' THEN FieldValue END) AS QAProviderFeedbackID                                      
            ,MAX(CASE WHEN FieldName = 'PayorId' THEN Remark END) AS QAPayorIdRemark                                      
            ,MAX(CASE WHEN FieldName = 'ProviderID' THEN Remark END) AS QAProviderIDRemark                     
            ,MAX(CASE WHEN FieldName = 'CPTCode' THEN Remark END) AS QACPTCodeRemark                                      
   ,MAX(CASE WHEN FieldName = 'Mod' THEN Remark END) AS QAModRemark                                      
            ,MAX(CASE WHEN FieldName = 'Dx' THEN Remark END) AS QADxRemark                                      
            ,MAX(CASE WHEN FieldName = 'ProviderFeedbackID' THEN Remark END) AS QAProviderFeedbackIDRemark                                      
        FROM [dbo].[WorkItemAudit] wa                                      
        JOIN QAVersionCTE qv                                      
        ON qv.ClinicalCaseId = wa.ClinicalCaseId AND wa.VersionId = qv.VersionId                                      
        GROUP BY wa.ClinicalCaseId                                      
    )                             
 --,CoderRemarksCTE AS (                                      
 --       SELECT wa.[ClinicalCaseId]                                      
 --           ,MAX(CASE WHEN FieldName = 'PayorId' THEN FieldValue END) AS PayorId                                      
 --           ,MAX(CASE WHEN FieldName = 'ProviderID' THEN FieldValue END) AS ProviderID                                      
 --           ,MAX(CASE WHEN FieldName = 'CPTCode' THEN FieldValue END) AS CPTCode                                      
 --           ,MAX(CASE WHEN FieldName = 'Mod' THEN FieldValue END) AS Mod                                      
 --           ,MAX(CASE WHEN FieldName = 'Dx' THEN FieldValue END) AS Dx                                      
 --           ,MAX(CASE WHEN FieldName = 'ProviderFeedbackID' THEN FieldValue END) AS ProviderFeedbackID                                      
 --           ,MAX(CASE WHEN FieldName = 'PayorId' THEN Remark END) AS PayorIdRemark                                      
 --           ,MAX(CASE WHEN FieldName = 'ProviderID' THEN Remark END) AS ProviderIDRemark                                      
 --           ,MAX(CASE WHEN FieldName = 'CPTCode' THEN Remark END) AS CPTCodeRemark                                      
 --  ,MAX(CASE WHEN FieldName = 'Mod' THEN Remark END) AS ModRemark                                      
 --           ,MAX(CASE WHEN FieldName = 'Dx' THEN Remark END) AS DxRemark                                      
 --           ,MAX(CASE WHEN FieldName = 'ProviderFeedbackID' THEN Remark END) AS ProviderFeedbackIDRemark                                      
 --       FROM [dbo].[WorkItemAudit] wa                                      
 --       JOIN CoderVersionCTE cv                                      
 --       ON cv.ClinicalCaseId = wa.ClinicalCaseId AND wa.VersionId = cv.VersionId                            
 --       GROUP BY wa.ClinicalCaseId                                      
 --   )                                      
    SELECT TOP 1                               
    cc.ClinicalCaseID                                      
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
 INTO #ShadowQAAvailable                                   
    FROM ClinicalCase cc INNER JOIN WorkItem wi ON wi.ClinicalCaseId = cc.ClinicalCaseId                                      
    LEFT JOIN WorkItemProvider wp ON wp.ClinicalCaseId =CC.ClinicalCaseId                                      
    LEFT JOIN QAVersionCTE qv ON qv.ClinicalCaseId = cc.ClinicalCaseId                                      
 LEFT JOIN CoderVersionCTE cv ON cv.ClinicalCaseId = cc.ClinicalCaseId                                      
    LEFT JOIN (                                      
SELECT DISTINCT dx.ClinicalCaseId, dx.VersionId,                                      
  SUBSTRING(                                      
   (                                      
    SELECT ',' + dxc.DxCode  AS [text()]                                      
    FROM dbo.DxCode dxc                                      
    WHERE dx.ClinicalCaseId = dxc.ClinicalCaseId AND dx.VersionID = dxc.VersionID                                      
    ORDER BY dxc.ClinicalCaseId                                      
    FOR XML PATH ('')                              
   ), 2, 1000) [DxCode]                                      
  FROM dbo.DxCode dx                                      
 ) dx                                      
 ON cc.ClinicalCaseId = dx.ClinicalCaseId AND dx.VersionId = cv.VersionId                          
    LEFT JOIN          
 (                                      
  SELECT DISTINCT cp.ClinicalCaseId, cp.VersionId,                                      
  SUBSTRING(                                      
   (                                      
    SELECT '|' + cptc.cptCode + '^'+isnull(cptc.Modifier,'') + '^'+ISNULL(cptc.qty,'')+'^'+isnull(cptc.Links,'')   AS [text()]               
    FROM dbo.cptcode cptc                                      
    WHERE cp.ClinicalCaseId = cptc.ClinicalCaseId AND cp.VersionID = cptc.VersionID                                      
    ORDER BY cptc.ClinicalCaseId                                      
    FOR XML PATH ('')                              
   ), 2, 1000) [CptCode]                                      
  FROM dbo.cptcode cp                                      
 ) cp          
 ON cc.ClinicalCaseId = cp.ClinicalCaseId AND cp.VersionId = cv.VersionId                                      
    LEFT JOIN QARemarksCTE qr ON qr.ClinicalCaseId = wi.ClinicalCaseId                            
    --LEFT JOIN CoderRemarksCTE cr ON cr.ClinicalCaseId = wi.ClinicalCaseId                              
 LEFT JOIN Payor QAP ON QAP.PayorId = qr.QAPayorId                                      
 LEFT JOIN [Provider] QAPr ON QAPr.ProviderID = qr.QAProviderID                                      
 LEFT JOIN ProviderFeedback QAPf ON QAPf.ProviderFeedbackId = qr.QAProviderFeedbackID                                      
                                  
 LEFT JOIN Payor P ON P.PayorId = wp.PayorId                                      
 LEFT JOIN [Provider] Pr ON Pr.ProviderID = wp.ProviderID                                      
 LEFT JOIN ProviderFeedback Pf ON Pf.ProviderFeedbackId = wp.ProviderFeedbackID                                   
                                  
 INNER JOIN ProjectUser pu                                    
 ON pu.ProjectId = wi.ProjectId                                      
    WHERE cc.ProjectId = @ProjectID AND ((wi.StatusId = 9 AND wi.ShadowQABy = @UserId AND ISNULL(wi.IsBlocked, 0) = 0) OR wi.StatusId = 8) AND pu.UserId = @UserId AND pu.RoleId = 3 AND pu.IsActive = 1                                   
    AND qv.rn = 1                            
    ORDER BY qv.VersionDate                                    
                          
 UPDATE WorkItem set StatusId = 9, ShadowQABy = @UserId, ShadowQADate = GETDATE() WHERE ClinicalCaseId = (        
 SELECT ClinicalCaseId FROM #ShadowQAAvailable                                    
 )                                    
                                     
 SELECT * FROM #ShadowQAAvailable                                    
                                      
END                  
ELSE IF @Role = 'ShadowQA' AND @ChartType = 'Block'                                      
BEGIN                                      
                                      
 ;WITH QAVersionCTE AS (                                      
        SELECT v.ClinicalCaseId, v.VersionId, v.VersionDate, v.StatusId                                      
        , ROW_NUMBER() OVER(PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC) AS rn                 
    FROM [Version] v                                      
        JOIN WorkItem wi                                    
        ON v.ClinicalCaseId = wi.ClinicalCaseId                                      
        WHERE wi.ProjectId = @ProjectID                            
  AND wi.StatusId IN (8, 9)                          
        AND v.StatusId IN (7, 8)                            
    )                                      
 ,CoderVersionCTE AS (                                      
        SELECT v.ClinicalCaseId, v.VersionId, v.VersionDate, v.StatusId                                      
        , ROW_NUMBER() OVER(PARTITION BY v.ClinicalCaseId ORDER BY VersionId DESC) AS rn                                      
        FROM [Version] v                                      
        JOIN WorkItem wi                                    
        ON v.ClinicalCaseId = wi.ClinicalCaseId                             
        WHERE wi.ProjectId = @ProjectID                            
  AND wi.StatusId IN (8, 9)                          
        AND v.StatusId IN (3, 12)                            
    ) ,QARemarksCTE AS (                                      
        SELECT wa.[ClinicalCaseId]                                      
            ,MAX(CASE WHEN FieldName = 'PayorId' THEN FieldValue END) AS QAPayorId                                      
            ,MAX(CASE WHEN FieldName = 'ProviderID' THEN FieldValue END) AS QAProviderID                                      
            ,MAX(CASE WHEN FieldName = 'CPTCode' THEN FieldValue END) AS QACPTCode                           
            ,MAX(CASE WHEN FieldName = 'Mod' THEN FieldValue END) AS QAMod                                      
            ,MAX(CASE WHEN FieldName = 'Dx' THEN FieldValue END) AS QADx                                      
            ,MAX(CASE WHEN FieldName = 'ProviderFeedbackID' THEN FieldValue END) AS QAProviderFeedbackID                                      
            ,MAX(CASE WHEN FieldName = 'PayorId' THEN Remark END) AS QAPayorIdRemark                                      
            ,MAX(CASE WHEN FieldName = 'ProviderID' THEN Remark END) AS QAProviderIDRemark                     
            ,MAX(CASE WHEN FieldName = 'CPTCode' THEN Remark END) AS QACPTCodeRemark                                      
   ,MAX(CASE WHEN FieldName = 'Mod' THEN Remark END) AS QAModRemark                                      
            ,MAX(CASE WHEN FieldName = 'Dx' THEN Remark END) AS QADxRemark                                      
            ,MAX(CASE WHEN FieldName = 'ProviderFeedbackID' THEN Remark END) AS QAProviderFeedbackIDRemark                                      
        FROM [dbo].[WorkItemAudit] wa                                      
        JOIN QAVersionCTE qv                                      
        ON qv.ClinicalCaseId = wa.ClinicalCaseId AND wa.VersionId = qv.VersionId                                      
        GROUP BY wa.ClinicalCaseId                                      
    )                                     
    SELECT TOP 1                               
    cc.ClinicalCaseID                                      
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
 ,bc.Name AS BlockCategory  
 ,bh.Remarks AS BlockRemarks  
 ,bh.CreateDate AS BlockedDate  
 --INTO #ShadowQABlocked  
    FROM ClinicalCase cc INNER JOIN WorkItem wi ON wi.ClinicalCaseId = cc.ClinicalCaseId  
 LEFT JOIN BlockHistory bh ON bh.ClinicalCaseId =CC.ClinicalCaseId  
 LEFT JOIN BlockCategory bc ON bc.BlockCategoryId =bh.BlockCategoryId  
    LEFT JOIN WorkItemProvider wp ON wp.ClinicalCaseId =CC.ClinicalCaseId                                      
    LEFT JOIN QAVersionCTE qv ON qv.ClinicalCaseId = cc.ClinicalCaseId                                      
 LEFT JOIN CoderVersionCTE cv ON cv.ClinicalCaseId = cc.ClinicalCaseId                                      
    LEFT JOIN (                                      
SELECT DISTINCT dx.ClinicalCaseId, dx.VersionId,                                      
  SUBSTRING(                                      
   (                                      
    SELECT ',' + dxc.DxCode  AS [text()]                                      
    FROM dbo.DxCode dxc                                      
    WHERE dx.ClinicalCaseId = dxc.ClinicalCaseId AND dx.VersionID = dxc.VersionID                                      
    ORDER BY dxc.ClinicalCaseId                                      
    FOR XML PATH ('')                              
   ), 2, 1000) [DxCode]                                      
  FROM dbo.DxCode dx                                      
 ) dx                                      
 ON cc.ClinicalCaseId = dx.ClinicalCaseId AND dx.VersionId = cv.VersionId                          
    LEFT JOIN          
 (                                      
  SELECT DISTINCT cp.ClinicalCaseId, cp.VersionId,                                      
  SUBSTRING(                                      
   (                                      
    SELECT '|' + cptc.cptCode + '^'+isnull(cptc.Modifier,'') + '^'+ISNULL(cptc.qty,'')+'^'+isnull(cptc.Links,'')   AS [text()]               
    FROM dbo.cptcode cptc                                      
    WHERE cp.ClinicalCaseId = cptc.ClinicalCaseId AND cp.VersionID = cptc.VersionID                                      
    ORDER BY cptc.ClinicalCaseId                                      
    FOR XML PATH ('')                              
   ), 2, 1000) [CptCode]                                      
  FROM dbo.cptcode cp                                      
 ) cp          
 ON cc.ClinicalCaseId = cp.ClinicalCaseId AND cp.VersionId = cv.VersionId                                      
    LEFT JOIN QARemarksCTE qr ON qr.ClinicalCaseId = wi.ClinicalCaseId                            
    --LEFT JOIN CoderRemarksCTE cr ON cr.ClinicalCaseId = wi.ClinicalCaseId                           
 LEFT JOIN Payor QAP ON QAP.PayorId = qr.QAPayorId                                      
 LEFT JOIN [Provider] QAPr ON QAPr.ProviderID = qr.QAProviderID                                      
 LEFT JOIN ProviderFeedback QAPf ON QAPf.ProviderFeedbackId = qr.QAProviderFeedbackID                                      
                                  
 LEFT JOIN Payor P ON P.PayorId = wp.PayorId                                      
 LEFT JOIN [Provider] Pr ON Pr.ProviderID = wp.ProviderID                                      
 LEFT JOIN ProviderFeedback Pf ON Pf.ProviderFeedbackId = wp.ProviderFeedbackID                                   
                                  
 INNER JOIN ProjectUser pu                                    
 ON pu.ProjectId = wi.ProjectId                                      
    WHERE cc.ProjectId = @ProjectID AND ((wi.StatusId = 9 AND wi.ShadowQABy = @UserId AND ISNULL(wi.IsBlocked, 0) = 1)) AND pu.UserId = @UserId AND pu.RoleId = 3 AND pu.IsActive = 1                                   
    AND qv.rn = 1                            
    ORDER BY qv.VersionDate  
  
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