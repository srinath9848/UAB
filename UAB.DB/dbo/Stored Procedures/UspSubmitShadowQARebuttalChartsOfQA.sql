CREATE PROCEDURE [dbo].[UspSubmitShadowQARebuttalChartsOfQA]                    
@ClinicalcaseID INT,                                
@UserId INT,             
@StatusID INT,                                    
@utWorkItemAudit AS utWorkItemAudit readonly                              
                              
AS  
BEGIN                                
                                
BEGIN TRY                                      
  BEGIN TRANSACTION                                      
                                      
  DECLARE @RC INT                                      
  DECLARE @message VARCHAR(500)                                      
                                      
  EXEC @RC = sp_getapplock @Resource = 'UspSubmitShadowQARebuttalChartsOfQA'                                      
   ,@LockMode = 'Exclusive'                                      
   ,@LockOwner = 'Transaction'                                      
   ,@LockTimeout = 30000 -- 1/2 minute                                      
                                      
  IF @RC < 0                                      
  BEGIN                                      
     SELECT @message = CONVERT(VARCHAR(30), GETUTCDATE(), 121) + ': Sorry, could not obtain a lock within the timeout period, return code was ' + CONVERT(VARCHAR(30), @RC) + '.'                                      
                                      
     RAISERROR (                                      
    @message                                      
    ,0                                      
    ,1                                      
    )                                      
     WITH NOWAIT;                                      
                                      
     SELECT '0' AS WorkItemID                                      
   ,@message AS Message                                      
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
  
--If the 1st QA Audit values and the last Audit values are same,     
--that means there are no changes to Coder values. So, it should go to Ready for Posting.    
--TODO: Think about if there will be any scenarios, where 1st and last audit values are same,     
--      but the Coder values changes suggested by either QA/SQA are accepted in between the versions.    
IF NOT EXISTS (  
 SELECT FieldName, FieldValue, NULLIF(ClaimId,0), IsAccepted FROM @utWorkItemAudit  
 EXCEPT  
 SELECT FieldName, FieldValue, ClaimId, IsAccepted  
 FROM WorkItemAudit  
 WHERE ClinicalCaseId = @ClinicalcaseID AND VersionId = (SELECT MIN(VersionId) FROM WorkItemAudit WHERE ClinicalCaseId = @ClinicalcaseID)  
)  
BEGIN  
 SET @StatusID = 15  
END  
    
                                
IF @StatusID =14       --To Coder Correction            
BEGIN  
 UPDATE WorkItem SET StatusId = @StatusID,ShadowQADate = GETUTCDATE(), ShadowQABy = @UserId                        
 WHERE ClinicalCaseId = @ClinicalcaseID                      
          
 INSERT INTO [Version] (ClinicalCaseId,VersionDate,UserId,StatusID)                           
 SELECT @ClinicalcaseID,GETUTCDATE(),@UserId,10   -- Shadow QA Completed        
        
 DECLARE @VersionID INT                                 
 SELECT @VersionID = SCOPE_IDENTITY()                  
              
 INSERT INTO [Version] (ClinicalCaseId,VersionDate,UserId,StatusID)     
 SELECT @ClinicalcaseID,GETUTCDATE(),@UserId,@StatusID          
                
              
 INSERT INTO WorkItemAudit(ClinicalCaseId,VersionID,FieldName,FieldValue,Remark,ErrorTypeID,ClaimId,IsAccepted)                              
 SELECT @ClinicalcaseID,@VersionID,FieldName,FieldValue,Remark,ErrorTypeId, NULLIF(ClaimId,0),IsAccepted FROM @utWorkItemAudit                                 
END                  
                    
ELSE             --Shadow QA Rejected             
BEGIN  
 UPDATE WorkItem SET StatusId = @StatusID,ShadowQADate = GETUTCDATE(), ShadowQABy = @UserId                        
 WHERE ClinicalCaseId = @ClinicalcaseID                              
                     
 INSERT INTO [Version] (ClinicalCaseId,VersionDate,UserId,StatusID)                                
 SELECT @ClinicalcaseID,GETUTCDATE(),@UserId,@StatusID                
              
 SELECT @VersionID = SCOPE_IDENTITY()                   
              
 INSERT INTO WorkItemAudit(ClinicalCaseId,VersionID,FieldName,FieldValue,Remark,ErrorTypeID,ClaimId,IsAccepted)                              
 SELECT @ClinicalcaseID,@VersionID,FieldName,FieldValue,Remark,ErrorTypeId, nullif(ClaimId,0),IsAccepted FROM @utWorkItemAudit                 
END  
                                
COMMIT TRANSACTION                                      
END TRY                                      
                                      
BEGIN CATCH  
 ROLLBACK TRANSACTION                            
 THROW;    
END CATCH                                    
                                
END