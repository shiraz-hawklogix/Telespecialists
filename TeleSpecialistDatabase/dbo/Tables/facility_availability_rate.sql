CREATE TABLE [dbo].[facility_availability_rate] (
    [far_key]              INT              IDENTITY (1, 1) NOT NULL,
    [far_fac_key]          UNIQUEIDENTIFIER NULL,
    [far_shifts]           INT              NULL,
    [far_rate]             DECIMAL (18, 2)  NULL,
    [far_start_date]       DATETIME         NULL,
    [far_end_date]         DATETIME         NULL,
    [far_recurrence]       BIT              NOT NULL,
    [far_created_by]       NVARCHAR (128)   NULL,
    [far_created_date]     DATETIME         NULL,
    [far_created_by_name]  NVARCHAR (100)   NULL,
    [far_modified_by]      NVARCHAR (128)   NULL,
    [far_modified_date]    DATETIME         NULL,
    [far_modified_by_name] NVARCHAR (100)   NULL,
    CONSTRAINT [PK_facility_availability_rate] PRIMARY KEY CLUSTERED ([far_key] ASC)
);

