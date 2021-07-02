CREATE PROCEDURE UspSubmitQAOnHoldChart      
@ClinicalCaseId INT,      
@Answer VARCHAR(200),      
@UserId INT      
AS      
BEGIN      
      
UPDATE WorkItem SET Onhold = 0       
WHERE ClinicalCaseId = @ClinicalCaseId      
      
UPDATE CoderQuestion SET Answer = @Answer,      
       AnsweredBy = @UserId,      
       AnsweredDate = GETUTCDATE()      
WHERE ClinicalCaseId = @ClinicalCaseId      
    
      
END