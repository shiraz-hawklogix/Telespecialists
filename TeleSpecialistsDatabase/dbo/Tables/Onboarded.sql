CREATE TABLE [dbo].[Onboarded] (
    [Onboarded_ID]           INT            IDENTITY (1, 1) NOT NULL,
    [ParameterName]          NVARCHAR (MAX) NULL,
    [ParameterName_Info]     NVARCHAR (MAX) NULL,
    [ParameterName_Image]    NVARCHAR (MAX) NULL,
    [ParameterName_Image_Id] NVARCHAR (50)  NULL,
    [Facility_Name]          NVARCHAR (200) NULL,
    [Facility_Id]            NVARCHAR (200) NULL,
    [Parameter_Add_Date]     DATETIME       NULL,
    [ser_no]                 INT            NULL,
    [SortNum]                INT            NULL,
    CONSTRAINT [PK_Onboarded] PRIMARY KEY CLUSTERED ([Onboarded_ID] ASC)
);

