CREATE PROCEDURE [dbo].[USPLoadData]           
@utInputData AS utInputData readonly,          
@utDuplicateInputData AS utInputData readonly          
 AS            
BEGIN            
          
                              
 BEGIN TRY                                
  BEGIN TRANSACTION                                
                                
  DECLARE @RC INT                                
  DECLARE @message VARCHAR(500)                                
                                
  EXEC @RC = sp_getapplock @Resource = 'USPLoadData'                                
   ,@LockMode = 'Exclusive'                                
   ,@LockOwner = 'Transaction'                                
   ,@LockTimeout = 15000 --15 seconds                                                    
                                
  IF @RC < 0                                
  BEGIN                                
   SELECT @message = CONVERT(VARCHAR(30), GETUTCDATE(), 121) + ': Sorry, could not obtain a lock within the timeout period,          
    return code was ' + CONVERT(VARCHAR(30), @RC) + '.'                                
                                
   RAISERROR (                                
     @message                                
     ,0                                
     ,1                                
     )                                
   WITH NOWAIT;                                
                                
   ROLLBACK TRANSACTION                                
                                
   RETURN @RC                                
  END                                
  ELSE                                
  BEGIN          
      
  INSERT INTO [Provider]([Name])        
  SELECT DISTINCT [Provider] FROM @utInputData AS i LEFT JOIN [Provider] p ON p.[Name] = i.[Provider]          
  WHERE p.[name] is null     
          
 SELECT i.[ID],            
  i.[ProjectID],            
  i.[FileName],            
  i.[ListName],            
  i.[ListID],            
  i.[PatientMRN],            
  i.[PatientLastName],            
  i.[PatientFirstName],            
  i.[DateOfService],            
  i.[EncounterNumber],            
  i.[Provider]            
  ,CC.ClinicalCaseId            
  ,p.ProviderId            
 INTO #Result            
 FROM @utInputData AS i            
 LEFT JOIN ClinicalCase CC ON i.PatientMRN = CC.PatientMRN            
 AND i.DateOfService = CC.DateOfService            
 AND CC.ProjectId = i.ProjectId            
 LEFT JOIN [Provider] p ON p.[Name] = i.[Provider]            
            
 SELECT *            
 INTO #NonDuplicates            
 FROM #Result            
 WHERE ClinicalCaseId IS NULL            
            
 SELECT *            
 INTO #Duplicates            
 FROM #Result            
 WHERE ClinicalCaseId IS NOT NULL            
            
 INSERT INTO ClinicalCaseDuplicate (            
  ProjectId            
  ,PatientMRN            
  ,PatientLastName            
  ,PatientFirstName            
  ,DateOfService            
  ,EncounterNumber            
  ,CreatedDate            
  ,ProviderId            
  ,[FileName]            
  ,Remarks          
  )            
 SELECT ProjectID            
  ,PatientMRN            
  ,PatientLastName            
  ,PatientFirstName            
  ,DateOfService            
  ,EncounterNumber            
  ,GETUTCDATE()            
  ,ProviderId            
  ,[FileName]            
  ,'Duplicate with DB'          
 FROM #Duplicates            
          
 --Insert Within File Duplicate          
 INSERT INTO ClinicalCaseDuplicate (            
  ProjectId            
  ,PatientMRN            
  ,PatientLastName            
  ,PatientFirstName            
  ,DateOfService            
  ,EncounterNumber            
  ,CreatedDate            
  ,ProviderId            
  ,[FileName]            
  ,Remarks          
  )            
 SELECT ProjectID            
  ,PatientMRN            
  ,PatientLastName            
  ,PatientFirstName        
  ,DateOfService            
  ,EncounterNumber            
  ,GETUTCDATE()            
  ,NULL            
  ,[FileName]            
  ,'Duplicate Within File'          
 FROM @utDuplicateInputData            
            
 DECLARE @ClinicalCaseId INT            
        
 DECLARE @MinId INT = (SELECT TOP 1 Id FROM #NonDuplicates ORDER BY Id)            
        
 DECLARE @MaxId INT = (            
   SELECT MAX(ID)            
   FROM #NonDuplicates            
   )            
            
 WHILE (@MinId <= @MaxId)            
 BEGIN            
              
  IF NOT EXISTS ( SELECT ListId            
      FROM List            
      WHERE [Name] = (            
        SELECT ISNULL(ListName, '')            
        FROM #NonDuplicates            
        WHERE Id = @MinId            
      )            
  )            
  BEGIN            
   INSERT INTO List (            
    ListId            
    ,[Name]            
    )            
   SELECT ListID            
    ,ListName            
   FROM #NonDuplicates            
   WHERE Id = @MinId            
  END            
            
  INSERT INTO ClinicalCase (            
   ProjectId            
   ,[FileName]           
   ,ListId            
   ,PatientMRN            
   ,PatientLastName            
   ,PatientFirstName            
   ,DateOfService            
   ,EncounterNumber            
   ,CreatedDate            
   ,ProviderId            
   )            
  SELECT ProjectID            
   ,[FileName]            
   ,l.ListId            
   ,PatientMRN            
   ,PatientLastName            
   ,PatientFirstName            
   ,DateOfService            
   ,EncounterNumber            
   ,GETUTCDATE()            
   ,ProviderId            
  FROM #NonDuplicates n            
  LEFT JOIN List l ON l.[Name] = n.ListName            
  WHERE Id = @MinId            
            
  SELECT @ClinicalCaseId = SCOPE_IDENTITY()            
            
  INSERT INTO WorkItem (            
   ClinicalCaseId            
   ,StatusId            
   ,ProjectId            
   )            
  SELECT @ClinicalCaseId            
   ,1            
   ,ProjectID            
  FROM #NonDuplicates            
  WHERE Id = @MinId            
            
  INSERT INTO Version (Clinicalcaseid,versiondate,userid,statusid)           
  SELECT @ClinicalCaseId            
   ,GETUTCDATE()            
   ,0            
   ,1            
            
  SET @MinId = (SELECT TOP 1 Id FROM #NonDuplicates WHERE Id>@MinId ORDER BY Id)            
 END            
          
 END          
          
  COMMIT TRANSACTION                                
 END TRY                                
                                
 BEGIN CATCH                                
  ROLLBACK TRANSACTION                        
             
   DECLARE @ErrorMessage VARCHAR(MAX)          
   DECLARE @ErrorSeverity VARCHAR(MAX)          
   DECLARE @ErrorState VARCHAR(MAX)          
          
   SET @ErrorMessage  = ERROR_MESSAGE()          
    SET @ErrorSeverity = ERROR_SEVERITY()          
    SET @ErrorState    = ERROR_STATE()          
          
    RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)          
                   
 END CATCH              
          
END 