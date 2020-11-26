CREATE TABLE [dbo].[physician_rate] (
    [rat_key]              INT             IDENTITY (1, 1) NOT NULL,
    [rat_phy_key]          NVARCHAR (128)  NULL,
    [rat_starting]         INT             NULL,
    [rat_ending]           INT             NULL,
    [rat_range]            NVARCHAR (50)   NULL,
    [rat_price]            DECIMAL (18, 2) NULL,
    [rat_shift_hour]       INT             NULL,
    [rat_shift_id]         INT             NULL,
    [rat_shift_name]       NVARCHAR (150)  NULL,
    [rat_cas_id]           INT             NULL,
    [rat_created_by]       NVARCHAR (128)  NULL,
    [rat_created_date]     DATETIME        NULL,
    [rat_created_by_name]  NVARCHAR (100)  NULL,
    [rat_modified_by]      NVARCHAR (128)  NULL,
    [rat_modified_date]    DATETIME        NULL,
    [rat_modified_by_name] NVARCHAR (100)  NULL,
    [rat_start_date]       DATETIME        NULL,
    [rat_end_date]         DATETIME        NULL,
    CONSTRAINT [PK_Rate] PRIMARY KEY CLUSTERED ([rat_key] ASC),
    CONSTRAINT [FK_Rate_Rate] FOREIGN KEY ([rat_phy_key]) REFERENCES [dbo].[AspNetUsers] ([Id])
);


GO
ALTER TABLE [dbo].[physician_rate] NOCHECK CONSTRAINT [FK_Rate_Rate];

