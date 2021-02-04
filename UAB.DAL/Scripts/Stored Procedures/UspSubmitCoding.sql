alter PROCEDURE UspSubmitCoding  --1,'some text',1,'71045',26,'J98.4',1,1,1,null
@PayorID INT,
@NoteTitle VARCHAR(200) = NULL,
@ProviderID INT,
@CPTCode VARCHAR(5) = '71045',
@Mod VARCHAR(25) = null,
@Dx VARCHAR(MAX),
@ProviderFeedbackID INT,
@CoderQuestion VARCHAR(MAX) =NULL,
@ClinicalcaseID INT,
@AssignedTo INT
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

UPDATE WorkItem SET StatusId = 2,
					AssignedDate = GETDATE(),
                    AssignedTo = @AssignedTo,					
					NoteTitle = @NoteTitle
WHERE ClinicalCaseId = @ClinicalcaseID

INSERT INTO [Version] (ClinicalCaseId,VersionDate,UserId,StatusID)
SELECT @ClinicalcaseID,GETDATE(),0,2

DECLARE @VersionID INT 
SELECT @VersionID = SCOPE_IDENTITY()

INSERT INTO CptCode(ClinicalCaseId,VersionId,CPTCode,Modifier)
SELECT @ClinicalcaseID,@VersionID,@CPTCode,@Mod

INSERT INTO DxCode(ClinicalCaseId,VersionId,DxCode)
SELECT @ClinicalcaseID,@VersionID,value FROM dbo.fnSplit(NULLIF(@Dx, ''), ',')   

INSERT INTO WorkitemProvider(ClinicalCaseId,VersionId,ProviderID,PayorID,ProviderFeedbackID)
SELECT @ClinicalcaseID,@VersionID,@ProviderID,@PayorID,@ProviderFeedbackID

IF @CoderQuestion IS NOT NULL
begin
INSERT INTO CoderQuestion(ClinicalCaseId,Question,QuestionBy,QuestionDate)
SELECT @ClinicalcaseID,@CoderQuestion,0,GETDATE()
end


 COMMIT TRANSACTION      
 END TRY      
      
 BEGIN CATCH  
 declare @err varchar(max)= (select  Error_Message()  )

   RAISERROR (@err,0,1      
			 )   
  ROLLBACK TRANSACTION      
 END CATCH    

END

