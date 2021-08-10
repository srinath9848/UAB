CREATE PROCEDURE [dbo].[UspSubmitQARebuttalChartsOfCoder]                                    
@ClinicalcaseID INT,                                    
@UserId INT,                         
@utBasicParams AS utBasicParams readonly,               
@utDxCode AS utDxCode readonly,                            
@utCptCode AS utCptCode readonly,                                                                                            
@utWorkItemAudit AS utWorkItemAudit readonly                                    
                        
AS                                    
BEGIN                                    
                                    
BEGIN TRY                                          
  BEGIN TRANSACTION                                          
                                          
  DECLARE @RC INT                                          
  DECLARE @message VARCHAR(500)                                          
                                          
  EXEC @RC = sp_getapplock @Resource = 'UspSubmitQARebuttalChartsOfCoder'                                          
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
                          
DECLARE @IsShadowQA INT                      
DECLARE @StatusId INT        
DECLARE @VersionID INT                              
                          
select @IsShadowQA = IsShadowQA from workitem where clinicalcaseid = @ClinicalcaseID                     
                
-- If Accept all ---> Ready for Posting                
IF NOT EXISTS(SELECT 1 FROM @utWorkItemAudit WHERE IsAccepted=0)                      
SET @StatusID = 15                                    
                      
 ---With Shadow QA  --> QA Rebutted                   
ELSE IF EXISTS(SELECT 1 FROM @utWorkItemAudit WHERE IsAccepted=0) AND @IsShadowQA = 1                      
 SET @StatusID = 13                     
                
--With out Shadow QA   --> To Coder Correction                   
IF EXISTS(SELECT 1 FROM @utWorkItemAudit WHERE IsAccepted=0) AND @IsShadowQA = 0                   
 SET @StatusID = 14                          
                           
UPDATE WorkItem SET StatusId = @StatusID,QADate = GETUTCDATE()                                 
WHERE ClinicalCaseId = @ClinicalcaseID                                    
                              
IF NOT EXISTS(SELECT 1 FROM @utWorkItemAudit WHERE IsAccepted=0)                      
BEGIN                                                        
INSERT INTO [Version] (ClinicalCaseId,VersionDate,UserId,StatusID)                                     
SELECT @ClinicalcaseID,GETUTCDATE(),@UserId,6   -- QA Completed        
      
SELECT @VersionID = SCOPE_IDENTITY()                                   
                                      
INSERT INTO [Version] (ClinicalCaseId,VersionDate,UserId,StatusID)                                      
SELECT @ClinicalcaseID,GETUTCDATE(),@UserId,@StatusID         
      
INSERT INTO DxCode(ClinicalCaseId,VersionId,DxCode,ClaimId)                                                    
SELECT @ClinicalcaseID,@VersionID,DxCode,nullif(ClaimId,0) from @utDxCode                                  
                            
INSERT INTO CptCode(ClinicalCaseId,VersionId,CPTCode,Modifier,Qty,Links,ClaimId)                                                      
SELECT @ClinicalcaseID,@VersionID,CPTCode,[Mod],Qty,Links,nullif(ClaimId,0) from @utCptCode                                  
                            
INSERT INTO WorkitemProvider(ClinicalCaseId,VersionId,ProviderID,PayorID,ProviderFeedbackID,ClaimId)                                                      
SELECT @ClinicalcaseID,@VersionID,ProviderID,PayorID,nullif(ProviderFeedbackID,0),nullif(ClaimId,0) from @utBasicParams                              
                                                        
INSERT INTO WorkItemAudit(ClinicalCaseId,VersionID,FieldName,FieldValue,Remark,ErrorTypeID,ClaimId,IsAccepted)                                  
SELECT @ClinicalcaseID,@VersionID,FieldName,FieldValue,Remark,ErrorTypeID,nullif(ClaimId,0),IsAccepted FROM @utWorkItemAudit                                                                             
                                 
                             
END                                 
ELSE                            
BEGIN                              
                        
INSERT INTO [Version] (ClinicalCaseId,VersionDate,UserId,StatusID)                                    
SELECT @ClinicalcaseID,GETUTCDATE(),@UserId,7        
                                                    
INSERT INTO [Version] (ClinicalCaseId,VersionDate,UserId,StatusID)                                    
SELECT @ClinicalcaseID,GETUTCDATE(),@UserId,@StatusID        
  
SELECT @VersionID = SCOPE_IDENTITY()           
                                  
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