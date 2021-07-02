CREATE PROCEDURE [dbo].[UspSubmitProviderPostedChart]  --1,'some text',1,'71045',26,'J98.4',1,1,'2021-05-07 05:13:19.2600000','Test Coder Comment'                        
@PayorID INT = NULL,                          
@NoteTitle VARCHAR(200) = NULL,                          
@ProviderID INT = NULL,                   
@utCpt AS utCpt readonly,                      
@utClaim1 AS utClaim readonly,                      
@utCpt1 AS utCpt readonly,                        
@Mod VARCHAR(25) = NULL,                          
@Dx VARCHAR(MAX) = NULL,                          
@ProviderFeedbackID INT = NULL,                          
@CoderQuestion VARCHAR(MAX) = NULL,                          
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
                          
                                
  EXEC @RC = sp_getapplock @Resource = 'UspSubmitCodingAppLock'                                
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
                        
                  
DECLARE @MinClaimId INT = (SELECT Min(RNO) FROM @utClaim1)                         
DECLARE @MaxClaimId INT = (SELECT MAX(RNO) FROM @utClaim1)              
              
 --INSERT INTO [Version] (ClinicalCaseId,VersionDate,UserId,StatusID)                          
 --SELECT @ClinicalcaseID,GETUTCDATE(),@UserId,3                                 
       
 DECLARE @StatusId INT      
 SET @StatusId = 17      
                
  INSERT INTO [Version] (ClinicalCaseId,VersionDate,UserId,StatusID)                          
 SELECT @ClinicalcaseID,GETUTCDATE(),@UserId,@StatusId    
    
 DECLARE @VersionID INT                           
 SELECT @VersionID = SCOPE_IDENTITY()     
    
 INSERT INTO [ProviderPosted](PostingDate,CoderComment,ClinicalCaseId,ProviderId)    
 SELECT @PostedDate,@CoderComment,@ClinicalcaseID,@ProviderPostedId    
              
WHILE (@MinClaimId <= @MaxClaimId)                                  
BEGIN               
              
INSERT INTO [Claim]               
SELECT @ClinicalcaseID,@VersionId              
              
 DECLARE @ClaimID INT                           
 SELECT @ClaimID = SCOPE_IDENTITY()               
              
-- DECLARE @MinId BIGINT = (SELECT MIN(Rno) FROM @utCpt)                                
--DECLARE @MaxId BIGINT = (SELECT MAX(Rno) FROM @utCpt)               
              
--WHILE (@MinId <= @MaxId)            
--BEGIN               
              
 INSERT INTO CptCode(ClinicalCaseId,VersionId,CPTCode,Modifier,Qty,Links,ClaimId)                 
 SELECT @ClinicalcaseID,@VersionID,CPTCode,[Mod],Qty,Links,@ClaimID FROM @utCPT Where Rno = @MinClaimId              
              
              
--SELECT @MinId  = (SELECT Min(Rno) FROM @utCPT WHERE Rno > @MinId)                 
--END              
            
DECLARE @ClaimDx varchar(500)            
            
Select @ClaimDx = Dx from @utClaim1 where Rno= @MinClaimId                  
            
 INSERT INTO DxCode(ClinicalCaseId,VersionId,ClaimId,DxCode)                          
 SELECT @ClinicalcaseID,@VersionID,@ClaimID,value FROM dbo.fnSplit(NULLIF(@ClaimDx, ''), ',')                
              
 INSERT INTO WorkitemProvider(ClinicalCaseId,VersionId,ProviderID,PayorID,ProviderFeedbackID,ClaimId)                          
 SELECT @ClinicalcaseID,@VersionID,ProviderId,PayorId,nullif(ProviderFeedbackId,''),@ClaimID  from @utClaim1 where Rno= @MinClaimId                
              
 SET @MinClaimId += 1                               
                            
END              
            
-- Claim 1 Insertion Started---            
            
INSERT INTO DxCode(ClinicalCaseId,VersionId,DxCode)                          
 SELECT @ClinicalcaseID,@VersionID,value FROM dbo.fnSplit(NULLIF(@Dx, ''), ',')                
             
 DECLARE @MinId BIGINT = (SELECT MIN(Rno) FROM @utCpt)                                
DECLARE @MaxId BIGINT = (SELECT MAX(Rno) FROM @utCpt)               
              
WHILE (@MinId <= @MaxId)                                  
BEGIN               
              
 INSERT INTO CptCode(ClinicalCaseId,VersionId,CPTCode,Modifier,Qty,Links)                 
 SELECT @ClinicalcaseID,@VersionID,CPTCode,[Mod],Qty,Links FROM @utCPT Where Rno = @MinId              
              
              
SELECT @MinId  = (SELECT Min(Rno) FROM @utCPT WHERE Rno > @MinId)                 
END                           
                          
 INSERT INTO WorkitemProvider(ClinicalCaseId,VersionId,ProviderID,PayorID,ProviderFeedbackID)                          
 SELECT @ClinicalcaseID,@VersionID,@ProviderID,@PayorID,@ProviderFeedbackID                    
                          
 UPDATE WorkItem SET StatusId = @StatusId,                          
     CodedDate = GETUTCDATE(),                          
     NoteTitle = @NoteTitle  ,                
  IsPriority=0,                
     IsBlocked=0 ,        
  IsQA = 0  ,  
  IsWrongProvider = @IsWrongProvider  
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