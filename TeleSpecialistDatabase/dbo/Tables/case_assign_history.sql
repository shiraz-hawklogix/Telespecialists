CREATE TABLE [dbo].[case_assign_history] (
    [cah_key]                   INT            IDENTITY (1, 1) NOT NULL,
    [cah_action]                VARCHAR (50)   NOT NULL,
    [cah_cas_key]               INT            NOT NULL,
    [cah_phy_key]               NVARCHAR (128) NULL,
    [cah_request_sent_time]     DATETIME       NULL,
    [cah_action_time]           DATETIME       NULL,
    [cah_sort_order]            INT            NULL,
    [cah_is_active]             BIT            CONSTRAINT [DF_case_assign_history_cah_is_deleted] DEFAULT ((1)) NOT NULL,
    [cah_created_date]          DATETIME       NOT NULL,
    [cah_created_by]            NVARCHAR (128) NOT NULL,
    [cah_modified_date]         DATETIME       NULL,
    [cah_modified_by]           NVARCHAR (128) NULL,
    [cah_is_manuall_assign]     BIT            CONSTRAINT [DF__case_assi__cah_i__3AAC9BB0] DEFAULT ((0)) NOT NULL,
    [cah_request_sent_time_utc] DATETIME       NULL,
    [cah_action_time_utc]       DATETIME       NULL,
    [cah_created_date_utc]      DATETIME       NULL,
    CONSTRAINT [PK_case_assign_history] PRIMARY KEY CLUSTERED ([cah_key] ASC),
    CONSTRAINT [FK_case_assign_history_AspNetUsers] FOREIGN KEY ([cah_phy_key]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);

