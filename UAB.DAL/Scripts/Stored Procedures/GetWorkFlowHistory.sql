CREATE PROCEDURE GetWorkFlowHistory --1284    
@ClinicalCaseID INT      
AS      
BEGIN      
SELECT s.Name [Event]      
    ,ver.VersionDate [Date]      
    ,case   
 when ver.Userid = 0 AND ver.statusId <> 18 then 'FileProcessor'   
 when ver.Userid = 0 AND ver.statusId =18 then 'Un Assign'   
 else (u.FirstName + ' ' + u.LastName)   
 END   
 AS UserName    
    FROM Version ver      
left JOIN [User] u ON u.UserId = ver.UserId      
INNER JOIN [Status] s ON s.StatusId = ver.StatusId      
WHERE ver.ClinicalCaseId = @ClinicalCaseID      
Order by ver.VersionId      
END