CREATE PROCEDURE [dbo].[UspSubmitProviderPostedChart]      
@PayorID INT = NULL,                                
@NoteTitle VARCHAR(200) = NULL,                                
@ProviderID INT = NULL,                         
@utDxCode AS utDxCode readonly,                                
@utCptCode AS utCptCode readonly,                           
@ProviderFeedbackID INT = NULL,                                
@ClinicalcaseID INT,                                
@UserId INT,           
@ProviderPostedId INT,          
@PostedDate DateTime2,                                
@CoderComment varchar(500) = NULL,        
@IsWrongProvider BIT        
AS                                
BEGIN                                
                                
BEGIN TRY                                      
  BEGIN TRANSACTION                                      
                                      
  DECLARE @RC INT                                      
  DECLARE @message VARCHAR(500)                                      
                                
                                      
  EXEC @RC = sp_getapplock @Resource = 'UspSubmitProviderPostedChart'                                      
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
                              
             
 DECLARE @StatusId INT            
 SET @StatusId = 17            
                      
  INSERT INTO [Version] (ClinicalCaseId,VersionDate,UserId,StatusID)                                
 SELECT @ClinicalcaseID,GETUTCDATE(),@UserId,@StatusId          
          
 DECLARE @VersionID INT                                 
 SELECT @VersionID = SCOPE_IDENTITY()           
          
 INSERT INTO [ProviderPosted](PostingDate,CoderComment,ClinicalCaseId,ProviderId,PayorID,ProviderFeedbackID,VersionId)          
 SELECT @PostedDate,@CoderComment,@ClinicalcaseID,@ProviderPostedId,@PayorID,@ProviderFeedbackID,@VersionID          
                    
BEGIN                     
     
 INSERT INTO ProviderDxCode(ClinicalCaseId,VersionId,DxCode,ClaimId)                                                        
SELECT @ClinicalcaseID,@VersionID,DxCode,nullif(ClaimId,0) from @utDxCode    
    
INSERT INTO ProviderCptCode(ClinicalCaseId,VersionId,CPTCode,Modifier,Qty,Links,ClaimId)                                          
SELECT @ClinicalcaseID,@VersionID,CPTCode,[Mod],Qty,Links,nullif(ClaimId,0) from @utCptCode                         
                    
END                    
                  
-- Claim 1 Insertion Started---                  
                                
 UPDATE WorkItem SET StatusId = @StatusId,                                
     CodedDate = GETUTCDATE(),                                
     NoteTitle = @NoteTitle  ,                      
     IsPriority=0,                      
     IsBlocked=0 ,              
  IsWrongProvider = @IsWrongProvider,        
  IsQA = 0          
 WHERE ClinicalCaseId = @ClinicalcaseID                     
                   
 -- Claim 1 Insertion Ended---                           
                             
                                
 COMMIT TRANSACTION                                      
 END TRY                                      
                                      
 BEGIN CATCH                                  
 declare @err varchar(max)= (select  Error_Message()  )                                
                                
   RAISERROR (@err,0,1                                      
    )                                   
  ROLLBACK TRANSACTION                                      
 END CATCH                                    
                        
END     
    
  