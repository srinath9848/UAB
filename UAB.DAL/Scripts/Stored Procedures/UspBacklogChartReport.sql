ALTER PROCEDURE [dbo].[UspBacklogChartReport] --NULL,'PerDay'          
@ProjectId INT = NULL,          
@RangeType VARCHAR(100),
@StartDate Date,
@EndDate Date
AS          
BEGIN        
IF @ProjectId = 0         
 SET @ProjectId = NULL          
          
IF @RangeType = 'PerDay'          
BEGIN       
select       
count(cc.ClinicalCaseID) AS Total      
 ,DATEDIFF(DAY,cc.DateOfService,GETDATE())As DelayDays      
 ,s.Name AS Status,p.Name AS ProjectName      
from       
WorkItem wi      
join clinicalcase cc on cc.ClinicalCaseID=wi.ClinicalCaseID      
join Project p on p.ProjectId=wi.ProjectId      
join status s on s.StatusId=wi.StatusId      
where      
wi.StatusId <>16 AND DATEDIFF(DAY,cc.DateOfService,GETDATE())>=p.SLAInDays      
and p.ProjectId=@ProjectId      
and  DATEDIFF(DAY,cc.DateOfService,GETDATE()) =1
AND (cc.DateOfService >= @StartDate OR (cc.DateOfService = ISNULL(@StartDate,cc.DateOfService)))
 AND (cc.DateOfService <= @EndDate OR (cc.DateOfService = ISNULL(@EndDate,cc.DateOfService)))
group by DATEDIFF(DAY,cc.DateOfService,GETDATE())      
,s.Name,p.Name       
END      
ELSE IF @RangeType = 'PerWeek'          
BEGIN       
select       
count(cc.ClinicalCaseID) AS Total      
 ,DATEDIFF(DAY,cc.DateOfService,GETDATE())As DelayDays      
 ,s.Name AS Status,p.Name AS ProjectName      
from       
WorkItem wi      
join clinicalcase cc on cc.ClinicalCaseID=wi.ClinicalCaseID      
join Project p on p.ProjectId=wi.ProjectId      
join status s on s.StatusId=wi.StatusId      
where      
wi.StatusId <>16 AND DATEDIFF(DAY,cc.DateOfService,GETDATE())>=p.SLAInDays      
and p.ProjectId=@ProjectId      
and DATEDIFF(DAY,cc.DateOfService,GETDATE()) <=7
AND (cc.DateOfService >= @StartDate OR (cc.DateOfService = ISNULL(@StartDate,cc.DateOfService)))
 AND (cc.DateOfService <= @EndDate OR (cc.DateOfService = ISNULL(@EndDate,cc.DateOfService)))
group by DATEDIFF(DAY,cc.DateOfService,GETDATE())      
,s.Name,p.Name       
END      
ELSE IF @RangeType = 'PerMonth'          
BEGIN       
select       
count(cc.ClinicalCaseID) AS Total      
 ,DATEDIFF(DAY,cc.DateOfService,GETDATE())As DelayDays      
 ,s.Name AS Status,p.Name AS ProjectName      
from       
WorkItem wi      
join clinicalcase cc on cc.ClinicalCaseID=wi.ClinicalCaseID      
join Project p on p.ProjectId=wi.ProjectId      
join status s on s.StatusId=wi.StatusId      
where      
wi.StatusId <>16 AND DATEDIFF(DAY,cc.DateOfService,GETDATE())>=p.SLAInDays      
and p.ProjectId=@ProjectId       
and  DATEDIFF(DAY,cc.DateOfService,GETDATE()) <=30
AND (cc.DateOfService >= @StartDate OR (cc.DateOfService = ISNULL(@StartDate,cc.DateOfService)))
 AND (cc.DateOfService <= @EndDate OR (cc.DateOfService = ISNULL(@EndDate,cc.DateOfService)))
group by DATEDIFF(DAY,cc.DateOfService,GETDATE())      
,s.Name,p.Name       
END      
ELSE IF @RangeType = 'MorethanMonth'          
BEGIN       
select       
count(cc.ClinicalCaseID) AS Total      
 ,DATEDIFF(DAY,cc.DateOfService,GETDATE())As DelayDays      
 ,s.Name AS Status,p.Name AS ProjectName      
from       
WorkItem wi      
join clinicalcase cc on cc.ClinicalCaseID=wi.ClinicalCaseID      
join Project p on p.ProjectId=wi.ProjectId      
join status s on s.StatusId=wi.StatusId      
where      
wi.StatusId <>16 AND DATEDIFF(DAY,cc.DateOfService,GETDATE())>=p.SLAInDays      
and p.ProjectId=@ProjectId      
and  DATEDIFF(DAY,cc.DateOfService,GETDATE()) >30
AND (cc.DateOfService >= @StartDate OR (cc.DateOfService = ISNULL(@StartDate,cc.DateOfService)))
 AND (cc.DateOfService <= @EndDate OR (cc.DateOfService = ISNULL(@EndDate,cc.DateOfService)))
group by DATEDIFF(DAY,cc.DateOfService,GETDATE())      
,s.Name,p.Name       
END      
END
GO