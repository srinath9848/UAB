CREATE PROCEDURE [dbo].[UspPendingChartDetailReport]--  1,'PerWeek','5/17/2021 4:16:40 PM',20,'May',2021    
@ProjectId INT = NULL,          
@RangeType VARCHAR(100),      
@Date DateTime2 = NULL,      
@Week INT = NULL,      
@Month VARCHAR(10) = NULL,      
@Year INT = NULL   ,  
@TimeZoneOffset DECIMAL(8,2) ,    
@StartDate DateTIME,        
@EndDate DateTIME    
AS          
BEGIN          
        
IF @ProjectId = 0         
 SET @ProjectId = NULL       
      
 IF @RangeType = 'PerDay'          
BEGIN          
 SELECT DISTINCT CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId,
 CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name, ISNULL(P.Name,PP.Name) AS Provider        
 FROM WorkItem W           
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId        
 INNER JOIN BlockHistory BH on CC.ClinicalcaseId = bh.ClinicalcaseId      
 INNER JOIN BlockCategory BC on BH.BlockCategoryID = BC.BlockCategoryId    
 LEFT JOIN List L ON L.ListId = CC.ListId    
 LEFT JOIN Provider P ON P.ProviderId = CC.ProviderId    
  LEFT JOIN WorkItemProvider Wip ON Wip.ClinicalCaseId = CC.ClinicalCaseId
 LEFT JOIN Provider PP ON PP.ProviderID = Wip.ProviderId
 WHERE W.ProjectId = @ProjectId    
 AND CONVERT(VARCHAR, DATEADD(Second, @TimeZoneOffset * 60 * 60, BH.CreateDate), 1)=@Date      
END      
ELSE IF @RangeType = 'PerWeek'          
BEGIN          
 SELECT DISTINCT CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name, ISNULL(P.Name,PP.Name) AS Provider        
 FROM WorkItem W           
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId        
 INNER JOIN BlockHistory BH on CC.ClinicalcaseId = bh.ClinicalcaseId      
 INNER JOIN BlockCategory BC on BH.BlockCategoryID = BC.BlockCategoryId    
 LEFT JOIN List L ON L.ListId = CC.ListId    
 LEFT JOIN Provider P ON P.ProviderId = CC.ProviderId    
  LEFT JOIN WorkItemProvider Wip ON Wip.ClinicalCaseId = CC.ClinicalCaseId
 LEFT JOIN Provider PP ON PP.ProviderID = Wip.ProviderId
 WHERE W.ProjectId = @ProjectId  
 AND DATEADD(Second, @TimeZoneOffset * 60 * 60, BH.CreateDate) >= @StartDate    
AND  DATEADD(Second, @TimeZoneOffset * 60 * 60, BH.CreateDate) <= DATEADD(Hour,23,DATEADD(MINUTE,59,DATEADD(SECOND,59,@Enddate)))   
 --AND DATEPART(WEEK, BH.CreateDate)-DATEPART(WEEK, DATEADD(MM, DATEDIFF(MM,0,BH.CreateDate), 0))+ 1 = @Week      
   AND DATEPART(ISO_WEEK, DATEADD(Second, @TimeZoneOffset * 60 * 60, BH.CreateDate)) = @Week    
 AND DATENAME(month, DATEADD(Second, @TimeZoneOffset * 60 * 60, BH.CreateDate))=@Month      
 AND DATEPART(yyyy, DATEADD(Second, @TimeZoneOffset * 60 * 60, BH.CreateDate))=@Year         
END      
ELSE IF @RangeType = 'PerMonth'          
BEGIN         
SELECT DISTINCT CC.DateOfService AS DateOfService,L.Name AS ListName,CC.ClinicalCaseId AS ClinicalCaseId,CC.PatientMRN AS PatientMRN, CC.PatientFirstName+' '+CC.PatientLastName AS Name, ISNULL(P.Name,PP.Name) AS Provider        
 FROM WorkItem W           
 INNER JOIN Clinicalcase CC ON W.ClinicalcaseId = CC.ClinicalcaseId        
 INNER JOIN BlockHistory BH on CC.ClinicalcaseId = bh.ClinicalcaseId      
 INNER JOIN BlockCategory BC on BH.BlockCategoryID = BC.BlockCategoryId    
 LEFT JOIN List L ON L.ListId = CC.ListId    
 LEFT JOIN Provider P ON P.ProviderId = CC.ProviderId    
  LEFT JOIN WorkItemProvider Wip ON Wip.ClinicalCaseId = CC.ClinicalCaseId
 LEFT JOIN Provider PP ON PP.ProviderID = Wip.ProviderId
 WHERE W.ProjectId = @ProjectId    
 AND DATEADD(Second, @TimeZoneOffset * 60 * 60, BH.CreateDate) >= @StartDate    
AND  DATEADD(Second, @TimeZoneOffset * 60 * 60, BH.CreateDate) <= DATEADD(Hour,23,DATEADD(MINUTE,59,DATEADD(SECOND,59,@Enddate)))   
 AND DATENAME(month, DATEADD(Second, @TimeZoneOffset * 60 * 60, BH.CreateDate))=@Month      
 AND DATEPART(yyyy, DATEADD(Second, @TimeZoneOffset * 60 * 60, BH.CreateDate))=@Year      
END      
END