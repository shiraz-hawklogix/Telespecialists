
CREATE TABLE [dbo].[physician_holiday_rate] (
    [phr_key]              INT             IDENTITY (1, 1) NOT NULL,
    [phr_phy_key]          NVARCHAR (128)  NULL,
    [phr_date]             DATETIME        NULL,
    [phr_rate]             DECIMAL (18, 2) NULL,
    [phr_created_by]       NVARCHAR (128)  NULL,
    [phr_created_date]     DATETIME        NULL,
    [phr_created_by_name]  NVARCHAR (100)  NULL,
    [phr_modified_by]      NVARCHAR (128)  NULL,
    [phr_modified_date]    DATETIME        NULL,
    [phr_modified_by_name] NVARCHAR (100)  NULL,
    [phr_uss_key]          BIGINT          NULL,
    [phr_shift_key]        INT             NULL,
    CONSTRAINT [PK_physician_holiday_rate] PRIMARY KEY CLUSTERED ([phr_key] ASC),
    CONSTRAINT [FK_physician_holiday_rate_user_schedule] FOREIGN KEY ([phr_uss_key]) REFERENCES [dbo].[user_schedule] ([uss_key])
);


GO
