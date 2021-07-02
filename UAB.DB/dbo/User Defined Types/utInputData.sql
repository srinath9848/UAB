CREATE TYPE [dbo].[utInputData] AS TABLE (
    [ID]               INT           NULL,
    [ProjectID]        INT           NULL,
    [FileName]         VARCHAR (50)  NULL,
    [ListName]         VARCHAR (100) NULL,
    [ListID]           VARCHAR (100) NULL,
    [PatientMRN]       VARCHAR (50)  NULL,
    [PatientLastName]  VARCHAR (255) NULL,
    [PatientFirstName] VARCHAR (25)  NULL,
    [DateOfService]    VARCHAR (25)  NULL,
    [EncounterNumber]  VARCHAR (25)  NULL,
    [Provider]         VARCHAR (100) NULL);

