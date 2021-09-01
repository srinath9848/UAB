CREATE PROCEDURE [dbo].[UspPostedChartDetailedReport] --NULL,'PerDay'          
@ProjectId INT = NULL,              
@RangeType VARCHAR(100),          
@Date DateTime = NULL,          
@Week INT = NULL,          
@Month VARCHAR(10) = NULL,          
@Year INT = NULL,        
@TimeZoneOffset DECIMAL(8,2) ,      
@StartDate DateTIME,          
@EndDate DateTIME,
@WeekStartDate DateTime,
@WeekEndDate DateTime
AS          
BEGIN
IF @RangeType = 'PerDay'          
BEGIN
;WITH DataSet AS (
	SELECT distinct CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId        
	,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name, ISNULL(P.Name,PP.Name) AS Provider
	,ROW_NUMBER() OVER(PARTITION BY cc.ClinicalCaseId ORDER BY Wip.VersionId DESC, Wip.WorkItemProviderId ASC) AS rn
	FROM WorkItem W           
	INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId          
	INNER JOIN Version v ON v.ClinicalCaseId = CC.ClinicalcaseId        
	LEFT JOIN List L ON L.ListId = CC.ListId        
	LEFT JOIN Provider P ON P.ProviderId = CC.ProviderId    
	LEFT JOIN WorkItemProvider Wip ON Wip.ClinicalCaseId = CC.ClinicalCaseId  
	LEFT JOIN Provider PP ON PP.ProviderID = Wip.ProviderId      
	WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))          
	AND v.StatusId in (16)        
	AND CONVERT(VARCHAR, DATEADD(Second, @TimeZoneOffset * 60 * 60, Versiondate), 1)=@Date        
)
SELECT * FROM DataSet WHERE rn = 1
END          
ELSE IF @RangeType = 'PerWeek'          
BEGIN
;WITH DataSet AS (
	SELECT distinct CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId        
	,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name, ISNULL(P.Name,PP.Name) AS Provider
	,ROW_NUMBER() OVER(PARTITION BY cc.ClinicalCaseId ORDER BY Wip.VersionId DESC, Wip.WorkItemProviderId ASC) AS rn
	FROM WorkItem W           
	INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId          
	INNER JOIN Version v ON v.ClinicalCaseId = CC.ClinicalcaseId        
	LEFT JOIN List L ON L.ListId = CC.ListId        
	LEFT JOIN Provider P ON P.ProviderId = CC.ProviderId     
	LEFT JOIN WorkItemProvider Wip ON Wip.ClinicalCaseId = CC.ClinicalCaseId  
	LEFT JOIN Provider PP ON PP.ProviderID = Wip.ProviderId     
	WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))          
	AND v.StatusId in (16)        
	AND DATEADD(Second, @TimeZoneOffset * 60 * 60, Versiondate) >= @WeekStartDate      
	AND  DATEADD(Second, @TimeZoneOffset * 60 * 60, Versiondate) <= DATEADD(Hour,23,DATEADD(MINUTE,59,DATEADD(SECOND,59,@WeekEndDate)))      
)
SELECT * FROM DataSet WHERE rn = 1
END          
ELSE IF @RangeType = 'PerMonth'          
BEGIN
;WITH DataSet AS (
	SELECT distinct CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId        
	,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name, ISNULL(P.Name,PP.Name) AS Provider
	,ROW_NUMBER() OVER(PARTITION BY cc.ClinicalCaseId ORDER BY Wip.VersionId DESC, Wip.WorkItemProviderId ASC) AS rn
	FROM WorkItem W           
	INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId          
	INNER JOIN Version v ON v.ClinicalCaseId = CC.ClinicalcaseId        
	LEFT JOIN List L ON L.ListId = CC.ListId        
	LEFT JOIN Provider P ON P.ProviderId = CC.ProviderId   
	LEFT JOIN WorkItemProvider Wip ON Wip.ClinicalCaseId = CC.ClinicalCaseId  
	LEFT JOIN Provider PP ON PP.ProviderID = Wip.ProviderId       
	WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))          
	AND v.StatusId in (16)        
	AND DATEADD(Second, @TimeZoneOffset * 60 * 60, Versiondate) >= @StartDate      
	AND  DATEADD(Second, @TimeZoneOffset * 60 * 60, Versiondate) <= DATEADD(Hour,23,DATEADD(MINUTE,59,DATEADD(SECOND,59,@Enddate)))      
	AND DATENAME(month, DATEADD(Second, @TimeZoneOffset * 60 * 60, Versiondate)) = @Month         
	AND   DATEPART(yyyy , DATEADD(Second, @TimeZoneOffset * 60 * 60, Versiondate)) = @Year        
)
SELECT * FROM DataSet WHERE rn = 1
END          
END
GO