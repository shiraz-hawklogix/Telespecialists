CREATE TABLE [dbo].[physician_status_log] (
    [psl_key]           INT              IDENTITY (1, 1) NOT NULL,
    [psl_user_key]      NVARCHAR (128)   NULL,
    [psl_phs_key]       INT              NOT NULL,
    [psl_status_name]   VARCHAR (50)     NULL,
    [psl_created_date]  DATETIME         NULL,
    [psl_created_by]    NVARCHAR (128)   NULL,
    [psl_start_date]    DATETIME         NULL,
    [psl_end_date]      DATETIME         NULL,
    [psl_modified_by]   NVARCHAR (128)   NULL,
    [psl_modified_date] DATETIME         NULL,
    [psl_uss_key]       BIGINT           NULL,
    [psl_cas_key]       INT              NULL,
    [psl_comments]      VARCHAR (300)    NULL,
    [psl_fac_key]       UNIQUEIDENTIFIER NULL,
    [psl_case_details]  VARCHAR (MAX)    NULL,
    [psl_facility_name] VARCHAR (MAX)    NULL,
    CONSTRAINT [PK_physician_status_log] PRIMARY KEY CLUSTERED ([psl_key] ASC),
    CONSTRAINT [FK_physician_status_log_AspNetUsers] FOREIGN KEY ([psl_user_key]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_physician_status_log_physician_status_log] FOREIGN KEY ([psl_key]) REFERENCES [dbo].[physician_status_log] ([psl_key])
);


GO
ALTER TABLE [dbo].[physician_status_log] NOCHECK CONSTRAINT [FK_physician_status_log_AspNetUsers];


GO
ALTER TABLE [dbo].[physician_status_log] NOCHECK CONSTRAINT [FK_physician_status_log_physician_status_log];

