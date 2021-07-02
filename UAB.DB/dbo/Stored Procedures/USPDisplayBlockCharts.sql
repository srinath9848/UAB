CREATE PROCEDURE [dbo].[USPDisplayBlockCharts]          
 --1,'Coder',3,1                                
 @ProjectID INT          
 ,@Role VARCHAR(50)    
 ,@UserId INT          
AS          
BEGIN     
IF @Role = 'Coder'    
  BEGIN      
select  cc.ClinicalCaseId,cc.PatientFirstName + ' ' + cc.PatientLastName AS [Name],cc.DateOfService,cc.PatientMRN,bc.Name AS BlockCategory    
,Bh.Remarks AS BlockRemarks,Bh.CreateDate AS BlockedDate,l.[Name] AS ListName ,CC.ProviderId     
from ClinicalCase cc     
join WorkItem wi on cc.ClinicalCaseId=wi.ClinicalCaseId    
LEFT JOIN BlockHistory bh ON bh.ClinicalCaseId = CC.ClinicalCaseId          
LEFT JOIN BlockCategory bc ON bc.BlockCategoryId = bh.BlockCategoryId   
LEFT JOIN List l ON l.ListId = CC.ListId        
where    
wi.AssignedTo=@UserId and wi.projectid=@ProjectID and wi.QABy is NULL and wi.ShadowQABy is NULL and wi.IsBlocked=1 and wi.StatusId=2 AND bh.BlockedInQueueKind='coding'      
order by bh.BlockHistoryid desc    
  END     
  ELSE IF @Role = 'QA'    
  BEGIN    
select  cc.ClinicalCaseId,cc.PatientFirstName + ' ' + cc.PatientLastName AS [Name],cc.DateOfService,cc.PatientMRN,bc.Name AS BlockCategory    
,Bh.Remarks AS BlockRemarks,Bh.CreateDate AS BlockedDate ,l.[Name] AS ListName  
 from ClinicalCase cc     
join WorkItem wi on cc.ClinicalCaseId=wi.ClinicalCaseId    
LEFT JOIN BlockHistory bh ON bh.ClinicalCaseId = CC.ClinicalCaseId          
LEFT JOIN BlockCategory bc ON bc.BlockCategoryId = bh.BlockCategoryId     
LEFT JOIN List l ON l.ListId = CC.ListId      
where    
wi.QABy=@UserId and wi.projectid=@ProjectID and wi.ShadowQABy is NULL and wi.IsBlocked=1 and wi.StatusId=5   AND bh.BlockedInQueueKind='QA'   
order by bh.BlockHistoryid desc    
  END    
  ELSE IF @Role = 'ShadowQA'    
  BEGIN    
select  cc.ClinicalCaseId,cc.PatientFirstName + ' ' + cc.PatientLastName AS [Name],cc.DateOfService,cc.PatientMRN,bc.Name AS BlockCategory    
,Bh.Remarks AS BlockRemarks,Bh.CreateDate AS BlockedDate,l.[Name] AS ListName  
from ClinicalCase cc     
join WorkItem wi on cc.ClinicalCaseId=wi.ClinicalCaseId    
LEFT JOIN BlockHistory bh ON bh.ClinicalCaseId = CC.ClinicalCaseId          
LEFT JOIN BlockCategory bc ON bc.BlockCategoryId = bh.BlockCategoryId   
LEFT JOIN List l ON l.ListId = CC.ListId        
where    
wi.projectid=@ProjectID and wi.ShadowQABy=@UserId and wi.IsBlocked=1 and wi.StatusId=9 AND bh.BlockedInQueueKind='ShadowQA'
order by bh.BlockHistoryid desc    
  END    
END 