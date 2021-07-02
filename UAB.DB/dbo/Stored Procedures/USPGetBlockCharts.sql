CREATE PROCEDURE [dbo].[USPGetBlockCharts] --1,'QA',38                      
@ProjectID INT,                                            
@Role VARCHAR(50),                                          
--@ChartType VARCHAR(50),                                          
@UserId INT                                          
AS      
BEGIN    

Declare @RoleID INT,@StatusID INT,@QueueKind Varchar(10), @CCIDs VARCHAR(max)            

IF @Role = 'Coder'          
BEGIN   
	SET @RoleID = 1
	SET @StatusID = 2
	SET @QueueKind = 'Coding'
END
ELSE IF @Role = 'QA'
BEGIN
	SET @RoleID = 2
	SET @StatusID = 5
	SET @QueueKind = 'QA'
END
ELSE IF @Role = 'ShadowQA'
BEGIN
	SET @RoleID = 3
	SET @StatusID = 9
	SET @QueueKind = 'ShadowQA'
END

               
SELECT CC.ClinicalCaseID            
    ,PatientMRN            
    ,PatientFirstName + ' ' + PatientLastName AS [Name]            
 ,DateOfService          
    ,isnull(CC.ProviderId, 0)  as ProviderId               
    ,Bc.Name AS BlockCategory            
    ,Bh.Remarks AS BlockRemarks            
    ,Bh.CreateDate AS BlockedDate            
    ,l.[Name] AS ListName 
	,P.Name AS ProjectName 
	INTO #Result         
   FROM ClinicalCase CC            
   INNER JOIN WorkItem W ON W.ClinicalCaseId = CC.ClinicalCaseId            
   LEFT JOIN List l ON l.ListId = CC.ListId  
   INNER JOIN Project P ON CC.ProjectId = P.ProjectId
   INNER JOIN ProjectUser pu ON pu.ProjectId = w.ProjectId            
   INNER JOIN BlockHistory Bh ON Bh.ClinicalCaseId = CC.ClinicalcaseId            
   INNER JOIN BlockCategory Bc ON Bh.BlockCategoryId = Bc.BlockCategoryId            
   WHERE CC.ProjectId = @ProjectID            
   AND isnull(w.AssignedTo,0) = case when @Role='Coder' then @UserId else isnull(w.AssignedTo,0) end
   AND isnull(w.QABy,0) = case when @Role='QA' then @UserId else isnull(w.QABy,0) end
   AND isnull(w.ShadowQABy,0) = case when @Role='ShadowQA' then @UserId else isnull(w.ShadowQABy,0) end
   AND ISNULL(W.IsBlocked, 0) = 1       
    AND pu.UserId = @UserId            
    AND pu.RoleId = @RoleID       
 AND W.statusid= @StatusID           
    AND pu.IsActive = 1    
 AND Bh.BlockedInQueueKind = @QueueKind          
   ORDER BY CC.ClinicalCaseID                       
                
 SELECT * FROM #Result

 SELECT @CCIDs = COALESCE(@CCIDs + ',', '') + cast(Clinicalcaseid as varchar(10))  
 FROM #Result

 SELECT @CCIDs AS CCIDs

END
  