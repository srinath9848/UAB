CREATE PROCEDURE [dbo].[UspSubmitShadowQA]                        
@ClinicalcaseID INT,                              
@UserId INT,                              
@utWorkItemAudit AS utWorkItemAudit readonly                        
AS                              
BEGIN                              
                              
BEGIN TRY                                    
  BEGIN TRANSACTION                                    
                                    
  DECLARE @RC INT                                    
  DECLARE @message VARCHAR(500)                                    
                                    
  EXEC @RC = sp_getapplock @Resource = 'UspSubmitShadowQAAppLock'                                    
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
                                
  
DECLARE @isQAAgreed BIT = 0   
                      
DECLARE @IsShadowQAAgreed BIT = 0   
                     
IF NOT EXISTS(Select wa.* from WorkItemAudit wa RIGHT JOIN [Version] v ON wa.VersionId = v.VersionId where wa.ClinicalCaseId=@ClinicalcaseID AND v.StatusId IN (6, 7) AND wa.IsAccepted = 0)   
SET @isQAAgreed = 1         
                              
IF NOT EXISTS(SELECT 1 FROM @utWorkItemAudit WHERE IsAccepted=0)                 
 SET @IsShadowQAAgreed = 1                       
                        
 DECLARE @StatusID INT                        
                      
IF (@isQAAgreed = 1 AND @IsShadowQAAgreed = 1)                      
BEGIN                      
 SET @StatusID = 15                       
 INSERT INTO [Version] (ClinicalCaseId,VersionDate,UserId,StatusID)                              
SELECT @ClinicalcaseID,GETUTCDATE(),@UserId,10                         
                       
DECLARE @VersionID INT                       
SELECT @VersionID = SCOPE_IDENTITY()             
 INSERT INTO [Version] (ClinicalCaseId,VersionDate,UserId,StatusID)                              
SELECT @ClinicalcaseID,GETUTCDATE(),@UserId,@StatusID                       
 END                      
ELSE IF (@isQAAgreed = 0 AND @IsShadowQAAgreed = 1)                      
BEGIN                      
 SET @StatusID = 14                       
 INSERT INTO [Version] (ClinicalCaseId,VersionDate,UserId,StatusID)                              
SELECT @ClinicalcaseID,GETUTCDATE(),@UserId,10      
      
SELECT @VersionID = SCOPE_IDENTITY()                 
                      
INSERT INTO [Version] (ClinicalCaseId,VersionDate,UserId,StatusID)                              
SELECT @ClinicalcaseID,GETUTCDATE(),@UserId,@StatusID                       
 END                      
                       
 IF (@IsShadowQAAgreed = 0)                      
 BEGIN                      
 SET @StatusID = 11                       
 INSERT INTO [Version] (ClinicalCaseId,VersionDate,UserId,StatusID)                              
SELECT @ClinicalcaseID,GETUTCDATE(),@UserId,@StatusID         
      
SELECT @VersionID = SCOPE_IDENTITY()          
                    
 END                      
                              
UPDATE WorkItem SET StatusId = @StatusID , ShadowQABy=@UserId, ShadowQADate= GETUTCDATE()                       
WHERE ClinicalCaseId = @ClinicalcaseID                              
                              
IF EXISTS(SELECT 1 FROM @utWorkItemAudit)                 
INSERT INTO WorkItemAudit(ClinicalCaseId,VersionID,FieldName,FieldValue,Remark,ErrorTypeID,ClaimId,IsAccepted)                                    
SELECT @ClinicalcaseID,@VersionID,FieldName,FieldValue,Remark,ErrorTypeId, nullif(ClaimId,0),IsAccepted FROM @utWorkItemAudit                                                     
                         
                              
COMMIT TRANSACTION                                
 END TRY                                    
                                    
 BEGIN CATCH                                
 declare @err varchar(max)= (select  Error_Message()  )                              
                              
   RAISERROR (@err,0,1                                    
    )                                 
  ROLLBACK TRANSACTION                     
 END CATCH                                  
                              
END