CREATE TABLE [dbo].[user_schedule] (
    [uss_key]                BIGINT          IDENTITY (1, 1) NOT NULL,
    [uss_user_id]            NVARCHAR (128)  NOT NULL,
    [uss_date]               DATETIME        NOT NULL,
    [uss_time_from]          BIGINT          NOT NULL,
    [uss_time_to]            BIGINT          NOT NULL,
    [uss_description]        NVARCHAR (MAX)  NULL,
    [uss_is_active]          BIT             CONSTRAINT [DF_resource_scheduler_rsr_is_active] DEFAULT ((1)) NULL,
    [uss_created_by]         NVARCHAR (128)  NOT NULL,
    [uss_created_by_name]    VARCHAR (300)   NULL,
    [uss_created_date]       DATETIME        CONSTRAINT [DF_user_schedule_uss_created_date] DEFAULT (getdate()) NOT NULL,
    [uss_modified_by]        NVARCHAR (128)  NULL,
    [uss_modified_by_name]   VARCHAR (300)   NULL,
    [uss_modified_date]      DATETIME        NULL,
    [uss_time_from_calc]     DATETIME2 (7)   NULL,
    [uss_time_to_calc]       DATETIME2 (7)   NULL,
    [uss_custome_rate]       DECIMAL (18, 2) NULL,
    [uss_shift_key]          INT             NULL,
    [uss_is_publish]         BIT             CONSTRAINT [Const_uss_is_publish] DEFAULT ('false') NULL,
    [uss_date_num]           BIGINT          NULL,
    [uss_time_from_calc_num] BIGINT          NULL,
    [uss_time_to_calc_num]   BIGINT          NULL,
    CONSTRAINT [PK_resource_scheduler] PRIMARY KEY CLUSTERED ([uss_key] ASC),
    CONSTRAINT [FK_resource_scheduler_AspNetUsers] FOREIGN KEY ([uss_user_id]) REFERENCES [dbo].[AspNetUsers] ([Id])
);


GO
ALTER TABLE [dbo].[user_schedule] NOCHECK CONSTRAINT [FK_resource_scheduler_AspNetUsers];


GO
CREATE NONCLUSTERED INDEX [nci_wi_user_schedule_9EC5442B5C2A72D139E0F3F9CDB5E1CB]
    ON [dbo].[user_schedule]([uss_user_id] ASC)
    INCLUDE([uss_created_by], [uss_created_by_name], [uss_created_date], [uss_date], [uss_description], [uss_is_active], [uss_modified_by], [uss_modified_by_name], [uss_modified_date], [uss_time_from], [uss_time_to]);

