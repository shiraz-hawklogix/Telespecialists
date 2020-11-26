CREATE TABLE [dbo].[case_copy_log] (
    [cpy_key]                       INT              IDENTITY (1, 1) NOT NULL,
    [cpy_source_timezone]           VARCHAR (100)    NULL,
    [cpy_source_time]               DATETIME         NULL,
    [cpy_target_timezone_offset]    INT              NULL,
    [cpy_target_timezone]           VARCHAR (100)    NULL,
    [cpy_target_time]               DATETIME         NULL,
    [cpy_five9_original_stamp_time] VARCHAR (50)     NULL,
    [cpy_case_key]                  INT              NULL,
    [cpy_fac_key]                   UNIQUEIDENTIFIER NULL,
    [cpy_page_url]                  VARCHAR (500)    NULL,
    [cpy_copied_text]               NVARCHAR (MAX)   NOT NULL,
    [cpy_call_id]                   VARCHAR (500)    NULL,
    [cpy_fac_name]                  VARCHAR (500)    NULL,
    [cpy_user_agent]                VARCHAR (500)    NULL,
    [cpy_browser_name]              VARCHAR (50)     NULL,
    [cpy_created_by]                NVARCHAR (128)   NOT NULL,
    [cpy_created_by_name]           VARCHAR (300)    NOT NULL,
    [cpy_created_date_est]          DATETIME         NOT NULL,
    [cpy_is_info_refreshed]         BIT              CONSTRAINT [DF_case_copy_log_cpy_is_info_refreshed] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_case_copy_log] PRIMARY KEY CLUSTERED ([cpy_key] ASC)
);

