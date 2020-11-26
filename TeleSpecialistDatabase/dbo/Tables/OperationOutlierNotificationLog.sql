CREATE TABLE [dbo].[OperationOutlierNotificationLog] (
    [Id]                          INT           IDENTITY (1, 1) NOT NULL,
    [cas_case_number]             VARCHAR (100) NULL,
    [cas_case_type]               VARCHAR (50)  NULL,
    [cas_case_color]              VARCHAR (20)  NULL,
    [cas_created_date]            DATETIME      NULL,
    [cas_modified_date]           DATETIME      NULL,
    [cas_case_fac_name]           VARCHAR (MAX) NULL,
    [cas_case_assign_phy_initial] VARCHAR (200) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

