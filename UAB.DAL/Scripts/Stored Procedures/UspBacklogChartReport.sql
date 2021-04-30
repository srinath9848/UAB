CREATE PROCEDURE UspBacklogChartReport --1,'PerDay'      
@ProjectId INT = NULL,      
@RangeType VARCHAR(100)      
AS      
BEGIN    
IF @ProjectId = 0     
 SET @ProjectId = NULL      
      
IF @RangeType = 'PerDay'      
BEGIN   
select   
count(cc.ClinicalCaseID) AS Total  
 ,DATEDIFF(DAY,cc.CreatedDate,wi.AssignedDate)As DelayDays  
 ,s.Name AS Status,p.Name AS ProjectName  
from   
WorkItem wi  
join clinicalcase cc on cc.ClinicalCaseID=wi.ClinicalCaseID  
join Project p on p.ProjectId=wi.ProjectId  
join status s on s.StatusId=wi.StatusId  
where  
wi.StatusId <>16 AND DATEDIFF(DAY,cc.CreatedDate,wi.AssignedDate)>=p.SLAInDays  
and p.ProjectId=@ProjectId  
and  DATEDIFF(DAY,cc.CreatedDate,wi.AssignedDate) =1  
group by DATEDIFF(DAY,cc.CreatedDate,wi.AssignedDate)  
,s.Name,p.Name   
END  
ELSE IF @RangeType = 'PerWeek'      
BEGIN   
select   
count(cc.ClinicalCaseID) AS Total  
 ,DATEDIFF(DAY,cc.CreatedDate,wi.AssignedDate)As DelayDays  
 ,s.Name AS Status,p.Name AS ProjectName  
from   
WorkItem wi  
join clinicalcase cc on cc.ClinicalCaseID=wi.ClinicalCaseID  
join Project p on p.ProjectId=wi.ProjectId  
join status s on s.StatusId=wi.StatusId  
where  
wi.StatusId <>16 AND DATEDIFF(DAY,cc.CreatedDate,wi.AssignedDate)>=p.SLAInDays  
and p.ProjectId=@ProjectId  
and  DATEDIFF(DAY,cc.CreatedDate,wi.AssignedDate) >1 and DATEDIFF(DAY,cc.CreatedDate,wi.AssignedDate) <=7  
group by DATEDIFF(DAY,cc.CreatedDate,wi.AssignedDate)  
,s.Name,p.Name   
END  
ELSE IF @RangeType = 'PerMonth'      
BEGIN   
select   
count(cc.ClinicalCaseID) AS Total  
 ,DATEDIFF(DAY,cc.CreatedDate,wi.AssignedDate)As DelayDays  
 ,s.Name AS Status,p.Name AS ProjectName  
from   
WorkItem wi  
join clinicalcase cc on cc.ClinicalCaseID=wi.ClinicalCaseID  
join Project p on p.ProjectId=wi.ProjectId  
join status s on s.StatusId=wi.StatusId  
where  
wi.StatusId <>16 AND DATEDIFF(DAY,cc.CreatedDate,wi.AssignedDate)>=p.SLAInDays  
and p.ProjectId=@ProjectId   
and  DATEDIFF(DAY,cc.CreatedDate,wi.AssignedDate) >7 and DATEDIFF(DAY,cc.CreatedDate,wi.AssignedDate) <=30  
group by DATEDIFF(DAY,cc.CreatedDate,wi.AssignedDate)  
,s.Name,p.Name   
END  
ELSE IF @RangeType = 'MorethanMonth'      
BEGIN   
select   
count(cc.ClinicalCaseID) AS Total  
 ,DATEDIFF(DAY,cc.CreatedDate,wi.AssignedDate)As DelayDays  
 ,s.Name AS Status,p.Name AS ProjectName  
from   
WorkItem wi  
join clinicalcase cc on cc.ClinicalCaseID=wi.ClinicalCaseID  
join Project p on p.ProjectId=wi.ProjectId  
join status s on s.StatusId=wi.StatusId  
where  
wi.StatusId <>16 AND DATEDIFF(DAY,cc.CreatedDate,wi.AssignedDate)>=p.SLAInDays  
and p.ProjectId=@ProjectId  
and  DATEDIFF(DAY,cc.CreatedDate,wi.AssignedDate) >30   
group by DATEDIFF(DAY,cc.CreatedDate,wi.AssignedDate)  
,s.Name,p.Name   
END  
END