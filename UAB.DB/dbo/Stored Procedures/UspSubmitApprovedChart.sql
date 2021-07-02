CREATE PROCEDURE [dbo].[UspSubmitApprovedChart]   
@ClinicalcaseID INT,    
@AssignedTo INT      
AS    
BEGIN  
  
Update WorkItem set StatusId=16 , AssignedDate= GETUTCDATE()   
where ClinicalCaseId=@ClinicalcaseID   
  
INSERT INTO [Version] (ClinicalCaseId,VersionDate,UserId,StatusID)                        
 SELECT @ClinicalcaseID,GETUTCDATE(),@AssignedTo,16  
  
END