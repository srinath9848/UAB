Create PROCEDURE UspAgingDetails             
@ProjectId INT = NULL,              
@ColumnName varchar(50)    
AS              
BEGIN     
select  cc.ClinicalCaseId,cc.PatientFirstName + ' ' + cc.PatientLastName AS [Name],cc.DateOfService,cc.PatientMRN,p.Name AS [Provider],s.Name as Status,  
bc.Name AS BlockCategory        
,Bh.Remarks AS BlockRemarks,Bh.CreateDate AS BlockedDate ,u.FirstName + ' ' + u.LastName AS BlockedByUser     
 from ClinicalCase cc         
join WorkItem wi on cc.ClinicalCaseId=wi.ClinicalCaseId        
LEFT JOIN BlockHistory bh ON bh.ClinicalCaseId = CC.ClinicalCaseId              
LEFT JOIN BlockCategory bc ON bc.BlockCategoryId = bh.BlockCategoryId      
left JOIN [User] u ON u.UserId = bh.BlockedByUserId  
LEFT join provider p on p.ProviderID=cc.ProviderId      
LEFT join status s on s.StatusId=wi.StatusId   
where wi.isblocked=1  
AND wi.ProjectId=3
AND (bh.CreateDate=(select top 1 CreateDate from BlockHistory where clinicalcaseid=cc.ClinicalCaseId order by 1 desc) )
order by cc.ClinicalCaseId    
END