CREATE TABLE [dbo].[physician_status_snooze] (
    [pss_key]              INT            IDENTITY (1, 1) NOT NULL,
    [pss_phs_key]          INT            NOT NULL,
    [pss_user_key]         NVARCHAR (128) NOT NULL,
    [pss_snooze_time]      TIME (7)       NOT NULL,
    [pss_is_active]        BIT            NOT NULL,
    [pss_processed_date]   DATETIME       NULL,
    [pss_created_date]     DATETIME       NOT NULL,
    [pss_created_by]       NVARCHAR (128) NOT NULL,
    [pss_createe_by_name]  VARCHAR (300)  NOT NULL,
    [pss_is_latest_snooze] BIT            CONSTRAINT [DF_physician_status_snooze_pss_is_latest_snooze] DEFAULT ((1)) NOT NULL,
    [pss_modified_by]      NVARCHAR (128) NULL,
    [pss_modified_by_name] VARCHAR (300)  NULL,
    [pss_modified_date]    DATETIME       NULL,
    CONSTRAINT [PK_physician_status_snooze] PRIMARY KEY CLUSTERED ([pss_key] ASC),
    CONSTRAINT [FK_physician_status_snooze_AspNetUsers] FOREIGN KEY ([pss_user_key]) REFERENCES [dbo].[AspNetUsers] ([Id]),
    CONSTRAINT [FK_physician_status_snooze_physician_status] FOREIGN KEY ([pss_phs_key]) REFERENCES [dbo].[physician_status] ([phs_key])
);


GO
ALTER TABLE [dbo].[physician_status_snooze] NOCHECK CONSTRAINT [FK_physician_status_snooze_AspNetUsers];


GO
ALTER TABLE [dbo].[physician_status_snooze] NOCHECK CONSTRAINT [FK_physician_status_snooze_physician_status];

