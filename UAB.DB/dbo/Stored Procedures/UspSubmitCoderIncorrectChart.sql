CREATE PROCEDURE [dbo].[UspSubmitCoderIncorrectChart]                   
@ClinicalcaseID INT,                                              
@UserId INT,                                              
@StatusId INT,                        
@utWorkItemAudit AS utWorkItemAudit readonly,      
@utBasicParams AS utBasicParams readonly,       
@utDxCode AS utDxCode readonly,                    
@utCptCode AS utCptCode readonly                                                                                    
                                                                   
AS                                              
BEGIN                                              
                                              
BEGIN TRY                                                    
  BEGIN TRANSACTION                                                    
                                                    
  DECLARE @RC INT                                                    
  DECLARE @message VARCHAR(500)                                                    
                                                    
  EXEC @RC = sp_getapplock @Resource = 'UspSubmitCoderIncorrectChartAppLock'                                                    
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
DECLARE @Error nvarchar(MAX);                                          
                                 
IF @StatusId=12                                         
BEGIN                                          
                                     
UPDATE WorkItem SET StatusId = @StatusId,                                              
     AssignedDate = GETUTCDATE()                                              
WHERE ClinicalCaseId = @ClinicalcaseID           
                                              
INSERT INTO [Version] (ClinicalCaseId,VersionDate,UserId,StatusID)                                      
SELECT @ClinicalcaseID,GETUTCDATE(),@UserId,@StatusId                                              
                                      
SELECT @VersionID = SCOPE_IDENTITY()      

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
UPDATE WorkItem SET StatusId = @StatusId,                                              
     AssignedDate = GETUTCDATE()                                              
WHERE ClinicalCaseId = @ClinicalcaseID                                              
                                              
INSERT INTO [Version] (ClinicalCaseId,VersionDate,UserId,StatusID)                                              
SELECT @ClinicalcaseID,GETUTCDATE(),@UserId,3                                              
                                              
SELECT @VersionID = SCOPE_IDENTITY()                                              
                                              
INSERT INTO [Version] (ClinicalCaseId,VersionDate,UserId,StatusID)                                              
SELECT @ClinicalcaseID,GETUTCDATE(),@UserId,@StatusId                                  
              
INSERT INTO DxCode(ClinicalCaseId,VersionId,DxCode,ClaimId)                                            
SELECT @ClinicalcaseID,@VersionID,DxCode,nullif(ClaimId,0) from @utDxCode                            
                    
INSERT INTO CptCode(ClinicalCaseId,VersionId,CPTCode,Modifier,Qty,Links,ClaimId)                                              
SELECT @ClinicalcaseID,@VersionID,CPTCode,[Mod],Qty,Links,nullif(ClaimId,0) from @utCptCode                          
                    
INSERT INTO WorkitemProvider(ClinicalCaseId,VersionId,ProviderID,PayorID,ProviderFeedbackID,ClaimId)                                              
SELECT @ClinicalcaseID,@VersionID,ProviderID,PayorID,nullif(ProviderFeedbackID,0),nullif(ClaimId,0) from @utBasicParams                      
    
INSERT INTO WorkItemAudit(ClinicalCaseId,VersionID,FieldName,FieldValue,Remark,ErrorTypeID,ClaimId,IsAccepted)                          
SELECT @ClinicalcaseID,@VersionID,FieldName,FieldValue,Remark,ErrorTypeID,nullif(ClaimId,0),IsAccepted FROM @utWorkItemAudit                                                              
     
  END                                          
                                          
 COMMIT TRANSACTION                                                    
 END TRY                                                    
                                                    
 BEGIN CATCH                                                
 declare @err varchar(max)= (select  Error_Message()  )                                              
   SELECT @Error = @err                     
   RAISERROR (@err,0,1                                                    
    )                                                 
  ROLLBACK TRANSACTION                                                    
 END CATCH                                                
                                              
END 

