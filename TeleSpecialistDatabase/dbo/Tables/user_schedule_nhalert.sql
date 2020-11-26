CREATE TABLE [dbo].[user_schedule_nhalert] (
    [uss_key]              BIGINT          IDENTITY (1, 1) NOT NULL,
    [uss_user_id]          NVARCHAR (128)  NOT NULL,
    [uss_date]             DATETIME        NOT NULL,
    [uss_time_from]        BIGINT          NOT NULL,
    [uss_time_to]          BIGINT          NOT NULL,
    [uss_description]      NVARCHAR (MAX)  NULL,
    [uss_is_active]        BIT             CONSTRAINT [DF_user_schedule_nhalert_rsr_is_active] DEFAULT ((1)) NULL,
    [uss_created_by]       NVARCHAR (128)  NOT NULL,
    [uss_created_by_name]  VARCHAR (300)   NULL,
    [uss_created_date]     DATETIME        CONSTRAINT [DF_user_schedule_nhalert_uss_created_date] DEFAULT (getdate()) NOT NULL,
    [uss_modified_by]      NVARCHAR (128)  NULL,
    [uss_modified_by_name] VARCHAR (300)   NULL,
    [uss_modified_date]    DATETIME        NULL,
    [uss_time_from_calc]   DATETIME2 (7)   NULL,
    [uss_time_to_calc]     DATETIME2 (7)   NULL,
    [uss_custome_rate]     DECIMAL (18, 2) NULL,
    [uss_shift_key]        INT             NULL,
    CONSTRAINT [PK_user_schedule_nhalert] PRIMARY KEY CLUSTERED ([uss_key] ASC),
    CONSTRAINT [FK_user_schedule_nhalert_AspNetUsers] FOREIGN KEY ([uss_user_id]) REFERENCES [dbo].[AspNetUsers] ([Id])
);


GO
ALTER TABLE [dbo].[user_schedule_nhalert] NOCHECK CONSTRAINT [FK_user_schedule_nhalert_AspNetUsers];

