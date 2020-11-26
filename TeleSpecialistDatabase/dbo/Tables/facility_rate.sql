CREATE TABLE [dbo].[facility_rate] (
    [fct_key]              INT              IDENTITY (1, 1) NOT NULL,
    [fct_facility_key]     UNIQUEIDENTIFIER NOT NULL,
    [fct_billing_key]      INT              NULL,
    [fct_starting]         INT              NULL,
    [fct_ending]           INT              NULL,
    [fct_range]            NVARCHAR (50)    NULL,
    [fct_rate]             DECIMAL (18, 2)  NULL,
    [fct_start_date]       DATETIME         NULL,
    [fct_end_date]         DATETIME         NULL,
    [fct_created_by]       NVARCHAR (128)   NULL,
    [fct_created_date]     DATETIME         NULL,
    [fct_created_by_name]  NVARCHAR (100)   NULL,
    [fct_modified_by]      NVARCHAR (128)   NULL,
    [fct_modified_date]    DATETIME         NULL,
    [fct_modified_by_name] NVARCHAR (100)   NULL,
    CONSTRAINT [PK_facility_rate] PRIMARY KEY CLUSTERED ([fct_key] ASC)
);

