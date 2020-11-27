CREATE TABLE [dbo].[Hospital_Protocols] (
    [ID]                  INT            IDENTITY (1, 1) NOT NULL,
    [ParameterName]       NVARCHAR (100) NULL,
    [ParameterName_Info]  NVARCHAR (MAX) NULL,
    [ParameterName_Image] NVARCHAR (MAX) NULL,
    [Facility_Name]       NVARCHAR (200) NULL,
    [Facility_Id]         NVARCHAR (200) NULL,
    [Parameter_Add_Date]  DATETIME       NULL,
    [SortNum]             INT            NULL,
    CONSTRAINT [PK_Hospital_Protocols] PRIMARY KEY CLUSTERED ([ID] ASC)
);

