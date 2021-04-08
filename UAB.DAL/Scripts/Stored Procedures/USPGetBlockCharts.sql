Create PROCEDURE [dbo].[USPGetBlockCharts] --1,'Coder','Block',1          
@ProjectID INT,                                
@Role VARCHAR(50),                              
@ChartType VARCHAR(50),                              
@UserId INT                              
AS         
IF @Role = 'Coder' AND @ChartType = 'Block'    
BEGIN    
SELECT  CC.ClinicalCaseID,PatientMRN                                
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
 AND ((W.AssignedTo = @UserId AND W.StatusId = 2  AND ISNULL(W.IsBlocked, 0) = 1))            
 AND pu.UserId = @UserId  AND pu.RoleId = 1 AND pu.IsActive = 1                  
 ORDER BY CC.ClinicalCaseID          
    
END    
ELSE  
IF @Role = 'QA' AND @ChartType = 'Block'                                        
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
 SELECT                         
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
 WHERE cc.ProjectId = @ProjectID AND ((W.StatusId = 5 AND W.QABy = @UserId AND ISNULL(W.IsBlocked, 0) = 1)) AND pu.UserId = @UserId AND pu.RoleId = 2 AND pu.IsActive = 1                                      
 AND v.rn = 1                                        
 ORDER BY v.VersionDate                                        
                                        
 --UPDATE WorkItem set StatusId = 5, QABy = @UserId, QADate = GETDATE() WHERE ClinicalCaseId = (    
 --SELECT ClinicalCaseId FROM #QABlocked    
 --)    
                                       
 --SELECT * FROM #QABlocked    
                                        
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
    SELECT                                
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