
CREATE PROCEDURE [dbo].[Rpt_GenerateChartSummaryReportDetails] (@ProjectId INT,@CurrentDos date,@ColumnName varchar(50))        
AS        
BEGIN      
IF @ColumnName = 'Received'    
BEGIN  
SELECT cc.DateOfService,cc.ClinicalCaseId,cc.PatientFirstName + ' ' + cc.PatientLastName AS [Name],cc.PatientMRN  
FROM ClinicalCase cc      
JOIN WorkItem wi      
ON wi.ClinicalCaseId = cc.ClinicalCaseId      
JOIN Project p      
ON p.ProjectId = wi.ProjectId      
LEFT JOIN BlockHistory bh      
ON bh.ClinicalCaseId = cc.ClinicalCaseId      
LEFT JOIN BlockCategory bc      
ON bc.BlockCategoryId = bh.BlockCategoryId      
WHERE cc.ProjectId = @ProjectId      
AND cc.DateOfService = @CurrentDos     
END    
ELSE IF @ColumnName = 'Coded'    
BEGIN  
SELECT cc.DateOfService,cc.ClinicalCaseId,cc.PatientFirstName + ' ' + cc.PatientLastName AS [Name],cc.PatientMRN  
FROM ClinicalCase cc      
JOIN WorkItem wi      
ON wi.ClinicalCaseId = cc.ClinicalCaseId      
JOIN Project p      
ON p.ProjectId = wi.ProjectId      
LEFT JOIN BlockHistory bh      
ON bh.ClinicalCaseId = cc.ClinicalCaseId      
LEFT JOIN BlockCategory bc      
ON bc.BlockCategoryId = bh.BlockCategoryId      
WHERE cc.ProjectId = @ProjectId      
AND cc.DateOfService = @CurrentDos     
AND wi.StatusId = 3      
END  
ELSE IF @ColumnName = 'Posted'    
BEGIN  
SELECT cc.DateOfService,cc.ClinicalCaseId,cc.PatientFirstName + ' ' + cc.PatientLastName AS [Name],cc.PatientMRN      
FROM ClinicalCase cc      
JOIN WorkItem wi      
ON wi.ClinicalCaseId = cc.ClinicalCaseId      
JOIN Project p      
ON p.ProjectId = wi.ProjectId      
LEFT JOIN BlockHistory bh      
ON bh.ClinicalCaseId = cc.ClinicalCaseId      
LEFT JOIN BlockCategory bc      
ON bc.BlockCategoryId = bh.BlockCategoryId      
WHERE cc.ProjectId = @ProjectId      
AND cc.DateOfService = @CurrentDos     
AND wi.StatusId = 16  
END  
ELSE IF @ColumnName = 'ProviderPosted'    
BEGIN  
SELECT cc.DateOfService,cc.ClinicalCaseId,cc.PatientFirstName + ' ' + cc.PatientLastName AS [Name],cc.PatientMRN       
FROM ClinicalCase cc      
JOIN WorkItem wi      
ON wi.ClinicalCaseId = cc.ClinicalCaseId      
JOIN Project p      
ON p.ProjectId = wi.ProjectId      
LEFT JOIN BlockHistory bh      
ON bh.ClinicalCaseId = cc.ClinicalCaseId      
LEFT JOIN BlockCategory bc      
ON bc.BlockCategoryId = bh.BlockCategoryId      
WHERE cc.ProjectId = @ProjectId      
AND cc.DateOfService = @CurrentDos  
AND wi.StatusId = 17  
END 
ELSE IF @ColumnName = 'TotalPosted'    
BEGIN  
SELECT cc.DateOfService,cc.ClinicalCaseId,cc.PatientFirstName + ' ' + cc.PatientLastName AS [Name],cc.PatientMRN       
FROM ClinicalCase cc      
JOIN WorkItem wi      
ON wi.ClinicalCaseId = cc.ClinicalCaseId      
JOIN Project p      
ON p.ProjectId = wi.ProjectId      
LEFT JOIN BlockHistory bh      
ON bh.ClinicalCaseId = cc.ClinicalCaseId      
LEFT JOIN BlockCategory bc      
ON bc.BlockCategoryId = bh.BlockCategoryId      
WHERE cc.ProjectId = @ProjectId      
AND cc.DateOfService = @CurrentDos  
AND wi.StatusId in (16,17)
END
ELSE IF @ColumnName = 'InternalBlocked'    
BEGIN  
SELECT cc.DateOfService,cc.ClinicalCaseId,cc.PatientFirstName + ' ' + cc.PatientLastName AS [Name],cc.PatientMRN      
FROM ClinicalCase cc      
JOIN WorkItem wi      
ON wi.ClinicalCaseId = cc.ClinicalCaseId      
JOIN Project p      
ON p.ProjectId = wi.ProjectId      
LEFT JOIN BlockHistory bh      
ON bh.ClinicalCaseId = cc.ClinicalCaseId      
LEFT JOIN BlockCategory bc      
ON bc.BlockCategoryId = bh.BlockCategoryId      
WHERE cc.ProjectId = @ProjectId      
AND cc.DateOfService = @CurrentDos  
AND wi.IsBlocked = 1 AND ISNULL(bc.BlockType, '') = 'Internal'  
END  
ELSE IF @ColumnName = 'ExternalBlocked'    
BEGIN  
SELECT cc.DateOfService,cc.ClinicalCaseId,cc.PatientFirstName + ' ' + cc.PatientLastName AS [Name],cc.PatientMRN      
FROM ClinicalCase cc      
JOIN WorkItem wi      
ON wi.ClinicalCaseId = cc.ClinicalCaseId      
JOIN Project p      
ON p.ProjectId = wi.ProjectId      
LEFT JOIN BlockHistory bh      
ON bh.ClinicalCaseId = cc.ClinicalCaseId      
LEFT JOIN BlockCategory bc      
ON bc.BlockCategoryId = bh.BlockCategoryId      
WHERE cc.ProjectId = @ProjectId      
AND cc.DateOfService = @CurrentDos     
AND wi.IsBlocked = 1 AND ISNULL(bc.BlockType, '') = 'External'  
END  
ELSE IF @ColumnName = 'NotCoded'    
BEGIN  
SELECT cc.DateOfService,cc.ClinicalCaseId,cc.PatientFirstName + ' ' + cc.PatientLastName AS [Name],cc.PatientMRN      
FROM ClinicalCase cc      
JOIN WorkItem wi      
ON wi.ClinicalCaseId = cc.ClinicalCaseId      
JOIN Project p      
ON p.ProjectId = wi.ProjectId      
LEFT JOIN BlockHistory bh      
ON bh.ClinicalCaseId = cc.ClinicalCaseId      
LEFT JOIN BlockCategory bc      
ON bc.BlockCategoryId = bh.BlockCategoryId      
WHERE cc.ProjectId = @ProjectId      
AND cc.DateOfService = @CurrentDos     
AND wi.StatusId IN (1, 2) AND wi.IsBlocked = 0  
END  
ELSE IF @ColumnName = 'NotAudited'    
BEGIN  
SELECT cc.DateOfService,cc.ClinicalCaseId,cc.PatientFirstName + ' ' + cc.PatientLastName AS [Name],cc.PatientMRN      
FROM ClinicalCase cc      
JOIN WorkItem wi      
ON wi.ClinicalCaseId = cc.ClinicalCaseId      
JOIN Project p      
ON p.ProjectId = wi.ProjectId      
LEFT JOIN BlockHistory bh      
ON bh.ClinicalCaseId = cc.ClinicalCaseId      
LEFT JOIN BlockCategory bc      
ON bc.BlockCategoryId = bh.BlockCategoryId      
WHERE cc.ProjectId = @ProjectId      
AND cc.DateOfService = @CurrentDos     
AND wi.StatusId > 3 AND wi.StatusId < 15 AND wi.IsBlocked = 0  
END  
ELSE IF @ColumnName = 'NotPosted'    
BEGIN  
SELECT cc.DateOfService,cc.ClinicalCaseId,cc.PatientFirstName + ' ' + cc.PatientLastName AS [Name],cc.PatientMRN      
FROM ClinicalCase cc      
JOIN WorkItem wi      
ON wi.ClinicalCaseId = cc.ClinicalCaseId      
JOIN Project p      
ON p.ProjectId = wi.ProjectId      
LEFT JOIN BlockHistory bh      
ON bh.ClinicalCaseId = cc.ClinicalCaseId      
LEFT JOIN BlockCategory bc      
ON bc.BlockCategoryId = bh.BlockCategoryId      
WHERE cc.ProjectId = @ProjectId      
AND cc.DateOfService = @CurrentDos     
AND wi.StatusId = 15 AND wi.IsBlocked = 0  
END 
END 