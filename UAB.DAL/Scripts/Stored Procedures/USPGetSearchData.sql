CREATE PROCEDURE UspGetSearchData     
@userId int = null,
@mrn varchar(100) = null,
@fname varchar(50) = null,
@lname varchar(50) = null,
@projectname varchar(100) = null,
@providername varchar(100) = null,
@DoSFrom date = null,
@DoSTo date = null,
@StatusName varchar(100) = null,
@IsBlocked bit = 0
AS                      
BEGIN
DECLARE @DoSBegin date = null, @DoSEnd date = null

IF (@DoSFrom IS NOT NULL AND @DoSTo IS NULL)
BEGIN
	SET @DoSBegin = @DoSFrom
	SET @DoSEnd = @DOSFrom
END

IF (@DoSFrom IS NULL AND @DoSTo IS NOT NULL)
BEGIN
	SET @DoSBegin = @DoSTo
	SET @DoSEnd = @DoSTo
END

IF (@DoSFrom IS NOT NULL AND @DoSTo IS NOT NULL)
BEGIN
	SET @DoSBegin = @DoSFrom
	SET @DoSEnd = @DoSTo
END

;WITH searchcte as(                   
SELECT DISTINCT  CC.ClinicalCaseID, cc.PatientFirstName as FirstName, cc.PatientLastName as LastName,            
PatientMRN,pro.[Name] as [Provider], CAST(DateOfService AS VARCHAR(20)) AS DateOfService,           
p.[Name] as ProjectName, s.[Name] as [Status]  ,wi.IsBlocked  ,wi.AssignedTo        
, DENSE_RANK() OVER(PARTITION BY cc.ClinicalCaseId ORDER BY ISNULL(pro.[Name], 0) ) AS providerorder
from ClinicalCase cc 
JOIN WorkItem wi on cc.ClinicalCaseId=wi.ClinicalCaseId
LEFT JOIN Project p on p.ProjectId=cc.ProjectId 
LEFT JOIN [Status] s on s.StatusId=wi.StatusId 
LEFT JOIN WorkItemProvider wip on wip.ClinicalCaseId=cc.ClinicalCaseId           
LEFT JOIN [Provider] pro on pro.ProviderId=wip.ProviderId  
LEFT JOIN ProjectUser pu  on p.ProjectId = pu.ProjectId   
WHERE (@projectname IS NULL OR p.[Name] = @projectname)
AND (@DoSBegin IS NULL OR cc.DateOfService BETWEEN @DoSBegin AND @DoSEnd)
AND (@fname IS NULL OR cc.PatientFirstName = @fname)
AND (@lname IS NULL OR cc.PatientLastName = @lname)
AND (@mrn IS NULL OR cc.PatientMRN = @mrn)
AND (@providername IS NULL OR pro.[Name] = @providername)
AND (@StatusName IS NULL OR s.[Name] = @StatusName)
AND (@userId IS NULL OR wi.AssignedTo = @userId)
AND ISNULL(wi.IsBlocked, 0) = @IsBlocked
)     
SELECT * FROM searchcte WHERE providerorder=1
END
