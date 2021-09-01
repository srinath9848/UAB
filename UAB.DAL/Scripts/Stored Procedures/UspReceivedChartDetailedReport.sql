CREATE PROCEDURE [dbo].[UspReceivedChartDetailedReport]-- NULL,'PerWeek'          
@ProjectId INT = NULL,            
@RangeType VARCHAR(100),        
@Date DateTime2 = NULL,        
@Week INT = NULL,        
@Month VARCHAR(10) = NULL,        
@Year INT = NULL ,      
@TimeZoneOffset DECIMAL(8,2) ,      
@StartDate DateTIME,          
@EndDate DateTIME,
@WeekStartDate DateTime,
@WeekEndDate DateTime
AS          
BEGIN          
        
IF @ProjectId = 0         
 SET @ProjectId = NULL          
          
IF @RangeType = 'PerDay'          
BEGIN
;WITH DataSet AS (      
	SELECT CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId      
	,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name,
	ISNULL(P.Name,PP.Name) AS Provider
	,ROW_NUMBER() OVER(PARTITION BY cc.ClinicalCaseId ORDER BY Wip.VersionId DESC, Wip.WorkItemProviderId ASC) AS rn
	FROM WorkItem W           
	INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId      
	LEFT JOIN List L ON L.ListId = CC.ListId      
	LEFT JOIN Provider P ON P.ProviderId = CC.ProviderId  
	LEFT JOIN WorkItemProvider Wip ON Wip.ClinicalCaseId = CC.ClinicalCaseId
	LEFT JOIN Provider PP ON PP.ProviderID = Wip.ProviderId
	WHERE W.ProjectId = @ProjectId
	--AND W.StatusId > 2
	AND CONVERT(VARCHAR, DATEADD(Second, @TimeZoneOffset * 60 * 60, CreatedDate), 1) = @Date      
)
SELECT * FROM DataSet WHERE rn = 1
END          
ELSE IF @RangeType = 'PerWeek'          
BEGIN
;WITH DataSet AS (
	SELECT CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId      
	,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name,ISNULL(P.Name,PP.Name) AS Provider
	,ROW_NUMBER() OVER(PARTITION BY cc.ClinicalCaseId ORDER BY Wip.VersionId DESC, Wip.WorkItemProviderId ASC) AS rn
	FROM WorkItem W           
	INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId      
	LEFT JOIN List L ON L.ListId = CC.ListId      
	LEFT JOIN Provider P ON P.ProviderId = CC.ProviderId  
	LEFT JOIN WorkItemProvider Wip ON Wip.ClinicalCaseId = CC.ClinicalCaseId
	LEFT JOIN Provider PP ON PP.ProviderID = Wip.ProviderId     
	WHERE W.ProjectId = @ProjectId
	--AND W.StatusId > 2
	AND DATEADD(Second, @TimeZoneOffset * 60 * 60, CreatedDate) >= @WeekStartDate      
	AND  DATEADD(Second, @TimeZoneOffset * 60 * 60, CreatedDate) <= DATEADD(Hour,23,DATEADD(MINUTE,59,DATEADD(SECOND,59,@WeekEndDate)))      
)
SELECT * FROM DataSet WHERE rn = 1
END          
ELSE IF @RangeType = 'PerMonth'          
BEGIN
;WITH DataSet AS (
	SELECT CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId      
	,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name,ISNULL(P.Name,PP.Name) AS Provider
	,ROW_NUMBER() OVER(PARTITION BY cc.ClinicalCaseId ORDER BY Wip.VersionId DESC, Wip.WorkItemProviderId ASC) AS rn
	FROM WorkItem W           
	INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId      
	LEFT JOIN List L ON L.ListId = CC.ListId      
	LEFT JOIN Provider P ON P.ProviderId = CC.ProviderId      
	LEFT JOIN WorkItemProvider Wip ON Wip.ClinicalCaseId = CC.ClinicalCaseId
	LEFT JOIN Provider PP ON PP.ProviderID = Wip.ProviderId
	WHERE W.ProjectId = @ProjectId
	--AND W.StatusId > 2
	AND DATEADD(Second, @TimeZoneOffset * 60 * 60, CreatedDate) >= @StartDate      
	AND  DATEADD(Second, @TimeZoneOffset * 60 * 60, CreatedDate) <= DATEADD(Hour,23,DATEADD(MINUTE,59,DATEADD(SECOND,59,@Enddate)))      
	AND DATENAME(month, DATEADD(Second, @TimeZoneOffset * 60 * 60, CreatedDate)) = @Month          
	AND   DATEPART(yyyy , DATEADD(Second, @TimeZoneOffset * 60 * 60, CreatedDate)) = @Year
)
SELECT * FROM DataSet WHERE rn = 1
END          
          
END
GO