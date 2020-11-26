CREATE TABLE [dbo].[Forcast_Data] (
    [Id]               INT            IDENTITY (1, 1) NOT NULL,
    [Fac_Id]           NVARCHAR (100) NULL,
    [Fac_Name]         NVARCHAR (100) NULL,
    [Month_Name]       DATETIME       NULL,
    [Month_Prediction] NVARCHAR (100) NULL,
    [Created_Date]     DATETIME       NULL,
    CONSTRAINT [PK_Forcast_Data] PRIMARY KEY CLUSTERED ([Id] ASC)
);

