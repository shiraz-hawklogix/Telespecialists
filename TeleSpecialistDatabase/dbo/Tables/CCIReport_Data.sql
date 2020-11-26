CREATE TABLE [dbo].[CCIReport_Data] (
    [CCI_ID]         INT            IDENTITY (1, 1) NOT NULL,
    [Physician_Id]   NVARCHAR (100) NULL,
    [Physician_CCI]  NVARCHAR (50)  NULL,
    [Month]          DATETIME       NULL,
    [Date]           DATETIME       NULL,
    [Physician_Name] NVARCHAR (100) NULL,
    CONSTRAINT [PK_CCIReport_Data] PRIMARY KEY CLUSTERED ([CCI_ID] ASC)
);

