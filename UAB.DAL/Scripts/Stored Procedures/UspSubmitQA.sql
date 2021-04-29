Create PROCEDURE [dbo].[UspSubmitQA]  --1,'some text',1,'71045',26,'J98.4',1,1,1,null              
@PayorID INT = NULL,              
@PayorRemarks VARCHAR(MAX) = NULL,              
@NoteTitle VARCHAR(200) = NULL,              
@ProviderID INT = NULL,              
@ProviderRemarks VARCHAR(MAX) = NULL,              
@CPTCode VARCHAR(1000) = NULL,              
@CPTCodeRemarks VARCHAR(MAX) = NULL,              
@Mod VARCHAR(25)  = NULL,              
@ModRemarks VARCHAR(MAX) = NULL,              
@Dx VARCHAR(MAX) = NULL,              
@DxRemarks VARCHAR(MAX) = NULL,              
@ProviderFeedbackID INT = NULL,              
@ProviderFeedbackRemarks VARCHAR(MAX) = NULL,              
@CoderQuestion VARCHAR(MAX) = NULL,              
@ClinicalcaseID INT,              
@UserId INT,              
@ErrorTypeID INT = NULL ,      
@IsAuditRequired BIT  ,  
@utAudit AS utAudit readonly                  
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
     SELECT @message = CONVERT(VARCHAR(30), GETDATE(), 121) + ': Sorry, could not obtain a lock within the timeout period, return code was ' + CONVERT(VARCHAR(30), @RC) + '.'                    
                    
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
     SELECT @message = CONVERT(VARCHAR(30), GETDATE(), 121) + ': AppLock obtained ..'                    
                    
     RAISERROR (                    
    @message                    
    ,0                    
    ,1                    
    )                    
     WITH NOWAIT;                    
  END               
                
DECLARE @VersionID INT              
DECLARE @StatusID INT              
              
IF @PayorRemarks <> '' OR @ProviderRemarks <> '' OR @CPTCodeRemarks <> '' OR @ModRemarks <> ''               
OR @DxRemarks <> '' OR @ProviderFeedbackRemarks <> ''              
BEGIN      
    
 IF @IsAuditRequired = 1    
 SET @StatusId = 8    
 ELSE     
    SET @StatusId = 14    
     
         
UPDATE WorkItem SET StatusId = @StatusId,              
     QADate = GETDATE(),  
	 IsPriority=0,              
     IsBlocked=0 ,   
  IsShadowQA=@IsAuditRequired              
WHERE ClinicalCaseId = @ClinicalcaseID              
          
INSERT INTO [Version] (ClinicalCaseId,VersionDate,UserId,StatusID)             
SELECT @ClinicalcaseID,GETDATE(),@UserId,7   -- QA Completed            
              
SELECT @VersionID = SCOPE_IDENTITY()            
          
INSERT INTO [Version] (ClinicalCaseId,VersionDate,UserId,StatusID)              
SELECT @ClinicalcaseID,GETDATE(),@UserId,@StatusId              
              
IF @PayorID IS NOT NULL AND @PayorRemarks <> ''              
BEGIN              
 INSERT INTO WorkItemAudit(ClinicalCaseId,VersionID,FieldName,FieldValue,Remark,ErrorTypeID)              
 SELECT @ClinicalcaseID,@VersionID,'PayorID',@PayorID,@PayorRemarks,@ErrorTypeID              
END              
              
IF @ProviderID IS NOT NULL AND @ProviderRemarks <> ''              
BEGIN              
 INSERT INTO WorkItemAudit(ClinicalCaseId,VersionID,FieldName,FieldValue,Remark,ErrorTypeID)              
 SELECT @ClinicalcaseID,@VersionID,'ProviderID',@ProviderID,@ProviderRemarks,@ErrorTypeID     
END              
              
IF @CPTCode IS NOT NULL AND @CPTCodeRemarks <> ''              
BEGIN              
 INSERT INTO WorkItemAudit(ClinicalCaseId,VersionID,FieldName,FieldValue,Remark,ErrorTypeID)              
 SELECT @ClinicalcaseID,@VersionID,'CPTCode',@CPTCode,@CPTCodeRemarks,@ErrorTypeID              
END              
              
IF @Mod IS NOT NULL AND @ModRemarks <> ''              
BEGIN              
 INSERT INTO WorkItemAudit(ClinicalCaseId,VersionID,FieldName,FieldValue,Remark,ErrorTypeID)              
 SELECT @ClinicalcaseID,@VersionID,'Mod',@Mod,@ModRemarks,@ErrorTypeID              
END              
              
IF @dx IS NOT NULL AND @DxRemarks <> ''              
BEGIN              
 INSERT INTO WorkItemAudit(ClinicalCaseId,VersionID,FieldName,FieldValue,Remark,ErrorTypeID)              
 SELECT @ClinicalcaseID,@VersionID,'Dx',@dx,@DxRemarks,@ErrorTypeID              
END              
              
IF @ProviderFeedbackID IS NOT NULL AND @ProviderFeedbackRemarks <> ''              
BEGIN              
 INSERT INTO WorkItemAudit(ClinicalCaseId,VersionID,FieldName,FieldValue,Remark,ErrorTypeID)            
 SELECT @ClinicalcaseID,@VersionID,'ProviderFeedbackID',@ProviderFeedbackID,@ProviderFeedbackRemarks,@ErrorTypeID              
END     
  
INSERT INTO WorkItemAudit(ClinicalCaseId,VersionID,FieldName,FieldValue,Remark,ErrorTypeID,ClaimId)          
SELECT @ClinicalcaseID,@VersionID,FieldName,FieldValue,Remark,@ErrorTypeID,ClaimId FROM @utAudit     
  
           
    END          
          
 ELSE          
 BEGIN         
      
   IF @IsAuditRequired = 1    
 SET @StatusId = 8    
 ELSE     
    SET @StatusId = 15    
    
UPDATE WorkItem SET StatusId = @StatusId,              
     QADate = GETDATE() ,
	 IsPriority=0,              
     IsBlocked=0 ,     
  IsShadowQA=@IsAuditRequired             
WHERE ClinicalCaseId = @ClinicalcaseID              
          
INSERT INTO [Version] (ClinicalCaseId,VersionDate,UserId,StatusID)             
SELECT @ClinicalcaseID,GETDATE(),@UserId,6   -- QA Completed            
              
INSERT INTO [Version] (ClinicalCaseId,VersionDate,UserId,StatusID)              
SELECT @ClinicalcaseID,GETDATE(),@UserId,@StatusId            
          
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