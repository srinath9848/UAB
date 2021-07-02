
CREATE PROCEDURE [dbo].[Rpt_GenerateChartSummaryReportDetails] (@ProjectId INT,@CurrentDos date,@ColumnName varchar(50)) 
AS                
BEGIN              
IF @ColumnName = 'Received'            
BEGIN          
SELECT distinct CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId    
 ,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name,ISNULL(Pro.Name,PP.Name) AS Provider    
FROM ClinicalCase cc              
JOIN WorkItem wi              
ON wi.ClinicalCaseId = cc.ClinicalCaseId              
JOIN Project p              
ON p.ProjectId = wi.ProjectId              
LEFT JOIN BlockHistory bh              
ON bh.ClinicalCaseId = cc.ClinicalCaseId              
LEFT JOIN BlockCategory bc              
ON bc.BlockCategoryId = bh.BlockCategoryId    
LEFT JOIN List L ON L.ListId = CC.ListId    
 LEFT JOIN Provider Pro ON Pro.ProviderId = cc.ProviderId    
 LEFT JOIN WorkItemProvider Wip ON Wip.ClinicalCaseId = CC.ClinicalCaseId
 LEFT JOIN Provider PP ON PP.ProviderID = Wip.ProviderId
WHERE cc.ProjectId = @ProjectId              
AND cc.DateOfService = @CurrentDos             
END            
ELSE IF @ColumnName = 'Coded'            
BEGIN          
SELECT distinct CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId    
 ,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name,ISNULL(Pro.Name,PP.Name) AS Provider    
FROM ClinicalCase cc              
JOIN WorkItem wi              
ON wi.ClinicalCaseId = cc.ClinicalCaseId              
JOIN Project p              
ON p.ProjectId = wi.ProjectId              
LEFT JOIN BlockHistory bh              
ON bh.ClinicalCaseId = cc.ClinicalCaseId              
LEFT JOIN BlockCategory bc              
ON bc.BlockCategoryId = bh.BlockCategoryId    
LEFT JOIN List L ON L.ListId = CC.ListId    
 LEFT JOIN Provider Pro ON Pro.ProviderId = CC.ProviderId    
 LEFT JOIN WorkItemProvider Wip ON Wip.ClinicalCaseId = CC.ClinicalCaseId
 LEFT JOIN Provider PP ON PP.ProviderID = Wip.ProviderId
WHERE cc.ProjectId = @ProjectId              
AND cc.DateOfService = @CurrentDos             
AND wi.StatusId = 4              
END          
ELSE IF @ColumnName = 'Posted'            
BEGIN          
SELECT distinct CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId    
 ,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name,ISNULL(Pro.Name,PP.Name) AS Provider            
FROM ClinicalCase cc              
JOIN WorkItem wi              
ON wi.ClinicalCaseId = cc.ClinicalCaseId              
JOIN Project p              
ON p.ProjectId = wi.ProjectId              
LEFT JOIN BlockHistory bh              
ON bh.ClinicalCaseId = cc.ClinicalCaseId              
LEFT JOIN BlockCategory bc              
ON bc.BlockCategoryId = bh.BlockCategoryId    
LEFT JOIN List L ON L.ListId = CC.ListId    
 LEFT JOIN Provider Pro ON Pro.ProviderId = CC.ProviderId 
 LEFT JOIN WorkItemProvider Wip ON Wip.ClinicalCaseId = CC.ClinicalCaseId
 LEFT JOIN Provider PP ON PP.ProviderID = Wip.ProviderId   
WHERE cc.ProjectId = @ProjectId              
AND cc.DateOfService = @CurrentDos             
AND wi.StatusId = 16          
END          
ELSE IF @ColumnName = 'ProviderPosted'            
BEGIN          
SELECT distinct CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId    
 ,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name
 --, ISNULL(P.Name,Pro.Name) AS Provider
 ,Pr.[Name] AS [Provider]
FROM ClinicalCase cc              
JOIN WorkItem wi              
ON wi.ClinicalCaseId = cc.ClinicalCaseId              
JOIN Project p              
ON p.ProjectId = wi.ProjectId              
LEFT JOIN BlockHistory bh              
ON bh.ClinicalCaseId = cc.ClinicalCaseId              
LEFT JOIN BlockCategory bc              
ON bc.BlockCategoryId = bh.BlockCategoryId    
LEFT JOIN List L ON L.ListId = CC.ListId    
 INNER JOIN ProviderPosted PP ON PP.ClinicalCaseId=CC.ClinicalCaseId
 LEFT JOIN Provider Pr ON Pr.ProviderId = PP.ProviderId
 -- LEFT JOIN WorkItemProvider Wip ON Wip.ClinicalCaseId = CC.ClinicalCaseId
 --LEFT JOIN Provider Pro ON Pro.ProviderID = Wip.ProviderId 
WHERE cc.ProjectId = @ProjectId              
AND cc.DateOfService = @CurrentDos          
AND wi.StatusId = 17          
END         
ELSE IF @ColumnName = 'TotalPosted'            
BEGIN          
SELECT distinct CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId    
 ,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name,ISNULL(Pr.Name, ISNULL(PP.Name ,Pro.Name)) AS Provider    
FROM ClinicalCase cc              
JOIN WorkItem wi         
ON wi.ClinicalCaseId = cc.ClinicalCaseId              
JOIN Project p              
ON p.ProjectId = wi.ProjectId              
LEFT JOIN BlockHistory bh              
ON bh.ClinicalCaseId = cc.ClinicalCaseId              
LEFT JOIN BlockCategory bc              
ON bc.BlockCategoryId = bh.BlockCategoryId    
LEFT JOIN List L ON L.ListId = CC.ListId    
 LEFT JOIN Provider Pro ON Pro.ProviderId = CC.ProviderId    
 LEFT JOIN WorkItemProvider Wip ON Wip.ClinicalCaseId = CC.ClinicalCaseId
 LEFT JOIN Provider PP ON PP.ProviderID = Wip.ProviderId
 LEFT JOIN ProviderPosted PrP ON PrP.ClinicalCaseId=CC.ClinicalCaseId
 LEFT JOIN Provider Pr ON Pr.ProviderId = PrP.ProviderId
WHERE cc.ProjectId = @ProjectId              
AND cc.DateOfService = @CurrentDos          
AND wi.StatusId in (16,17)        
END        
ELSE IF @ColumnName = 'InternalBlocked'            
BEGIN          
SELECT distinct CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId    
 ,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name,ISNULL(Pro.Name,PP.Name) AS Provider    
FROM ClinicalCase cc              
JOIN WorkItem wi              
ON wi.ClinicalCaseId = cc.ClinicalCaseId              
JOIN Project p              
ON p.ProjectId = wi.ProjectId              
LEFT JOIN BlockHistory bh              
ON bh.ClinicalCaseId = cc.ClinicalCaseId              
LEFT JOIN BlockCategory bc              
ON bc.BlockCategoryId = bh.BlockCategoryId    
LEFT JOIN List L ON L.ListId = CC.ListId    
 LEFT JOIN Provider Pro ON Pro.ProviderId = CC.ProviderId   
 LEFT JOIN WorkItemProvider Wip ON Wip.ClinicalCaseId = CC.ClinicalCaseId
 LEFT JOIN Provider PP ON PP.ProviderID = Wip.ProviderId 
WHERE cc.ProjectId = @ProjectId              
AND cc.DateOfService = @CurrentDos          
AND wi.IsBlocked = 1 AND ISNULL(bc.BlockType, '') = 'Internal'          
END          
ELSE IF @ColumnName = 'ExternalBlocked'            
BEGIN          
SELECT distinct CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId    
 ,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name,ISNULL(Pro.Name,PP.Name) AS Provider    
FROM ClinicalCase cc              
JOIN WorkItem wi              
ON wi.ClinicalCaseId = cc.ClinicalCaseId              
JOIN Project p              
ON p.ProjectId = wi.ProjectId              
LEFT JOIN BlockHistory bh              
ON bh.ClinicalCaseId = cc.ClinicalCaseId              
LEFT JOIN BlockCategory bc              
ON bc.BlockCategoryId = bh.BlockCategoryId    
LEFT JOIN List L ON L.ListId = CC.ListId    
 LEFT JOIN Provider Pro ON Pro.ProviderId = CC.ProviderId    
 LEFT JOIN WorkItemProvider Wip ON Wip.ClinicalCaseId = CC.ClinicalCaseId
 LEFT JOIN Provider PP ON PP.ProviderID = Wip.ProviderId
WHERE cc.ProjectId = @ProjectId              
AND cc.DateOfService = @CurrentDos             
AND wi.IsBlocked = 1 AND ISNULL(bc.BlockType, '') = 'External'          
END          
ELSE IF @ColumnName = 'NotCoded'            
BEGIN          
SELECT distinct CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId    
 ,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name,ISNULL(Pro.Name,PP.Name) AS Provider    
FROM ClinicalCase cc              
JOIN WorkItem wi              
ON wi.ClinicalCaseId = cc.ClinicalCaseId              
JOIN Project p              
ON p.ProjectId = wi.ProjectId              
LEFT JOIN BlockHistory bh              
ON bh.ClinicalCaseId = cc.ClinicalCaseId              
LEFT JOIN BlockCategory bc              
ON bc.BlockCategoryId = bh.BlockCategoryId      
LEFT JOIN List L ON L.ListId = CC.ListId    
 LEFT JOIN Provider Pro ON Pro.ProviderId = CC.ProviderId  
 LEFT JOIN WorkItemProvider Wip ON Wip.ClinicalCaseId = CC.ClinicalCaseId
 LEFT JOIN Provider PP ON PP.ProviderID = Wip.ProviderId  
WHERE cc.ProjectId = @ProjectId              
AND cc.DateOfService = @CurrentDos             
AND wi.StatusId IN (1, 2) AND wi.IsBlocked = 0          
END          
ELSE IF @ColumnName = 'NotAudited'            
BEGIN          
SELECT distinct CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId    
 ,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name,ISNULL(Pro.Name,PP.Name) AS Provider    
FROM ClinicalCase cc              
JOIN WorkItem wi              
ON wi.ClinicalCaseId = cc.ClinicalCaseId              
JOIN Project p              
ON p.ProjectId = wi.ProjectId              
LEFT JOIN BlockHistory bh              
ON bh.ClinicalCaseId = cc.ClinicalCaseId              
LEFT JOIN BlockCategory bc              
ON bc.BlockCategoryId = bh.BlockCategoryId     
LEFT JOIN List L ON L.ListId = CC.ListId    
 LEFT JOIN Provider Pro ON Pro.ProviderId = CC.ProviderId   
 LEFT JOIN WorkItemProvider Wip ON Wip.ClinicalCaseId = CC.ClinicalCaseId
 LEFT JOIN Provider PP ON PP.ProviderID = Wip.ProviderId 
WHERE cc.ProjectId = @ProjectId              
AND cc.DateOfService = @CurrentDos             
AND wi.StatusId > 3 AND wi.StatusId < 15 AND wi.IsBlocked = 0          
END          
ELSE IF @ColumnName = 'NotPosted'            
BEGIN          
SELECT distinct CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId    
 ,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name,ISNULL(Pro.Name,PP.Name) AS Provider    
FROM ClinicalCase cc              
JOIN WorkItem wi              
ON wi.ClinicalCaseId = cc.ClinicalCaseId              
JOIN Project p              
ON p.ProjectId = wi.ProjectId              
LEFT JOIN BlockHistory bh              
ON bh.ClinicalCaseId = cc.ClinicalCaseId              
LEFT JOIN BlockCategory bc              
ON bc.BlockCategoryId = bh.BlockCategoryId    
LEFT JOIN List L ON L.ListId = CC.ListId    
 LEFT JOIN Provider Pro ON Pro.ProviderId = CC.ProviderId  
 LEFT JOIN WorkItemProvider Wip ON Wip.ClinicalCaseId = CC.ClinicalCaseId
 LEFT JOIN Provider PP ON PP.ProviderID = Wip.ProviderId  
WHERE cc.ProjectId = @ProjectId              
AND cc.DateOfService = @CurrentDos             
AND wi.StatusId = 15 AND wi.IsBlocked = 0          
END      
ELSE IF @ColumnName = 'TotalBacklog'            
BEGIN          
SELECT distinct CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId    
 ,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name,ISNULL(Pro.Name,PP.Name) AS Provider    
FROM ClinicalCase cc              
JOIN WorkItem wi              
ON wi.ClinicalCaseId = cc.ClinicalCaseId              
JOIN Project p              
ON p.ProjectId = wi.ProjectId              
LEFT JOIN BlockHistory bh              
ON bh.ClinicalCaseId = cc.ClinicalCaseId              
LEFT JOIN BlockCategory bc              
ON bc.BlockCategoryId = bh.BlockCategoryId      
LEFT JOIN List L ON L.ListId = CC.ListId    
 LEFT JOIN Provider Pro ON Pro.ProviderId = CC.ProviderId   
 LEFT JOIN WorkItemProvider Wip ON Wip.ClinicalCaseId = CC.ClinicalCaseId
 LEFT JOIN Provider PP ON PP.ProviderID = Wip.ProviderId 
WHERE cc.ProjectId = @ProjectId              
AND cc.DateOfService = @CurrentDos             
--AND wi.StatusId <> 15
AND wi.StatusId < 16
END         
END