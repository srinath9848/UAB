CREATE procedure UspSubmitProviderPosted  
@ClinicalCaseID INT,  
@UserId INT  
AS  
BEGIN  
  
 INSERT INTO [Version] (ClinicalCaseId,VersionDate,UserId,StatusID)                          
 SELECT @ClinicalcaseID,GETUTCDATE(),@UserId,3    
  
 INSERT INTO [Version] (ClinicalCaseId,VersionDate,UserId,StatusID)                          
 SELECT @ClinicalcaseID,GETUTCDATE(),@UserId,17    
  
 UPDATE WorkItem SET StatusId = 17,        
      CodedDate = GETUTCDATE()  
 WHERE ClinicalCaseId = @ClinicalCaseID  
  
END  
  
select * from workitem