
CREATE procedure [dbo].[UspGetChartCountByRole] --38,'QA'    
@UserId INT,                
@Role VARCHAR(100)                   
AS                    
BEGIN                    
                
IF @Role = 'Coder'                   
BEGIN                  
 SELECT p.ProjectId, p.[Name],                
 SUM(CASE WHEN StatusId = 1 THEN 1 ELSE 0 END) + SUM(CASE WHEN StatusId = 2 AND AssignedTo = @UserId AND ISNULL(IsBlocked, 0) = 0 THEN 1 ELSE 0 END) AvailableCharts,            
 SUM(CASE WHEN StatusId = 14 AND wi.AssignedTo = @UserId THEN 1 ELSE 0 END) IncorrectCharts,                  
 SUM(CASE WHEN StatusId = 15 AND wi.AssignedTo = @UserId THEN 1 ELSE 0 END) ReadyForPostingCharts,          
 SUM(CASE WHEN AssignedTo = @UserId AND StatusId = 2 AND ISNULL(IsBlocked, 0) = 1 THEN 1 ELSE 0 END) BlockedCharts,                    
 0 AS ShadowQARebuttalCharts,                  
 0 AS CoderRebuttalCharts                  
 FROM Project p    
 JOIN ProjectUser pu    
 ON p.ProjectId = pu.ProjectId    
 LEFT JOIN WorkItem wi    
 ON wi.ProjectId = pu.ProjectId               
 WHERE pu.UserId = @UserId AND pu.RoleId = 1 AND pu.IsActive = 1                
 GROUP BY p.ProjectId, p.[Name]                
 ORDER BY P.ProjectId                
END                  
ELSE IF @Role = 'QA'                  
BEGIN                  
 SELECT P.ProjectId,P.[Name],                  
 SUM(CASE WHEN StatusId = 4 AND IsQA = 1 THEN 1 ELSE 0 END) + SUM(CASE WHEN StatusId = 5 AND ISNULL(IsBlocked, 0) = 0 AND QABy = @UserId THEN 1 ELSE 0 END) AvailableCharts,                  
 SUM(CASE WHEN StatusId = 12 AND wi.QABy =@UserId THEN 1 ELSE 0 END) CoderRebuttalCharts,                  
 SUM(CASE WHEN StatusId = 11 AND wi.QABy =@UserId THEN 1 ELSE 0 END) ShadowQARebuttalCharts,                  
 SUM(CASE WHEN QABy = @UserId AND StatusId = 5 AND ISNULL(IsBlocked, 0) = 1 THEN 1 ELSE 0 END) BlockedCharts,         
 0 AS ReadyForPostingCharts,                  
 0 AS IncorrectCharts                  
 FROM Project p    
 JOIN ProjectUser pu    
 ON p.ProjectId = pu.ProjectId    
 LEFT JOIN WorkItem wi    
 ON wi.ProjectId = pu.ProjectId    
 WHERE pu.UserId = @UserId AND pu.RoleId = 2 AND pu.IsActive = 1                
 GROUP BY p.ProjectId, p.[Name]                
 ORDER BY P.ProjectId               
                   
END                  
ELSE IF @Role = 'ShadowQA'                  
BEGIN                  
 SELECT P.ProjectId,P.Name,                  
 SUM(CASE WHEN StatusId = 8 AND IsShadowQA = 1 THEN 1 ELSE 0 END) + SUM(CASE WHEN StatusId = 9 AND ISNULL(IsBlocked, 0) = 0 AND ShadowQABy = @UserId THEN 1 ELSE 0 END) AvailableCharts,                  
 SUM(CASE WHEN StatusId = 13 AND wi.ShadowQABy = @UserId THEN 1 ELSE 0 END) IncorrectCharts,                  
 SUM(CASE WHEN ShadowQABy = @UserId AND StatusId = 9 AND ISNULL(IsBlocked, 0) = 1 THEN 1 ELSE 0 END) BlockedCharts,          
 0 AS ReadyForPostingCharts,                  
 0 AS CoderRebuttalCharts,                  
 0 AS ShadowQARebuttalCharts                  
 FROM Project p    
 JOIN ProjectUser pu    
 ON p.ProjectId = pu.ProjectId    
 LEFT JOIN WorkItem wi    
 ON wi.ProjectId = pu.ProjectId    
 WHERE pu.UserId = @UserId AND pu.RoleId = 3 AND pu.IsActive = 1                
 GROUP BY p.ProjectId, p.[Name]                
 ORDER BY P.ProjectId                
    
END    
END 
