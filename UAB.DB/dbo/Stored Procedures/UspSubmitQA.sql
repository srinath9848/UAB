CREATE PROCEDURE [dbo].[UspSubmitQA]                     
@ClinicalcaseID INT,                        
@UserId INT,                        
@IsAuditRequired BIT  ,            
@utWorkItemAudit AS utWorkItemAudit readonly                            
AS                        
BEGIN                        
                        
BEGIN TRY                              
  BEGIN TRANSACTION                              
                              
  DECLARE @RC INT                              
  DECLARE @message VARCHAR(500)                              
                              
  EXEC @RC = sp_getapplock @Resource = 'UspSubmitQAAppLock'                              
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
                          
DECLARE @VersionID INT                        
DECLARE @StatusID INT                        
                        
IF EXISTS(SELECT 1 FROM @utWorkItemAudit where IsAccepted=0)  
                   
BEGIN                
              
 IF @IsAuditRequired = 1              
 SET @StatusId = 8              
 ELSE               
    SET @StatusId = 14              
               
                   
UPDATE WorkItem SET StatusId = @StatusId,                        
     QADate = GETUTCDATE(),            
  IsPriority=0,                        
     IsBlocked=0 ,             
  IsShadowQA=@IsAuditRequired                        
WHERE ClinicalCaseId = @ClinicalcaseID                        
                    
INSERT INTO [Version] (ClinicalCaseId,VersionDate,UserId,StatusID)                       
SELECT @ClinicalcaseID,GETUTCDATE(),@UserId,7   -- QA Rejected                      
                        
SELECT @VersionID = SCOPE_IDENTITY()                      
                    
INSERT INTO [Version] (ClinicalCaseId,VersionDate,UserId,StatusID)                        
SELECT @ClinicalcaseID,GETUTCDATE(),@UserId,@StatusId         
    
            
INSERT INTO WorkItemAudit(ClinicalCaseId,VersionID,FieldName,FieldValue,Remark,ErrorTypeID,ClaimId,IsAccepted)                    
SELECT @ClinicalcaseID,@VersionID,FieldName,FieldValue,Remark,ErrorTypeId, nullif(ClaimId,0),IsAccepted FROM @utWorkItemAudit               
            
                     
    END                    
                    
 ELSE                    
 BEGIN                   
                
   IF @IsAuditRequired = 1              
 SET @StatusId = 8              
 ELSE               
    SET @StatusId = 15              
              
UPDATE WorkItem SET StatusId = @StatusId,                        
     QADate = GETUTCDATE() ,          
  IsPriority=0,                        
     IsBlocked=0 ,               
  IsShadowQA=@IsAuditRequired                       
WHERE ClinicalCaseId = @ClinicalcaseID     
                                       
INSERT INTO [Version] (ClinicalCaseId,VersionDate,UserId,StatusID)                       
SELECT @ClinicalcaseID,GETUTCDATE(),@UserId,6   -- QA Completed     
  
SELECT @VersionID = SCOPE_IDENTITY()                       
                        
INSERT INTO [Version] (ClinicalCaseId,VersionDate,UserId,StatusID)                        
SELECT @ClinicalcaseID,GETUTCDATE(),@UserId,@StatusId     
    
INSERT INTO WorkItemAudit(ClinicalCaseId,VersionID,FieldName,FieldValue,Remark,ErrorTypeID,ClaimId,IsAccepted)                    
SELECT @ClinicalcaseID,@VersionID,FieldName,FieldValue,Remark,ErrorTypeId, nullif(ClaimId,0),IsAccepted FROM @utWorkItemAudit               
                     
                    
 END                    
                        
 COMMIT TRANSACTION                              
 END TRY                              
                              
 BEGIN CATCH                          
 declare @err varchar(max)= (select  Error_Message()  )                        
                        
   RAISERROR (@err,0,1                              
    )                           
  ROLLBACK TRANSACTION                              
 END CATCH                            
                        
END