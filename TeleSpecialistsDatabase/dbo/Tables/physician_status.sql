CREATE TABLE [dbo].[physician_status] (
    [phs_key]                  INT            IDENTITY (1, 1) NOT NULL,
    [phs_name]                 VARCHAR (50)   NOT NULL,
    [phs_description]          VARCHAR (500)  NULL,
    [phs_color_code]           VARCHAR (10)   NOT NULL,
    [phs_threshhold_time]      TIME (7)       NULL,
    [phs_sort_order]           INT            NOT NULL,
    [phs_is_active]            BIT            CONSTRAINT [DF_physician_status_phs_is_active] DEFAULT ((1)) NOT NULL,
    [phs_is_default]           BIT            CONSTRAINT [DF_physician_status_phs_is_default] DEFAULT ((0)) NOT NULL,
    [phs_created_by]           NVARCHAR (128) NULL,
    [phs_created_date]         DATETIME       NULL,
    [phs_modified_by]          NVARCHAR (128) NULL,
    [phs_modified_date]        DATETIME       NULL,
    [phs_move_status_key]      INT            NULL,
    [phs_move_threshhold_time] TIME (7)       NULL,
    [phs_assignment_priority]  INT            NULL,
    [phs_enable_snooze]        BIT            CONSTRAINT [DF_physician_status_phs_enable_snooze] DEFAULT ((0)) NOT NULL,
    [phs_snooze_time]          TIME (7)       NULL,
    [phs_max_snooze_count]     INT            NULL,
    CONSTRAINT [PK_physician_status] PRIMARY KEY CLUSTERED ([phs_key] ASC),
    CONSTRAINT [FK_physician_status_physician_status] FOREIGN KEY ([phs_move_status_key]) REFERENCES [dbo].[physician_status] ([phs_key])
);


GO
ALTER TABLE [dbo].[physician_status] NOCHECK CONSTRAINT [FK_physician_status_physician_status];

