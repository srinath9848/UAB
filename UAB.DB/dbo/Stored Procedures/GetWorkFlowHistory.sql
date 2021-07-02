CREATE PROCEDURE GetWorkFlowHistory --1284    
@ClinicalCaseID INT      
AS      
BEGIN      
SELECT s.Name [Event]      
    ,ver.VersionDate [Date]      
    ,case   
 when ver.Userid = 0  then 'FileProcessor'   
 else (u.FirstName + ' ' + u.LastName)   
 END   
 AS UserName
 ,ver.Remarks    
    FROM Version ver      
left JOIN [User] u ON u.UserId = ver.UserId      
INNER JOIN [Status] s ON s.StatusId = ver.StatusId      
WHERE ver.ClinicalCaseId = @ClinicalCaseID      
Order by ver.VersionId      
END