CREATE TABLE [dbo].[physician_status_snooze_option] (
    [pso_key]              INT            IDENTITY (1, 1) NOT NULL,
    [pso_message]          VARCHAR (1000) NOT NULL,
    [pso_snooze_time]      TIME (7)       NOT NULL,
    [pso_is_active]        BIT            NOT NULL,
    [pso_created_date]     DATETIME       NOT NULL,
    [pso_created_by]       NVARCHAR (128) NOT NULL,
    [pso_created_by_name]  VARCHAR (300)  NOT NULL,
    [pso_modified_date]    DATETIME       NULL,
    [pso_modified_by]      NVARCHAR (128) NULL,
    [pso_modified_by_name] VARCHAR (300)  NULL,
    [pso_phs_key]          INT            NOT NULL,
    CONSTRAINT [PK_physician_status_snooze_option] PRIMARY KEY CLUSTERED ([pso_key] ASC),
    CONSTRAINT [FK_physician_status_snooze_option_physician_status] FOREIGN KEY ([pso_phs_key]) REFERENCES [dbo].[physician_status] ([phs_key]) ON DELETE CASCADE ON UPDATE CASCADE
);

