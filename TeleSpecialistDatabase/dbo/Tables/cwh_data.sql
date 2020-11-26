CREATE TABLE [dbo].[cwh_data] (
    [cwh_key]            INT              IDENTITY (1, 1) NOT NULL,
    [cwh_fac_id]         UNIQUEIDENTIFIER NOT NULL,
    [cwh_fac_name]       NVARCHAR (MAX)   NULL,
    [cwh_totalcwh]       FLOAT (53)       NULL,
    [cwh_month_wise_cwh] FLOAT (53)       NULL,
    [cwh_date]           DATETIME         NULL,
    [cwh_modified_by]    NVARCHAR (50)    NULL,
    [cwh_modified_date]  DATETIME         NULL,
    CONSTRAINT [PK_cwh_data] PRIMARY KEY CLUSTERED ([cwh_key] ASC)
);

