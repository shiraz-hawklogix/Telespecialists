CREATE TABLE [dbo].[physician_percentage_rate] (
    [ppr_key]              INT             IDENTITY (1, 1) NOT NULL,
    [ppr_phy_key]          NVARCHAR (128)  NULL,
    [ppr_shift_id]         INT             NULL,
    [ppr_shift_name]       NVARCHAR (50)   NULL,
    [ppr_percentage]       DECIMAL (18, 2) NULL,
    [ppr_created_by]       NVARCHAR (128)  NULL,
    [ppr_created_by_name]  NVARCHAR (100)  NULL,
    [ppr_created_date]     DATETIME        NULL,
    [ppr_modified_by]      NVARCHAR (128)  NULL,
    [ppr_modified_by_name] NVARCHAR (100)  NULL,
    [ppr_modified_date]    DATETIME        NULL,
    [ppr_start_date]       DATETIME        NULL,
    [ppr_end_date]         DATETIME        NULL,
    CONSTRAINT [PK_physician_percentage_rate] PRIMARY KEY CLUSTERED ([ppr_key] ASC)
);

