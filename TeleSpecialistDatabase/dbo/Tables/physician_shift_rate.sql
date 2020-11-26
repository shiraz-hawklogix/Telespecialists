CREATE TABLE [dbo].[physician_shift_rate] (
    [psr_key]              INT             IDENTITY (1, 1) NOT NULL,
    [psr_phy_key]          NVARCHAR (128)  NULL,
    [psr_shift]            INT             NULL,
    [psr_shift_name]       NVARCHAR (50)   NULL,
    [psr_rate]             DECIMAL (18, 2) NULL,
    [psr_created_by]       NVARCHAR (128)  NULL,
    [psr_created_date]     DATETIME        NULL,
    [psr_created_by_name]  NVARCHAR (100)  NULL,
    [psr_modified_by]      NVARCHAR (128)  NULL,
    [psr_modified_date]    DATETIME        NULL,
    [psr_modified_by_name] NVARCHAR (100)  NULL,
    [psr_start_date]       DATETIME        NULL,
    [psr_end_date]         DATETIME        NULL,
    CONSTRAINT [PK_physician_shift_rate] PRIMARY KEY CLUSTERED ([psr_key] ASC)
);

