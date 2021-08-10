CREATE PROCEDURE [dbo].[UspCodedChartDetailedReport] --1,'PerWeek',23,'1/1/0001 12:00:00 AM',null,'June',null,5.5              
@ProjectId INT = NULL,              
@RangeType VARCHAR(100),     
@Date DATETIME,      
@Week INT = NULL,          
@Month VARCHAR(10) = NULL,          
@Year INT = NULL ,    
@TimeZoneOffset DECIMAL(8,2) ,    
@StartDate DateTIME,        
@EndDate DateTIME         
AS              
BEGIN              
            
IF @ProjectId = 0             
 SET @ProjectId = NULL              
              
IF @RangeType = 'PerDay'              
BEGIN
;WITH DataSet AS (
	SELECT CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId        
	,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name, ISNULL(P.Name,PP.Name) AS Provider
	,ROW_NUMBER() OVER(PARTITION BY cc.ClinicalCaseId ORDER BY Wip.VersionId DESC, Wip.WorkItemProviderId ASC) AS rn
	FROM WorkItem W               
	INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId        
	LEFT JOIN List L ON L.ListId = CC.ListId        
	LEFT JOIN Provider P ON P.ProviderId = CC.ProviderId    
	LEFT JOIN WorkItemProvider Wip ON Wip.ClinicalCaseId = CC.ClinicalCaseId
	LEFT JOIN Provider PP ON PP.ProviderID = Wip.ProviderId    
	WHERE (W.ProjectId = @ProjectId OR W.ProjectID = ISNULL(@ProjectId,W.ProjectId))            
	AND W.StatusId NOT IN (1,2)        
	AND CONVERT(VARCHAR, DATEADD(Second, @TimeZoneOffset * 60 * 60, CodedDate), 1) = @Date    
)
SELECT * FROM DataSet WHERE rn = 1          
END              
ELSE IF @RangeType = 'PerWeek'              
BEGIN
;WITH DataSet AS (
	SELECT CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId        
	,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name, ISNULL(P.Name,PP.Name) AS Provider
	,ROW_NUMBER() OVER(PARTITION BY cc.ClinicalCaseId ORDER BY Wip.VersionId DESC, Wip.WorkItemProviderId ASC) AS rn
	FROM WorkItem W               
	INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId        
	LEFT JOIN List L ON L.ListId = CC.ListId        
	LEFT JOIN Provider P ON P.ProviderId = CC.ProviderId       
	LEFT JOIN WorkItemProvider Wip ON Wip.ClinicalCaseId = CC.ClinicalCaseId
	LEFT JOIN Provider PP ON PP.ProviderID = Wip.ProviderId 
	WHERE (W.ProjectId = @ProjectId OR (W.ProjectID = ISNULL(@ProjectId,W.ProjectId)))            
	AND W.StatusId NOT IN (1,2)      
	AND DATEADD(Second, @TimeZoneOffset * 60 * 60, CodedDate) >= @StartDate    
	AND  DATEADD(Second, @TimeZoneOffset * 60 * 60, CodedDate) <= DATEADD(Hour,23,DATEADD(MINUTE,59,DATEADD(SECOND,59,@Enddate)))    
	AND DATEPART(ISO_WEEK, DATEADD(Second, @TimeZoneOffset * 60 * 60, CodedDate)) = @Week             
	AND  DATENAME(month, DATEADD(Second, @TimeZoneOffset * 60 * 60, CodedDate)) = @Month            
	AND  DATEPART(yyyy , DATEADD(Second, @TimeZoneOffset * 60 * 60, CodedDate)) = @Year        
)
SELECT * FROM DataSet WHERE rn = 1
END              
ELSE IF @RangeType = 'PerMonth'              
BEGIN
;WITH DataSet AS (
	SELECT CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId        
	,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name, ISNULL(P.Name,PP.Name) AS Provider
	,ROW_NUMBER() OVER(PARTITION BY cc.ClinicalCaseId ORDER BY Wip.VersionId DESC, Wip.WorkItemProviderId ASC) AS rn
	FROM WorkItem W               
	INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId        
	LEFT JOIN List L ON L.ListId = CC.ListId        
	LEFT JOIN Provider P ON P.ProviderId = CC.ProviderId     
	LEFT JOIN WorkItemProvider Wip ON Wip.ClinicalCaseId = CC.ClinicalCaseId
	LEFT JOIN Provider PP ON PP.ProviderID = Wip.ProviderId   
	WHERE W.ProjectId = @ProjectId            
	AND W.StatusId NOT IN (1,2)       
	AND DATEADD(Second, @TimeZoneOffset * 60 * 60, CodedDate) >= @StartDate    
	AND  DATEADD(Second, @TimeZoneOffset * 60 * 60, CodedDate) <= DATEADD(Hour,23,DATEADD(MINUTE,59,DATEADD(SECOND,59,@Enddate)))    
	AND DATENAME(month, DATEADD(Second, @TimeZoneOffset * 60 * 60, CodedDate)) = @Month        
	AND DATEPART(yyyy, DATEADD(Second, @TimeZoneOffset * 60 * 60, CodedDate)) = @Year          
)
SELECT * FROM DataSet WHERE rn = 1
END              
              
END