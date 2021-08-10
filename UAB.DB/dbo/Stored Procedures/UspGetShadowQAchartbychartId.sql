


CREATE PROCEDURE [dbo].[UspGetShadowQAchartbychartId]      
 --1,'ShadowQA','Block',1 ,812                         
 @ProjectID INT      
 ,@Role VARCHAR(50)      
 ,@ChartType VARCHAR(50)      
 ,@UserId INT    
 ,@ClinicalCaseId INT     
AS      
BEGIN 
 IF @Role = 'ShadowQA'      
   AND @ChartType = 'Block'      
  BEGIN          
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
   WHERE c.ClinicalCaseId = @ClinicalCaseId AND bh.BlockedInQueueKind = 'ShadowQA'        
   ORDER BY c.ClaimOrder      
  END 
  END
