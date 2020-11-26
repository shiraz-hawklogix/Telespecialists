CREATE TABLE [dbo].[operationsoutliersTemp] (
    [CaseKey]                                    INT            IDENTITY (1, 1) NOT NULL,
    [CaseNumber]                                 BIGINT         NULL,
    [CaseType]                                   NVARCHAR (50)  NULL,
    [StartTime]                                  NVARCHAR (50)  NULL,
    [FacilityName]                               NVARCHAR (250) NULL,
    [Physician]                                  NVARCHAR (150) NULL,
    [TS_Response_Time_or_CallBack_Response_Time] NVARCHAR (50)  NULL,
    CONSTRAINT [PK_operationsoutliersTemp] PRIMARY KEY CLUSTERED ([CaseKey] ASC)
);

